using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EmergenceGuardian.VapourSynthViewer {
    /// <summary>
    /// This callback is guaranteed to be raised in the same order as requested.
    /// </summary>
    public delegate void VsOutputCallback(VsFrame frame, int index, string errorMsg);

    public class VsOutput : IDisposable {
        private IntPtr scriptPtr;
        private IntPtr apiPtr;
        private IntPtr nodePtr;
        private int nodeIndex;
        public VsInvokeApi Api { get; private set; }
        private List<VsFrameStatus> queue = new List<VsFrameStatus>();
        private bool isClearingQueue;
        private SemaphoreSlim displaySemaphore = new SemaphoreSlim(1, 1);
        private DateTime displayTime = DateTime.MinValue;
        private double maxFps = 0;
        private TimeSpan maxFpsSpan;
        public delegate void ClearQueueCallback();
        private ClearQueueCallback clearQueueCallback;

        /// <summary>
        /// Occurs when a frame is done processing.
        /// </summary>
        public event EventHandler<VsFrameStatus> FrameDone;
        /// <summary>
        /// Occurs when the frame in front of the queue is ready for display.
        /// </summary>
        public event EventHandler<VsFrameStatus> FrameReady;

        // We must keep reference so that the delegate doesn't expire before being called.
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate void VsFrameDoneCallback(IntPtr userData, IntPtr frameRef, int n, IntPtr nodeRef, IntPtr errorMsg);
        private VsFrameDoneCallback getFrameAsyncCallback;
        private IntPtr getFrameAsyncCallbackPtr;

        private VsOutput() { }

        internal VsOutput(IntPtr scriptPtr, int nodeIndex) {
            getFrameAsyncCallback = GetFrameAsync_Callback;
            getFrameAsyncCallbackPtr = Marshal.GetFunctionPointerForDelegate(getFrameAsyncCallback);
            this.scriptPtr = scriptPtr;
            this.nodeIndex = nodeIndex;

            nodePtr = VsInvoke.vsscript_getOutput(scriptPtr, nodeIndex);
            if (nodePtr == IntPtr.Zero)
                throw new Exception("Failed to retrieve VapourSynth output node.");

            this.apiPtr = VsInvoke.vsscript_getVSApi2(VsInvoke.VSSCRIPT_API_VERSION);
            if (this.apiPtr == IntPtr.Zero)
                throw new Exception("Failed to get VapourSynth API.");
            this.Api = Marshal.PtrToStructure<VsInvokeApi>(this.apiPtr);
        }

        public void Dispose() {
            VsInvoke.vsscript_clearOutput(apiPtr, nodeIndex);
            Api.freeNode(nodePtr);
        }

        public double MaxFps {
            get => maxFps;
            set {
                maxFps = value;
                maxFpsSpan = TimeSpan.FromSeconds(1 / maxFps);
            }
        }

        public VsVideoInfo VideoInfo {
            get {
                var P = Api.getVideoInfo(nodePtr);
                if (P == IntPtr.Zero)
                    throw new Exception("VapourSynth failed to get GetVideoInfo.");
                var R = Marshal.PtrToStructure<VsVideoInfo>(P);
                return R;
            }
        }

        public VsFrame GetFrame(int index) {
            const int BufSize = 1024;
            IntPtr ErrorPtr = Marshal.AllocCoTaskMem(BufSize);
            IntPtr Result = Api.getFrame(index, nodePtr, ErrorPtr, BufSize);
            if (Result == IntPtr.Zero) {
                string Err = Utf8Ptr.FromUtf8Ptr(ErrorPtr, BufSize);
                throw new VsException(Err);
            } else {
                return new VsFrame(this, Result, index);
            }
        }

        public IntPtr Core {
            get {
                return VsInvoke.vsscript_getCore(scriptPtr);
            }
        }

        public int SetThreadCount(int threads) {
            return Api.setThreadCount(threads, Core);
        }

        public void GetFrameAsync(int index) {
            lock (queue) {
                queue.Add(new VsFrameStatus(index));
            }
            Api.getFrameAsync(index, nodePtr, getFrameAsyncCallbackPtr, IntPtr.Zero);
            VsFrame.RaiseRequested(index);
        }

        private void GetFrameAsync_Callback(IntPtr userData, IntPtr frameRef, int n, IntPtr nodeRef, IntPtr errorMsg) {
            VsFrame NewFrame = new VsFrame(this, frameRef, n); ;

            List<VsFrameStatus> callbackList = new List<VsFrameStatus>();
            VsFrameStatus Found = null;
            ClearQueueCallback Callback = null;
            lock (queue) {
                for (int i = 0; i < queue.Count; i++) {
                    if (queue[i].Index == n && queue[i].Frame == null) {
                        queue[i].Frame = NewFrame;
                        if (queue[i].State == VsFrameState.Requested)
                            queue[i].State = VsFrameState.Completed;
                        queue[i].Error = Utf8Ptr.FromUtf8Ptr(errorMsg);
                        Found = queue[i];
                        break;
                    }
                }
                if (Found == null)
                    throw new InvalidOperationException("GetFrameAsync_Callback received a frame not found in the processing queue.");
                if (Found.State == VsFrameState.Cancelled) {
                    Found.Frame.Dispose();
                    queue.Remove(Found);
                    if (clearQueueCallback != null && GetQueueLength(VsFrameState.Cancelled) == 0) {
                        Callback = clearQueueCallback;
                        clearQueueCallback = null;
                    } else
                        return;
                }
                while (queue.Count > 0 && queue[0].State == VsFrameState.Completed) {
                    callbackList.Add(queue[0]);
                    queue.RemoveAt(0);
                }
            }
            if (Callback != null) {
                Callback.Invoke();
                return;
            }

            // Callback outside the lock so FrameDone keeps being fired and queue can be altered.
            // Use another lock to ensure frames are displayed in the right order.
            FrameDone?.Invoke(this, Found);
            if (callbackList.Count > 0) {
                displaySemaphore.Wait();
                try {
                    // When clearing queue, this is fresh new data after the flush.
                    isClearingQueue = false;

                    foreach (VsFrameStatus item in callbackList) {
                        // Limit display frame rate.
                        if (maxFps > 0) {
                            TimeSpan FrameDelay = DateTime.Now - displayTime;
                            if (FrameDelay < maxFpsSpan)
                                Thread.Sleep(maxFpsSpan - FrameDelay);
                            displayTime = DateTime.Now;
                        }

                        // When clearing queue, some frames may still be pending in the loop. Discard them.
                        if (isClearingQueue) {
                            item.Frame.Dispose();
                        } else {
                            // Display frame.
                            FrameReady?.Invoke(this, item);
                            item.Frame.Dispose();

                            // Without this line the UI is very sluggish and this solves it.
                            if (MaxFps > 0)
                                Thread.Yield();
                            else
                                Thread.Sleep(1);
                        }
                    }
                } finally {
                    displaySemaphore.Release();
                }
            }
        }

        /// <summary>
        /// Clears the processing queue.
        /// </summary>
        /// <returns>The amount of frames cleared from the queue.</returns>
        public int ClearQueue() {
            return ClearQueue(null);
        }

        /// <summary>
        /// Clears the processing queue.
        /// </summary>
        /// <param name="callback">Calls this method after all cancelled frames are done processing.</param>
        /// <returns>The amount of frames cleared from the queue.</returns>
        public int ClearQueue(ClearQueueCallback callback) {
            int Cleared = 0;
            lock (queue) {
                // isClearingQueue exits the display loop and will be reset in GetFrameAsync_Callback
                isClearingQueue = true;
                VsFrameStatus item;
                for (int i = 0; i < queue.Count; i++) {
                    item = queue[i];
                    if (item.State == VsFrameState.Completed) {
                        item.Frame.Dispose();
                        item.Frame = null;
                        Cleared++;
                        queue.RemoveAt(i--);
                    } else if (item.State == VsFrameState.Requested) {
                        item.State = VsFrameState.Cancelled;
                        Cleared++;
                    }
                }
                if (Cleared > 0)
                    clearQueueCallback = callback;
            }
            if (Cleared == 0)
                callback?.Invoke();
            return Cleared;
        }

        /// <summary>
        /// Returns the amount of uncompleted frames in the processing queue.
        /// </summary>
        public int GetQueueLength(VsFrameState state) {
            lock (queue) {
                int Result = 0;
                for (int i = 0; i < queue.Count; i++) {
                    if (queue[i].State == state)
                        Result++;
                }
                return Result;
            }
        }
    }
}

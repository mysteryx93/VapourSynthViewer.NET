using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

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
		public VsApiInvoke Api { get; private set; }
		private List<FrameStatus> queue = new List<FrameStatus>();
		private bool isClearingQueue;
		private object displayLock = new object();
		private DateTime displayTime = DateTime.MinValue;
		private double maxFps = 0;
		private TimeSpan maxFpsSpan;
		public delegate void ClearQueueCallback();
		private ClearQueueCallback clearQueueCallback;

		/// <summary>
		/// Occurs when a frame is done processing.
		/// </summary>
		public event EventHandler<FrameStatus> FrameDone;
		/// <summary>
		/// Occurs when the frame in front of the queue is ready for display.
		/// </summary>
		public event EventHandler<FrameStatus> FrameReady;

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
			this.Api = Marshal.PtrToStructure<VsApiInvoke>(this.apiPtr);
		}

		public void Dispose() {
			VsInvoke.vsscript_clearOutput(apiPtr, nodeIndex);
			Api.freeNode(nodePtr);
		}

		public double MaxFps {
			get {
				lock (displayLock) {
					return maxFps;
				}
			}
			set {
				lock (displayLock) {
					maxFps = value;
					maxFpsSpan = TimeSpan.FromSeconds(1 / maxFps);
				}
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
				queue.Add(new FrameStatus(index));
			}
			Api.getFrameAsync(index, nodePtr, getFrameAsyncCallbackPtr, IntPtr.Zero);
			// System.Diagnostics.Debug.WriteLine("VsFrame Get {0}", index);
		}

		private void GetFrameAsync_Callback(IntPtr userData, IntPtr frameRef, int n, IntPtr nodeRef, IntPtr errorMsg) {
			List<FrameStatus> callbackList = new List<FrameStatus>();
			FrameStatus Found = null;
			ClearQueueCallback Callback = null;
			VsFrame NewFrame = new VsFrame(this, frameRef, n); ;
			lock (queue) {
				for (int i = 0; i < queue.Count; i++) {
					if (queue[i].Index == n && queue[i].Frame == null) {
						queue[i].Frame = NewFrame;
						queue[i].Error = Utf8Ptr.FromUtf8Ptr(errorMsg);
						Found = queue[i];
						break;
					}
				}
				if (Found == null)
					NewFrame.Dispose();
				if (clearQueueCallback != null && Found != null) {
					Found.Frame.Dispose();
					queue.Remove(Found);
					if (queue.Count == 0) {
						Callback = clearQueueCallback;
						clearQueueCallback = null;
					} else
						return;
				}
				while (queue.Count > 0 && queue[0].Frame != null) {
					callbackList.Add(queue[0]);
					queue.RemoveAt(0);
				}
				//if (isClearingQueue){
				//	//foreach (FrameStatus item in callbackList) {
				//	//	item.Frame.Dispose();
				//	//}
				//	//if (queue.Count == 0) {
				//	if (callbackList.Count > 0) {
				//		isClearingQueue = false;
				//		// clearQueueCallback?.Invoke();
				//	}
				//	return;
				//} 
			}
			if (Callback != null) {
				Callback.Invoke();
				return;
			}
			if (Found != null)
				FrameDone?.Invoke(this, Found);

			// Callback outside the lock so FrameDone keeps being fired and queue can be altered.
			// Use another lock to ensure frames are displayed in the right order.
			if (callbackList.Count > 0) {
				lock (displayLock) {
					// ResetClearQueue();
					foreach (FrameStatus item in callbackList) {
						// Limit display frame rate.
						if (maxFps > 0) {
							TimeSpan FrameDelay = DateTime.Now - displayTime;
							if (FrameDelay < maxFpsSpan)
								Thread.Sleep(maxFpsSpan - FrameDelay);
							displayTime = DateTime.Now;
						}

						// Prevent additional frames from rendering when clearing queue.
						//if (ResetClearQueue()) {
						//	item.Frame.Dispose();
						//	break;
						//}

						// Display frame.
						FrameReady?.Invoke(this, item);
						item.Frame.Dispose();

						// Without this line the UI is very sluggish and this solves it.
						if (maxFps > 0)
							Thread.Yield();
						else
							Thread.Sleep(1);
					}
				}
			}
		}

		/// <summary>
		/// Clears the processing queue instantly.
		/// </summary>
		/// <returns>The amount of frames cleared from the queue.</returns>
		public int ClearQueue() {
			lock (queue) {
				//if (!isClearingQueue) {
					//isClearingQueue = true;
					//clearQueueCallback = callback;
					//return queue.Count;
					int Cleared = queue.Count;
					foreach (FrameStatus item in queue) {
						if (item.Frame != null)
							item.Frame.Dispose();
					}
					queue.Clear();
					//Thread.Sleep(200);
					return Cleared;
				//} else
				//	return 0;
			}
		}

		/// <summary>
		/// Calls specified callback when all frames in the queue are done processing.
		/// </summary>
		public void ClearQueueWait(ClearQueueCallback callback) {
			lock (queue) {
				//if (!isClearingQueue) {
				// isClearingQueue = true;
				if (queue.Count > 0)
					clearQueueCallback = callback;
				else
					callback.Invoke();
				//return queue.Count;
			}
		}

		/// <summary>
		/// Called to reset the display queue.
		/// </summary>
			//private bool ResetClearQueue() {
			//	if (isClearingQueue) {
			//		isClearingQueue = false;
			//		displayTime = DateTime.MinValue;
			//		return true;
			//	} else
			//		return false;
			//}

			/// <summary>
			/// Returns the amount of uncompleted frames in the processing queue.
			/// </summary>
		public int QueueLength {
			get {
				lock (queue) {
					int Result = 0;
					for (int i = 0; i < queue.Count; i++) {
						if (queue[i].Frame == null)
							Result++;
					}
					return Result;
				}
			}
		}
	}
}

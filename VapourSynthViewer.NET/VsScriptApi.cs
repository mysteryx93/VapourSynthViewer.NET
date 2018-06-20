using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EmergenceGuardian.VapourSynthViewer {
    public class VsScriptApi : IDisposable {
        /// <summary>
        /// Loads VapourSynth DLLs from specified path. Call this before calling any other functions if DLLs can't be found by default.
        /// </summary>
        /// <param name="path">The path containing the DLLs.</param>
        public static void SetDllPath(string path) {
            VsInvoke.SetDllPath(path);
        }

        /// <summary>
        /// Automatically calls Init and Finalize.
        /// </summary>
        static VsScriptApi() {
            if (VsInvoke.vsscript_init() == 0)
                throw new Exception("Failed to initialize VapourSynth environment.");
            AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                VsInvoke.vsscript_finalize();
            };
        }

        private IntPtr scriptPtr = IntPtr.Zero;

        private VsScriptApi() {}

        private VsScriptApi(IntPtr handle) {
            this.scriptPtr = handle;
        }

        public void Dispose() {
            VsInvoke.vsscript_freeScript(scriptPtr);
        }

        public static VsScriptApi Create() {
            IntPtr H = IntPtr.Zero;
            VsInvoke.vsscript_createScript(ref H);
            return new VsScriptApi(H);
        }

        public static VsScriptApi LoadFile(string path, bool setWorkingDir) {
            IntPtr H = IntPtr.Zero;
            if (VsInvoke.vsscript_evaluateFile(ref H, new Utf8Ptr(path).ptr, setWorkingDir ? VsInvoke.FlagSetWorkingDir : 0) == 0) {
                return new VsScriptApi(H);
            } else {
                string Err = GetError(H);
                VsInvoke.vsscript_freeScript(H);
                throw new VsException(Err);
            }
        }

        public static VsScriptApi LoadScript(string script, bool setWorkingDir) {
            IntPtr H = IntPtr.Zero;
            if (VsInvoke.vsscript_evaluateScript(ref H, new Utf8Ptr(script).ptr, IntPtr.Zero, setWorkingDir ? VsInvoke.FlagSetWorkingDir : 0) == 0) {
                return new VsScriptApi(H);
            } else {
                string Err = GetError(H);
                VsInvoke.vsscript_freeScript(H);
                throw new VsException(Err);
            }
        }

        private static String GetError(IntPtr h) {
            IntPtr Err = VsInvoke.vsscript_getError(h);
            return Utf8Ptr.FromUtf8Ptr(Err);
        }

        /// <summary>
        /// Returns frame at specified position.
        /// </summary>
        public VsOutput GetOutput(int index) {
            return new VsOutput(scriptPtr, index);
        }

        public int ClearOutput(int index) {
            return VsInvoke.vsscript_clearOutput(scriptPtr, index);
        }

        public static int GetApiVersion() {
            return VsInvoke.vsscript_getApiVersion();
        }

        [DllImport("msvcrt.dll", SetLastError = false)]
        static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

        public static void BitBlt(IntPtr dstp, int dstStride, IntPtr srcp, int srcStride, int rowSize, int height) {
            if (height != 0) {
                if (srcStride == dstStride && srcStride == rowSize) {
                    memcpy(dstp, srcp, rowSize * height);
                } else {
                    int srcOff = 0, dstOff = 0;
                    for (int i = 0; i < height; i++) {
                        memcpy(IntPtr.Add(dstp, dstOff), IntPtr.Add(srcp, srcOff), rowSize);
                        srcOff += srcStride;
                        dstOff += dstOff;
                    }
                }
            }
        }
    }
}

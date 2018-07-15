using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace EmergenceGuardian.VapourSynthApi {
    public static class VsHelper  {
        private static bool isInit = false;

        /// <summary>
        /// Loads VapourSynth DLLs from specified path. Call this before calling any other functions if DLLs can't be found by default.
        /// </summary>
        /// <param name="path">The path containing the DLLs.</param>
        public static void SetDllPath(string path) {
            VsInvoke.SetDllPath(path);
        }

        /// <summary>
        /// Initializes the VapourSynth environment. It will be automatically finalized when the process exits.
        /// </summary>
        public static void Init() {
            if (!isInit) {
                // Increments on success, returns 0 on failure.
                if (VsInvoke.vsscript_init() > 0) {
                    AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                        // Decrements on success.
                        if (VsInvoke.vsscript_finalize() == 0)
                            isInit = false;
                        else
                            throw new Exception("Failed to finalize VapourSynth environment.");
                    };
                    isInit = true;
                } else
                    throw new Exception("Failed to initialize VapourSynth environment. 'vsscript.dll' could not be loaded.");
            }
        }

        /// <summary>
        /// Returns the API version of the loaded Vapoursynth DLL.
        /// </summary>
        public static int GetApiVersion() {
            return VsInvoke.vsscript_getApiVersion();
        }

        [DllImport("msvcrt.dll", SetLastError = false)]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

        /// <summary>
        /// Copies frame data from one memory location to another.
        /// </summary>
        /// <param name="dstp">The destination pointer.</param>
        /// <param name="dstStride">The destination stride.</param>
        /// <param name="srcp">The source pointer.</param>
        /// <param name="srcStride">The source stride.</param>
        /// <param name="rowSize">The frame row size.</param>
        /// <param name="height">The frame height.</param>
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

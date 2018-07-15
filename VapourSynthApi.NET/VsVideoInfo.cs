using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EmergenceGuardian.VapourSynthApi {
    [StructLayout(LayoutKind.Sequential)]
    public class VsVideoInfo {
        private IntPtr formatPtr;
        public VsFormat Format {
            get {
                if (formatPtr != IntPtr.Zero)
                    return Marshal.PtrToStructure<VsFormat>(formatPtr);
                else
                    return null;
            }
        }
        public long FpsNum;
        public long FpsDen;
        public int Width;
        public int Height;
        public int NumFrames;
        public int Flags;

        /// <summary>
        /// Checks whether the format never changes between frames.
        /// </summary>
        public bool IsConstantFormat {
            get {
                return Height > 0 && Width > 0 && formatPtr != IntPtr.Zero;
            }
        }

        /// <summary>
        /// Checks whether the two clips have the same format (unknown/changeable will be considered the same too).
        /// </summary>
        /// <param name="clip">The clip to compare to.</param>
        public bool IsSameFormat(VsVideoInfo clip) {
            return Height == clip.Height && Width == clip.Width && formatPtr == clip.formatPtr;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class VsFormat {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        private byte[] NamePtr;
        public string Name {
            get {
                return Encoding.UTF8.GetString(NamePtr).TrimEnd('\0');
            }
        }
        public int Id;
        public VsColorFamily ColorFamily;
        public VsSampleType SampleType;
        public int BitsPerSample; /* number of significant bits */
        public int BytesPerSample; /* actual storage is always in a power of 2 and the smallest possible that can fit the number of bits used per sample */
        public int SubSamplingW; /* log2 subsampling factor, applied to second and third plane */
        public int SubSamplingH;
        public int NumPlanes; /* implicit from colorFamily */
    }

    public enum VsColorFamily {
        /* all planar formats */
        Gray = 1000000,
        RGB = 2000000,
        YUV = 3000000,
        YCoCg = 4000000,
        /* special for compatibility */
        Compat = 9000000
    }

    public enum VsSampleType {
        Integer = 0,
        Float = 1
    }
}

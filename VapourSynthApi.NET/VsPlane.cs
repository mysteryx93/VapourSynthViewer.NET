using System;
using System.IO;

namespace EmergenceGuardian.VapourSynthApi {
    public class VsPlane {
        private VsOutput output;
        private IntPtr frame;
        public int Plane { get; private set; }
        private int? stride;
        private IntPtr? ptr;
        private int? width;
        private int? height;

        private VsPlane() { }
        internal VsPlane(VsOutput output, IntPtr frame, int plane) {
            this.output = output;
            this.frame = frame;
            this.Plane = plane;
        }

        public int Stride {
            get {
                if (!stride.HasValue)
                    stride = output.Api.getStride(frame, Plane);
                return stride.Value;
            }
        }

        public IntPtr Ptr {
            get {
                if (!ptr.HasValue)
                    ptr = output.Api.getReadPtr(frame, Plane);
                return ptr.Value;
            }
        }

        public int Width {
            get {
                if (!width.HasValue)
                    width = output.Api.getFrameWidth(frame, Plane);
                return width.Value;
            }
        }

        public int Height {
            get {
                if (!height.HasValue)
                    height = output.Api.getFrameHeight(frame, Plane);
                return height.Value;
            }
        }

        public IntPtr GetWritePtr() {
            return output.Api.getwritePtr(frame, Plane);
        }

        ///// <summary>
        ///// Returns the plane data as a bitmap. You MUST copy the data before display because the raw memory pointer can change.
        ///// </summary>
        ///// <returns></returns>
        //public Bitmap AsBitmap() {
        //    Bitmap Bmp = new Bitmap(Width, Height, Stride, PixelFormat.Format8bppIndexed, Ptr);
        //    ColorPalette Pal = Bmp.Palette;
        //    for (int i = 0; i < 256; i++)
        //        Pal.Entries[i] = Color.FromArgb(255, i, i, i);
        //    Bmp.Palette = Pal;
        //    return Bmp;
        //}

        //public MemoryStream AsBitmapStream() {
        //    MemoryStream Memory = new MemoryStream();
        //    Bitmap Bmp = AsBitmap();
        //    Bmp.Save(Memory, System.Drawing.Imaging.ImageFormat.Bmp);
        //    Memory.Position = 0;
        //    return Memory;
        //}

        ///// <summary>
        ///// Returns COMPATBGR32 plane data as a bitmap. You MUST copy the data before display because the raw memory pointer can change.
        ///// </summary>
        ///// <returns></returns>
        //public Bitmap AsCOMPATBGR32Bitmap() {
        //    Bitmap Bmp = new Bitmap(Width, Height, Stride, PixelFormat.Format32bppRgb, Ptr);
        //    return Bmp;
        //}

        //public MemoryStream AsCOMPATBGR32BitmapStream() {
        //    MemoryStream Memory = new MemoryStream();
        //    Bitmap Bmp = AsCOMPATBGR32Bitmap();
        //    Bmp.Save(Memory, System.Drawing.Imaging.ImageFormat.Bmp);
        //    Memory.Position = 0;
        //    return Memory;
        //}
    }
}

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EmergenceGuardian.VapourSynthViewer;

namespace WpfScriptViewer {
    [ValueConversion(typeof(VsPlane), typeof(ImageSource))]
    public class VsPlaneToImageSourceConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            VsFrame frame = value as VsFrame;
            if (frame == null)
                return null;
            VsPlane plane = frame.GetPlane(0);
            //Imaging.CreateBitmapSourceFromHBitmap(plane.Ptr, IntPtr.Zero, Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());

            var bitmapSource = BitmapSource.Create(
                plane.Width, plane.Height, 96, 96, PixelFormats.Bgr32, null,
                plane.Ptr, plane.Stride * plane.Height, plane.Stride);

            return bitmapSource;
        }

        //private static System.Windows.Media.PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat sourceFormat) {
        //    switch (sourceFormat) {
        //        case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
        //            return PixelFormats.Bgr24;

        //        case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
        //            return PixelFormats.Bgra32;

        //        case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
        //            return PixelFormats.Bgr32;

        //            // .. as many as you need...
        //    }
        //    return new System.Windows.Media.PixelFormat();
        //}

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}

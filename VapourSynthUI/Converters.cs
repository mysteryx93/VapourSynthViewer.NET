using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EmergenceGuardian.WpfExtensions;

namespace EmergenceGuardian.VapourSynthUI {
    /// <summary>
    /// Converts boolean values to Visibility while allowing to configure true and false values.
    /// </summary>
    public sealed class SquarePixelsConverter : BooleanConverter<BitmapScalingMode> {
        public SquarePixelsConverter() :
            base(BitmapScalingMode.NearestNeighbor, BitmapScalingMode.Fant) { }
    }
}

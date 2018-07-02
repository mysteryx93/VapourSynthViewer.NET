using System;
using System.Globalization;
using System.Windows.Data;
using EmergenceGuardian.WpfExtensions;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Converts a zoom value to string, where 1 = 100%, and typing 100 gives back 1. Parameter specifies how many decimals to display.
    /// </summary>
    [ValueConversion(typeof(double), typeof(string))]
    public class ZoomConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double Value = (double)value;
            int Decimals = parameter != null ? (int)parameter : 0;
            return Value.ToString("p" + Decimals.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            string Value = ((string)value).Trim();
            if (Value.EndsWith("%"))
                Value = Value.Substring(0, Value.Length - 1);

            if (double.TryParse(Value, out double Result))
                return Result / 100.0;
            else
                return null;
        }
    }

    public sealed class MouseCaptureConverter : BooleanConverter<MouseCaptureState> {
        public MouseCaptureConverter() : base(MouseCaptureState.Mouse, MouseCaptureState.None) { }
    }
}

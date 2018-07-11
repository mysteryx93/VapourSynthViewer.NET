using System;
using System.Globalization;
using System.Windows.Data;
using ICSharpCode.AvalonEdit.Highlighting;

namespace EmergenceGuardian.WpfScriptViewer {
    public class HighlightingDefinitionConverter : IValueConverter {
        private static readonly HighlightingDefinitionTypeConverter Converter = new HighlightingDefinitionTypeConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Converter.ConvertFrom(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Converter.ConvertToString(value);
        }
    }
}

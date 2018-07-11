using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Reflection;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Automatically selects text when a TextBox receives focus.
    /// </summary>
    public class SyntaxHighlight : DependencyObject {
        public static readonly DependencyProperty DefinitionProperty = DependencyProperty.RegisterAttached(
            "Definition", typeof(string), typeof(SyntaxHighlight), new PropertyMetadata(null, DefinitionPropertyChanged));

        private static void DefinitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is TextEditor P) {
                var typeConverter = new HighlightingDefinitionTypeConverter();
                P.SyntaxHighlighting = (IHighlightingDefinition)typeConverter.ConvertFrom(e.NewValue);
            }
        }

        [AttachedPropertyBrowsableForChildrenAttribute(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(TextEditor))]
        public static string GetDefinition(DependencyObject d) => (string)d.GetValue(DefinitionProperty);
        public static void SetDefinition(DependencyObject d, string value) => d.SetValue(DefinitionProperty, value);

        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached(
            "Source", typeof(string), typeof(SyntaxHighlight), new PropertyMetadata(null, SourcePropertyChanged));

        private static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is TextEditor P) {
                Assembly a = Assembly.GetExecutingAssembly();
                using (Stream stream = a.GetManifestResourceStream((string)e.NewValue)) {
                    using (XmlReader r = XmlReader.Create(stream)) {
                        P.SyntaxHighlighting = HighlightingLoader.Load(r, HighlightingManager.Instance);
                    }
                }
            }
        }

        [AttachedPropertyBrowsableForChildrenAttribute(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(TextEditor))]
        public static string GetSource(DependencyObject d) => (string)d.GetValue(SourceProperty);
        public static void SetSource(DependencyObject d, string value) => d.SetValue(SourceProperty, value);
    }
}
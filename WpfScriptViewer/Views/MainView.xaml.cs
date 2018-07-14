using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EmergenceGuardian.WpfExtensions;
using ICSharpCode.AvalonEdit;
using LinqToVisualTree;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window {
        public MainViewModel ViewModel => DataContext as MainViewModel;
        private TextEditor CurrentTabEditor => Tabs.Descendants<TextEditor>().FirstOrDefault() as TextEditor;

        public MainView() {            
            InitializeComponent();
            ViewModel.RequestClose += delegate { this.Close(); };
            ViewModel.DisplayScript += async (s, e) => {
                await Task.Yield();
                CurrentTabEditor.Text = e.Script;
            };
            ViewModel.UpdateScript += (s, e) => e.Script = CurrentTabEditor.Text;
            Closing += delegate {
                ViewModelLocator.Cleanup();
                Environment.Exit(0);
            };
        }

        /// <summary>
        /// Keys 0-9 allow switching between viewer tabs but this also handles those keys for the editor textbox. Allow those keys to pass through.
        /// </summary>
        public void Editor_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.None && e.Key >= Key.D0 && e.Key <= Key.D9) {
                string KeyText = new KeyConverter().ConvertToString(e.Key);
                ((TextEditor)sender).TextArea.PerformTextInput(KeyText);
            }
        }
    }
}

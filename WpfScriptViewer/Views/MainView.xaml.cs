using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    }
}

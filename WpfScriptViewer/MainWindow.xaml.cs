using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EmergenceGuardian.VapourSynthViewer;
using EmergenceGuardian.VapourSynthUI;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static void Instance(MainWindowViewModel viewModel) {
            MainWindow F = new MainWindow();
            F.Show();
        }

        public MainWindow() {
            InitializeComponent();
        }

        public MainWindowViewModel ViewModel => FindResource("ViewModel") as MainWindowViewModel;

        private void ViewModel_RequestClose(object sender, EventArgs e) => this.Close();

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (!DesignerProperties.GetIsInDesignMode(this))
                ReadScriptFile(@"C:\GitHub\VapourSynthViewer.NET\test.vpy");

            // Player.Host.SetDllPath(@"C:\Program Files (x86)\VapourSynth\core64");
            // Player.Host.Load(@"C:\GitHub\VapourSynthViewer.NET\test.vpy");
        }

        private void ReadScriptFile(string file) {
            try {
                //ScriptText.Text = File.ReadAllText(file);
            } catch { }
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            Environment.Exit(0);
        }

        private T FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement {
            T child = default(T);
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                var ch = VisualTreeHelper.GetChild(parent, i);
                child = ch as T;
                if (child != null && child.Name == name)
                    break;
                else
                    child = FindVisualChildByName<T>(ch, name);

                if (child != null) break;
            }
            return child;
        }
    }
}

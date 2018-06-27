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

        private void ViewModel_RequestClose(object sender, EventArgs e) => this.Close();

        private void Window_Loaded(object sender, RoutedEventArgs e) {
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

        private void TabViewer_Selected(object sender, RoutedEventArgs e) {
            //await Task.Yield(); // Give time to create player control.
            //TabViewer.DataContext = ScriptText.Text;
            // PlayerHost.Script = ScriptText.Text;
        }

        private void TabViewer_Unselected(object sender, RoutedEventArgs e) {
            //Player.Host.Stop();
            //await Task.Delay(100);
            //await Player.Host.UnloadScript();
        }

    }
}

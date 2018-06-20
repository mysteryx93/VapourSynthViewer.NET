using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

namespace WpfScriptViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        //int Pos;
        //int PosRequested;
        //VsScriptApi ScriptApi;
        //VsVideoInfo Vi;
        //VsOutput output;
        //VsFormat Format;
        //int threads = 1;
        //bool playing = false;

        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
			//RunScript();
			Player.Host.SetDllPath(@"C:\Program Files (x86)\VapourSynth\core64");
			Player.Host.LimitFps = true;
			Player.Host.Load(@"C:\GitHub\VapourSynth Viewer .NET\test.vpy");
		}
	}
}

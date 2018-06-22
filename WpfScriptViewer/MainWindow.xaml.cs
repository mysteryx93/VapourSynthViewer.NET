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

        List<int> Frames = new List<int>();

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            //await RunTest();

            Player.Host.SetDllPath(@"C:\Program Files (x86)\VapourSynth\core64");
            Player.Host.LimitFps = true;
            VsFrame.Requested += (s, f) => {
                Frames.Add(f);
            };
            VsFrame.Deallocated += (s, f) => {
                for (int i = 0; i < Frames.Count; i++) {
                    if (Frames[i] == f.Index) {
                        Frames.RemoveAt(i);
                        return;
                    }
                }
            };

            Player.Host.Load(@"C:\GitHub\VapourSynthViewer.NET\test.vpy");
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            Environment.Exit(0);
        }
    }
}

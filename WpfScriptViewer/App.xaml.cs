using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            string OpenFile = e.Args.FirstOrDefault();
            WpfScriptViewer.MainView.Instance(OpenFile);
        }
    }
}

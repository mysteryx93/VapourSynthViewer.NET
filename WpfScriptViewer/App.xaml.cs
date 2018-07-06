using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Ioc;
using EmergenceGuardian.WpfExtensions;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        static App() {
            DispatcherHelper.Initialize();
        }

        //protected override void OnStartup(StartupEventArgs e) {
        //    base.OnStartup(e);

        //    MainViewModel ViewModel = ViewModelLocator.Instance.Main;
        //    if (e.Args.Any())  // Load file from command line.
        //        if (!ViewModel.ReadScriptFile(e.Args[0]))
        //            return;
        //    MainView NewForm = new MainView();
        //    NewForm.DataContext = ViewModel;
        //    NewForm.Show();
        //    // ViewModelLocator.Instance.DialogService.Show(null, ViewModel);
        //}
    }
}

using GalaSoft.MvvmLight.Threading;
using System.Windows;

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

using System;
using System.ComponentModel;
using System.Windows;
using EmergenceGuardian.WpfExtensions;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window {
        public MainViewModel ViewModel => DataContext as MainViewModel;

        public MainView() {            
            InitializeComponent();
            ViewModel.RequestClose += delegate { this.Close(); };
            Closing += delegate {
                ViewModelLocator.Cleanup();
                Environment.Exit(0);
            };
        }

        private void UI_Loaded(object sender, RoutedEventArgs e) {

        }
    }
}

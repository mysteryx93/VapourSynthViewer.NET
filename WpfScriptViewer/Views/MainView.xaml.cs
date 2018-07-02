using System;
using System.ComponentModel;
using System.Windows;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window {
        public static void Instance(string scriptFile) {
            MainView F = new MainView();
            if (!string.IsNullOrEmpty(scriptFile))
                F.ViewModel.ReadScriptFile(scriptFile);
            F.Show();
        }

        public MainView() {
            InitializeComponent();
            ViewModel = FindResource("ViewModel") as MainViewModel;
            ViewModel.MessageBox.MessageBoxRequest += MessageBox_MessageBoxRequest;
        }

        public MainViewModel ViewModel { get; private set; }
        private void ViewModel_RequestClose(object sender, EventArgs e) => this.Close();
        private void MessageBox_MessageBoxRequest(object sender, WpfFramework.Mvvm.MvvmMessageBoxEventArgs e) => e.Show();

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);
            Environment.Exit(0);
        }
    }
}

using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace EmergenceGuardian.WpfScriptViewer {
    /// <summary>
    /// Interaction logic for InputView.xaml
    /// </summary>
    public partial class InputView : Window {
        public IInputViewModel ViewModel => DataContext as IInputViewModel;

        public InputView() {
            InitializeComponent();
            ViewModel.RequestClose += delegate { this.Close(); };
        }
    }
}

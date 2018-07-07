using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using MvvmDialogs;
using EmergenceGuardian.WpfExtensions;

namespace EmergenceGuardian.WpfScriptViewer {
    public class DesignHelpViewModel : ViewModelBase, IHelpViewModel, IModalDialogViewModel {
        public DesignHelpViewModel() { }

        public bool? DialogResult => true;

        public string AppVersion => "1.0.0";
    }
}

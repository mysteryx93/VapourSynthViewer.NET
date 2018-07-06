using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;
using EmergenceGuardian.WpfExtensions;

namespace EmergenceGuardian.WpfScriptViewer {
    public interface IHelpViewModel : IModalDialogViewModel {
        string AppVersion { get; }
    }

    public class HelpViewModel : ViewModelBase, IHelpViewModel {
        public HelpViewModel() { }

        [PreferredConstructor()]
        public HelpViewModel(IEnvironmentService environmentService) {
            this.environmentService = environmentService;
        }

        private IEnvironmentService environmentService;

        public bool? DialogResult => true;

        public string AppVersion => environmentService.AppVersion.ToString(3);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace EmergenceGuardian.WpfScriptViewer {
    public class WorkspaceViewModel : ViewModelBase {
        public WorkspaceViewModel() { }

        public WorkspaceViewModel(string displayName, bool canClose) {
            this.DisplayName = displayName;
            this.CanClose = canClose;
        }

        private string displayName;
        private bool canClose = true;
        public event EventHandler RequestClose;

        private ICommand closeCommand;
        public ICommand CloseCommand => InitCommand(ref closeCommand, OnRequestClose, () => CanClose);

        public string DisplayName {
            get => displayName;
            set {
                displayName = value;
                RaisePropertyChanged("DisplayName");
            }
        }

        public bool CanClose {
            get => canClose;
            set {
                canClose = value;
                RaisePropertyChanged("CanClose");
            }
        }

        public virtual void OnRequestClose() {
            RequestClose?.Invoke(this, new EventArgs());
        }
    }
}

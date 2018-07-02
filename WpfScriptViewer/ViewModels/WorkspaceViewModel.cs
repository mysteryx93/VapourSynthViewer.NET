using System;
using EmergenceGuardian.WpfFramework.Mvvm;

namespace EmergenceGuardian.WpfScriptViewer {
    public class WorkspaceViewModel : ObserveableObject {
        public WorkspaceViewModel() { }

        public WorkspaceViewModel(string displayName, bool canClose) {
            this.DisplayName = displayName;
            this.CanClose = canClose;
        }

        private string displayName;
        private bool canClose = true;
        public event EventHandler RequestClose;

        private DelegateCommand closeCommand;
        public DelegateCommand CloseCommand => InitCommand(ref closeCommand, OnRequestClose, () => CanClose);

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

using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EmergenceGuardian.WpfScriptViewer {
    public class WorkspaceViewModel : ObservableObject {
        public WorkspaceViewModel() { }

        public WorkspaceViewModel(string displayName, bool canClose) {
            this.DisplayName = displayName;
            this.CanClose = canClose;
        }

        private string displayName;
        private bool canClose = true;
        public event EventHandler RequestClose;

        private RelayCommand closeCommand;
        public RelayCommand CloseCommand => this.InitCommand(ref closeCommand, OnRequestClose, () => CanClose);

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

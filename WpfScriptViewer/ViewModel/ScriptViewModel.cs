using GalaSoft.MvvmLight.Command;
using System;

namespace EmergenceGuardian.WpfScriptViewer {
    public class ScriptViewModel : WorkspaceViewModel {
        private string script;
        private bool isEditingHeader = false;
        public bool CanEditHeader { get; protected set; } = false;

        public ScriptViewModel() { }
        public ScriptViewModel(string displayName, bool canClose) : base(displayName, canClose) { }

        public string Script {
            get => script;
            set {
                script = value;
                RaisePropertyChanged("Script");
            }
        }

        public bool IsEditingHeader {
            get => isEditingHeader;
            set {
                isEditingHeader = value;
                RaisePropertyChanged("IsEditingHeader");
            }
        }

        private RelayCommand headerEditDoneCommand;
        public RelayCommand HeaderEditDoneCommand => this.InitCommand(ref headerEditDoneCommand, OnHeaderEditDone, CanHeaderEditDone);

        private bool CanHeaderEditDone() => IsEditingHeader;
        private void OnHeaderEditDone() => IsEditingHeader = false;
    }

    public class EditorViewModel : ScriptViewModel {
        public EditorViewModel() { }
        public EditorViewModel(string displayName) : base(displayName, false) { }
    }
    public class ViewerViewModel : ScriptViewModel {
        private TimeSpan position;
        private string errorMessage;

        public ViewerViewModel() {
            CanEditHeader = true;
        }
        public ViewerViewModel(string displayName, string script) : base(displayName, true) {
            this.Script = script;
            CanEditHeader = true;
        }

        public TimeSpan Position {
            get => position;
            set {
                position = value;
                RaisePropertyChanged("Position");
            }
        }

        public string ErrorMessage {
            get => errorMessage;
            set {
                errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }
    }
    public class RunViewModel : ScriptViewModel {
        public RunViewModel() { }
        public RunViewModel(string displayName) : base(displayName, false) { }
    }
}

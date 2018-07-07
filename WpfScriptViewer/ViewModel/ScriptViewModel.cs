using GalaSoft.MvvmLight.Command;
using System;

namespace EmergenceGuardian.WpfScriptViewer {
    public interface IScriptViewModel : IWorkspaceViewModel {
        bool CanEditHeader { get; }
        string Script { get; set; }
        bool IsEditingHeader { get; set; }
        RelayCommand HeaderEditDoneCommand { get; }
    }

    public class ScriptViewModel : WorkspaceViewModel, IScriptViewModel {
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

    public interface IEditorViewModel : IScriptViewModel { }
    public class EditorViewModel : ScriptViewModel, IEditorViewModel {
        public EditorViewModel() {
            CanClose = false;
            DisplayName = "Script";
        }
    }

    public interface IViewerViewModel : IScriptViewModel {
        TimeSpan Position { get; set; }
        string ErrorMessage { get; set; }
    }
    public class ViewerViewModel : ScriptViewModel, IViewerViewModel {
        private TimeSpan position;
        private string errorMessage;

        public ViewerViewModel() {
            CanClose = true;
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

    public interface IRunViewModel : IScriptViewModel { }
    public class RunViewModel : ScriptViewModel, IRunViewModel {
        public RunViewModel() {
            CanClose = false;
            DisplayName = "Run";
        }
    }
}

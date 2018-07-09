using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows;

namespace EmergenceGuardian.WpfScriptViewer {
    public interface IScriptViewModel : IWorkspaceViewModel {
        bool CanEditHeader { get; }
        string Script { get; set; }
        bool IsEditingHeader { get; set; }
        RelayCommand HeaderEditDoneCommand { get; }
        int Sort { get; }
        int Index { get; set; }
    }

    public class ScriptViewModel : WorkspaceViewModel, IScriptViewModel {
        private string script;
        private bool isEditingHeader = false;
        public bool CanEditHeader { get; protected set; } = true;
        private int index;

        public ScriptViewModel() { }
        public ScriptViewModel(string displayName, bool canClose) : base(displayName, canClose) { }

        public string Script {
            get => script;
            set => Set<string>(() => Script, ref script, value);
        }

        public bool IsEditingHeader {
            get => isEditingHeader;
            set => Set<bool>(() => IsEditingHeader, ref isEditingHeader, value);
        }

        /// <summary>
        /// Allows sorting the list of ScriptViewModel classes.
        /// </summary>
        public int Sort { get; protected set; }

        public int Index {
            get => index;
            set => Set<int>(() => Index, ref index, value);
        }

        private RelayCommand headerEditDoneCommand;
        public RelayCommand HeaderEditDoneCommand => this.InitCommand(ref headerEditDoneCommand, OnHeaderEditDone, CanHeaderEditDone);

        private bool CanHeaderEditDone() => IsEditingHeader;
        private void OnHeaderEditDone() => IsEditingHeader = false;
    }

    public interface IEditorViewModel : IScriptViewModel {
        string FileName { get; set; }
    }
    public class EditorViewModel : ScriptViewModel, IEditorViewModel {
        public EditorViewModel() {
            CanClose = true;
            DisplayName = "Script";
            Sort = 0;
        }

        private string fileName;
        public string FileName {
            get => fileName;
            set => Set<string>(() => FileName, ref fileName, value);
        }
    }

    public interface IViewerViewModel : IScriptViewModel {
        TimeSpan Position { get; set; }
        TimeSpan Duration { get; set; }
        string ErrorMessage { get; set; }
        double ScrollHorizontalOffset { get; set; }
        double ScrollVerticalOffset { get; set; }
    }
    public class ViewerViewModel : ScriptViewModel, IViewerViewModel {
        private string errorMessage;
        private TimeSpan position;
        private TimeSpan duration;
        private double scrollVerticalOffset;
        private double scrollHorizontalOffset;

        public ViewerViewModel() {
            CanClose = true;
            Sort = 1;
        }

        public string ErrorMessage {
            get => errorMessage;
            set => Set<string>(() => ErrorMessage, ref errorMessage, value);
        }

        public TimeSpan Position {
            get => position;
            set => Set<TimeSpan>(() => Position, ref position, value);
        }

        public TimeSpan Duration {
            get => duration;
            set => Set<TimeSpan>(() => Duration, ref duration, value);
        }

        public double ScrollHorizontalOffset {
            get => scrollHorizontalOffset;
            set => Set<double>(() => ScrollHorizontalOffset, ref scrollHorizontalOffset, value);
        }

        public double ScrollVerticalOffset {
            get => scrollVerticalOffset;
            set => Set<double>(() => ScrollVerticalOffset, ref scrollVerticalOffset, value);
        }
    }
}

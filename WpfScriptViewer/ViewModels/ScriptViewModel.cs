using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.WpfScriptViewer {
    public class ScriptViewModel : WorkspaceViewModel {
        private string script;
        private TimeSpan position;

        public ScriptViewModel() { }
        public ScriptViewModel(string displayName, bool canClose) : base(displayName, canClose) { }

        public string Script {
            get => script;
            set {
                script = value;
                RaisePropertyChanged("Script");
            }
        }

        public TimeSpan Position {
            get => position;
            set {
                position = value;
                RaisePropertyChanged("Position");
            }
        }
    }

    public class EditorViewModel : ScriptViewModel {
        public EditorViewModel() { }
        public EditorViewModel(string displayName) : base(displayName, false) { }
    }
    public class ViewerViewModel : ScriptViewModel {
        public ViewerViewModel() { }
        public ViewerViewModel(string displayName) : base(displayName, true) { }
    }
    public class RunViewModel : ScriptViewModel {
        public RunViewModel() { }
        public RunViewModel(string displayName) : base(displayName, false) { }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmergenceGuardian.WpfScriptViewer {
    public class MainWindowViewModel : WorkspaceViewModel {
        public ObservableCollection<ScriptViewModel> ScriptList { get; private set; } = new ObservableCollection<ScriptViewModel>();
        public EditorViewModel EditorModel { get; private set; } = new EditorViewModel("Script");
        public RunViewModel RunModel { get; private set; } = new RunViewModel("Run");
        public 
        PropertyObserver<ScriptViewModel> Observer;

        private ScriptViewModel selectedItem;
        public ScriptViewModel SelectedItem {
            get => selectedItem;
            set {
                selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                TabSelectionChanged();
            }
        }

        public MainWindowViewModel() {
            ScriptList.Add(EditorModel);
            ScriptList.Add(RunModel);

            ReadScriptFile(@"C:\GitHub\VapourSynthViewer.NET\test.vpy");
        }

        private void TabSelectionChanged() {
            if (SelectedItem == RunModel) {
                if (CanRun())
                    OnRun();
                else
                    SelectedItem = EditorModel;
            }
        }

        /// <summary>
        /// Returns the script being edited.
        /// </summary>
        public string ScriptEdit {
            get => ScriptList.First().Script;
            set => ScriptList.First().Script = value;
        }

        private ICommand runCommand;
        public ICommand RunCommand => InitCommand(ref runCommand, OnRun, CanRun);

        private void OnRun() {
            // If a viewer already has same script, activate it.
            string Script = ScriptEdit;
            for (int i = 1; i < ScriptList.Count - 1; i++) {
                if (ScriptList[i].Script == Script) {
                    SelectedItem = ScriptList[i];
                    return;
                }
            }
            // Otherwise, create new viewer. Insert before Run tab.
            ScriptViewModel Viewer = new ViewerViewModel("Viewer");
            Viewer.Script = Script;
            Viewer.RequestClose += Viewer_RequestClose;
            ScriptList.Insert(ScriptList.Count - 1, Viewer);
            SelectedItem = Viewer;
        }

        private void Viewer_RequestClose(object sender, EventArgs e) {
            // Remove and select previous tab.
            int Pos = ScriptList.IndexOf(sender as ScriptViewModel);
            SelectedItem = ScriptList[Pos - 1];
            ScriptList.RemoveAt(Pos);
        }

        private bool CanRun() {
            return !string.IsNullOrWhiteSpace(ScriptList.First().Script);
        }

        public void ReadScriptFile(string file) {
            try {
                ScriptEdit = File.ReadAllText(file);
            } catch { }
        }
    }
}

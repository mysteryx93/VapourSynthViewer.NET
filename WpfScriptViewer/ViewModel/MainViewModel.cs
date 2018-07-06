using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EmergenceGuardian.WpfExtensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;

namespace EmergenceGuardian.WpfScriptViewer {
    public class MainViewModel : WorkspaceViewModel {

        #region Declarations / Constructor

        public MainViewModel() {
            ScriptList.Add(editorModel);
            ScriptList.Add(runModel);
            ScriptList.CollectionChanged += delegate { updateAllCommand?.RaiseCanExecuteChanged(); };
        }

        [PreferredConstructor]
        public MainViewModel(IDialogService modalDialogService, IEnvironmentService environmentService):this() {
            this.dialogService = modalDialogService;
            this.environmentService = environmentService;
        }

        private readonly IDialogService dialogService;
        private readonly IEnvironmentService environmentService;
        
        public ObservableCollection<ScriptViewModel> ScriptList { get; private set; } = new ObservableCollection<ScriptViewModel>();
        private readonly EditorViewModel editorModel = new EditorViewModel("Script");
        private readonly RunViewModel runModel = new RunViewModel("Run");

        private double scrollVerticalOffset;
        private double scrollHorizontalOffset;
        private ScriptViewModel selectedItem;
        private TimeSpan playerPosition;
        private double zoom = 1;
        private ObservableCollection<string> zoomList;
        private bool isMultiThreaded = false;
        private int autoIndex = 0;

        // These are bound to the UI.
        public double MinZoom { get; } = 0.1;
        public double MaxZoom { get; } = 10;
        public double ZoomIncrement { get; } = 1.2;

        #endregion


        #region Properties

        public double ScrollVerticalOffset {
            get => scrollVerticalOffset;
            set => Set<double>(() => ScrollVerticalOffset, ref scrollVerticalOffset, value);
        }

        public double ScrollHorizontalOffset {
            get => scrollHorizontalOffset;
            set => Set<double>(() => ScrollHorizontalOffset, ref scrollHorizontalOffset, value);
        }

        public ScriptViewModel SelectedItem {
            get => selectedItem;
            set {
                // If we had a viewer tab selected, keep its position for other tabs.
                if (selectedItem is ViewerViewModel oldViewer && oldViewer.ErrorMessage == null)
                    playerPosition = oldViewer.Position;
                if (Set<ScriptViewModel>(() => SelectedItem, ref selectedItem, value) && value is ViewerViewModel newViewer)
                    newViewer.Position = newViewer.ErrorMessage == null ? playerPosition : TimeSpan.Zero;
            }
        }

        public TimeSpan PlayerPosition {
            get => playerPosition;
            set {
                if (Set<TimeSpan>(() => PlayerPosition, ref playerPosition, value) && SelectedItem is ViewerViewModel viewer)
                    viewer.Position = value;
            }
        }

        public double Zoom {
            get => zoom;
            set {
                value = Math.Max(value, MinZoom);
                value = Math.Min(value, MaxZoom);
                Set<double>(() => Zoom, ref zoom, value);
            }
        }

        public ObservableCollection<string> ZoomList {
            get {
                if (zoomList == null)
                    zoomList = new ObservableCollection<string> { "20%", "50%", "70%", "100%", "150%", "200%", "400%" };
                return zoomList;
            }
        }

        /// <summary>
        /// Gets or sets whether to enable multi-threaded mode.
        /// </summary>
        public bool IsMultiThreaded {
            get => isMultiThreaded;
            set {
                if (Set<bool>(() => IsMultiThreaded, ref isMultiThreaded, value))
                    RaisePropertyChanged("Threads");
            }
        }

        /// <summary>
        /// Returns the amount of threads over which to run each script. 0 means ProcessorCouunt.
        /// </summary>
        public int Threads => IsMultiThreaded ? 0 : 1;

        #endregion


        #region Commands

        private RelayCommand runCommand;
        public RelayCommand RunCommand => this.InitCommand(ref runCommand, OnRun, CanRun);

        private bool CanRun() => !string.IsNullOrWhiteSpace(ScriptList.First().Script);
        private void OnRun() {
            // If a viewer already has same script, activate it.
            string Script = editorModel.Script;
            for (int i = 1; i < ScriptList.Count - 1; i++) {
                if (ScriptList[i].Script == Script) {
                    SelectedItem = ScriptList[i];
                    return;
                }
            }
            // Otherwise, create new viewer. Insert before Run tab.
            ScriptViewModel Viewer = new ViewerViewModel("Viewer " + ++autoIndex, Script);
            Viewer.RequestClose += Viewer_RequestClose;
            ScriptList.Insert(ScriptList.Count - 1, Viewer);
            SelectedItem = Viewer;
        }

        private RelayCommand updateAllCommand;
        public RelayCommand UpdateAllCommand => this.InitCommand(ref updateAllCommand, OnUpdateAll, CanUpdateAll);

        private bool CanUpdateAll() => ScriptList.OfType<ViewerViewModel>().Count() > 1;
        private void OnUpdateAll() {
            if (SelectedItem is ViewerViewModel viewer)
                playerPosition = viewer.Position;
            foreach (ViewerViewModel item in ScriptList.OfType<ViewerViewModel>()) {
                if (item != SelectedItem)
                    item.Position = playerPosition;
            }
        }

        private RelayCommand zoomInCommand;
        public RelayCommand ZoomInCommand => this.InitCommand(ref zoomInCommand, OnZoomIn, CanZoomIn);

        private bool CanZoomIn() => true;
        private void OnZoomIn() {
            Zoom *= ZoomIncrement;
        }

        private RelayCommand zoomOutCommand;
        public RelayCommand ZoomOutCommand => this.InitCommand(ref zoomOutCommand, OnZoomOut, CanZoomOut);

        private bool CanZoomOut() => true;
        private void OnZoomOut() {
            Zoom /= ZoomIncrement;
        }

        private RelayCommand helpCommand;
        public RelayCommand HelpCommand => this.InitCommand(ref helpCommand, OnHelp, CanHelp);

        private bool CanHelp() => true;
        private void OnHelp() {
            dialogService.ShowDialog(this, ViewModelLocator.Instance.Help);
        }

        #endregion


        public void Window_Loaded() {
            // Load script file from command-line.
            if (environmentService.CommandLineArguments.Count() > 1)
                ReadScriptFile(environmentService.CommandLineArguments.ElementAt(1));
        }

        public void Header_PreviewLeftMouseButtonDown(ScriptViewModel sender, MouseButtonEventArgs e) {
            if (sender == SelectedItem && sender.CanEditHeader && !sender.IsEditingHeader) {
                sender.IsEditingHeader = true;
                e.Handled = true;
            }
        }

        public void Tabs_SelectionChanged() {
            if (SelectedItem == runModel) {
                if (CanRun())
                    OnRun();
                else
                    SelectedItem = editorModel;
            }
        }

        public void Viewer_MediaLoaded() {
            if (selectedItem is ViewerViewModel viewer)
                viewer.Position = playerPosition;
        }

        private void Viewer_RequestClose(object sender, EventArgs e) {
            ScriptViewModel Model = sender as ScriptViewModel;
            Model.Script = null;
            // Remove and select previous tab.
            int Pos = ScriptList.IndexOf(Model);
            SelectedItem = ScriptList[Pos - 1];
            ScriptList.RemoveAt(Pos);
        }

        public bool ReadScriptFile(string file) {
            try {
                editorModel.Script = File.ReadAllText(file);
                return true;
            } catch (Exception ex) {
                dialogService.ShowMessageBox(this, ex.Message, "Error loading file");
                if (CloseCommand.CanExecute(null))
                    CloseCommand.Execute(null);
                return false;
            }
        }
    }
}

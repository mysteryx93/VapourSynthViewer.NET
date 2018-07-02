using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using EmergenceGuardian.WpfExtensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace EmergenceGuardian.WpfScriptViewer {
    public class MainViewModel : WorkspaceViewModel {

        #region Declarations / Constructor

        public MainViewModel() {
            ScriptList.Add(editorModel);
            ScriptList.Add(runModel);
            ScriptList.CollectionChanged += delegate { updateAllCommand?.RaiseCanExecuteChanged(); };
        }

        public MessageBoxManager MessageBox { get; } = new MessageBoxManager();
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
            set {
                scrollVerticalOffset = value;
                RaisePropertyChanged("ScrollVerticalOffset");
            }
        }

        public double ScrollHorizontalOffset {
            get => scrollHorizontalOffset;
            set {
                scrollHorizontalOffset = value;
                RaisePropertyChanged("ScrollHorizontalOffset");
            }
        }

        public ScriptViewModel SelectedItem {
            get => selectedItem;
            set {
                // If we had a viewer tab selected, keep its position for other tabs.
                if (selectedItem is ViewerViewModel) {
                    if (selectedItem.ErrorMessage == null)
                        playerPosition = selectedItem.Position;
                }
                if (value is ViewerViewModel) {
                    if (value.ErrorMessage == null)
                        value.Position = playerPosition;
                    else
                        value.Position = TimeSpan.Zero;
                }

                selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                // TabSelectionChanged();
            }
        }

        public TimeSpan PlayerPosition {
            get => playerPosition;
            set {
                playerPosition = value;
                if (SelectedItem is ViewerViewModel)
                    (SelectedItem as ViewerViewModel).Position = value;
            }
        }

        public double Zoom {
            get => zoom;
            set {
                value = Math.Max(value, MinZoom);
                value = Math.Min(value, MaxZoom);
                zoom = value;
                RaisePropertyChanged("Zoom");
            }
        }

        public ObservableCollection<string> ZoomList {
            get {
                if (zoomList == null) {
                    zoomList = new ObservableCollection<string> {
                        "20%", "50%", "70%", "100%", "150%", "200%", "400%"
                    };
                }
                return zoomList;
            }
        }

        /// <summary>
        /// Gets or sets whether to enable multi-threaded mode.
        /// </summary>
        public bool IsMultiThreaded {
            get => isMultiThreaded;
            set {
                isMultiThreaded = value;
                RaisePropertyChanged("IsMultiThreaded");
                RaisePropertyChanged("Threads");
            }
        }

        /// <summary>
        /// Returns the amount of threads over which to run each script. 0 means ProcessorCouunt.
        /// </summary>
        public int Threads {
            get => IsMultiThreaded ? 0 : 1;
        }

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
            playerPosition = SelectedItem.Position;
            foreach (ScriptViewModel item in ScriptList) {
                if (item is ViewerViewModel && item != SelectedItem)
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

        #endregion


        public void Header_PreviewLeftMouseButtonDown(ScriptViewModel sender, MouseButtonEventArgs e) {
            if (sender == SelectedItem && sender.CanEditHeader) {
                if (!sender.IsEditingHeader) {
                    sender.IsEditingHeader = true;
                    e.Handled = true;
                }
            }
        }

        public void Header_PreviewKeyDown(ScriptViewModel sender, KeyEventArgs e) {
            if (e.Key == Key.Escape || e.Key == Key.Enter || e.Key == Key.Tab) {
                (sender as ScriptViewModel).IsEditingHeader = false;
                e.Handled = true;
            }
        }

        public void Tabs_SelectionChanged(SelectionChangedEventArgs e) {
            if (SelectedItem == runModel) {
                if (CanRun())
                    OnRun();
                else
                    SelectedItem = editorModel;
            }
        }

        public void Viewer_MediaLoaded() {
            if (selectedItem is ViewerViewModel) {
                SelectedItem.Position = playerPosition;
            }
        }

        private void Viewer_RequestClose(object sender, EventArgs e) {
            ScriptViewModel Model = sender as ScriptViewModel;
            Model.Script = null;
            // Remove and select previous tab.
            int Pos = ScriptList.IndexOf(Model);
            SelectedItem = ScriptList[Pos - 1];
            ScriptList.RemoveAt(Pos);
        }

        public void ReadScriptFile(string file) {
            try {
                editorModel.Script = File.ReadAllText(file);
            } catch (Exception ex) {
                MessageBox.Show(null, ex.Message, "Error loading file");
                CloseCommand.Execute(null);
            }
        }
    }
}

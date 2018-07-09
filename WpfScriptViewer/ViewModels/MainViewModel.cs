using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EmergenceGuardian.WpfExtensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using GalaSoft.MvvmLight.Threading;

namespace EmergenceGuardian.WpfScriptViewer {
    public class MainViewModel : WorkspaceViewModel {

        #region Declarations / Constructor

        public MainViewModel() {
        }

        [PreferredConstructor]
        public MainViewModel(IDialogService modalDialogService, IEnvironmentService environmentService) : this() {
            this.dialogService = modalDialogService;
            this.environmentService = environmentService;

            // Load script file from command-line.
            bool IsFirst = true; // First is application name.
            foreach (string arg in environmentService.CommandLineArguments) {
                if (IsFirst)
                    IsFirst = false;
                else
                    ReadScriptFile(arg);
            }

            if (!ScriptList.Any())
                OnNew();

            ScriptList.CollectionChanged += delegate { updateAllCommand?.RaiseCanExecuteChanged(); };
        }

        private readonly IDialogService dialogService;
        private readonly IEnvironmentService environmentService;

        public ObservableCollection<IScriptViewModel> ScriptList { get; private set; } = new ObservableCollection<IScriptViewModel>();

        private double scrollHorizontalOffset;
        private double scrollVerticalOffset;
        private IScriptViewModel selectedItem;
        private TimeSpan playerPosition;
        private double zoom = 1;
        private ObservableCollection<string> zoomList;
        private bool isMultiThreaded = false;
        private int editorIndex = 0;
        private int viewerIndex = 0;
        private const string FileFilter = "VapourSynth Script|*.vpy|All files|*.*";

        // These are bound to the UI.
        public double MinZoom { get; } = 0.1;
        public double MaxZoom { get; } = 10;
        public double ZoomIncrement { get; } = 1.2;

        #endregion


        #region Properties

        public IScriptViewModel SelectedItem {
            get => selectedItem;
            set {
                // If we had a viewer tab selected, keep its position for other tabs.
                if (selectedItem is ViewerViewModel oldViewer && oldViewer.ErrorMessage == null) {
                    playerPosition = oldViewer.Position;
                    // If we bind scroll directly to MainViewModel, when there are several tabs and we zoom, 
                    // zoom can change in 3 tabs and cumulate the effect on the scroll. We instead bind scroll on ScriptViewModel.
                    scrollHorizontalOffset = oldViewer.ScrollHorizontalOffset;
                    scrollVerticalOffset = oldViewer.ScrollVerticalOffset;
                }
                if (Set<IScriptViewModel>(() => SelectedItem, ref selectedItem, value) && value is IViewerViewModel newViewer) {
                    if (newViewer.ErrorMessage == null) {
                        newViewer.Position = playerPosition;
                        newViewer.ScrollHorizontalOffset = scrollHorizontalOffset;
                        newViewer.ScrollVerticalOffset = scrollVerticalOffset;
                    } else
                        newViewer.Position = TimeSpan.Zero;
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public TimeSpan PlayerPosition {
            get => playerPosition;
            set {
                if (Set<TimeSpan>(() => PlayerPosition, ref playerPosition, value) && SelectedItem is IViewerViewModel viewer)
                    viewer.Position = value;
            }
        }

        public double Zoom {
            get => zoom;
            set {
                ZoomScaleToFit = value == 0.0;
                if (value != 0.0) {
                    value = Math.Max(value, MinZoom);
                    value = Math.Min(value, MaxZoom);
                }
                Set<double>(() => Zoom, ref zoom, value);
            }
        }

        private bool zoomScaleToFit;
        public bool ZoomScaleToFit {
            get => zoomScaleToFit;
            set => Set<bool>(() => ZoomScaleToFit, ref zoomScaleToFit, value);
        }

        public ObservableCollection<string> ZoomList => zoomList ??
            (zoomList = new ObservableCollection<string> { "Scale to Fit", "20%", "50%", "70%", "100%", "150%", "200%", "400%" });

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

        private RelayCommand newCommand;
        public RelayCommand NewCommand => this.InitCommand(ref newCommand, OnNew, CanNew);

        private bool CanNew() => true;
        private void OnNew() {
            AddTab(ViewModelLocator.Instance.GetEditor(), ++editorIndex, null, "Script " + editorIndex.ToString());
        }

        private RelayCommand openCommand;
        public RelayCommand OpenCommand => this.InitCommand(ref openCommand, OnOpen, CanOpen);

        private bool CanOpen() => true;
        private void OnOpen() {
            OpenFileDialogSettings OpenSettings = new OpenFileDialogSettings() {
                DefaultExt = ".vpy",
                Filter = FileFilter,
                CheckFileExists = true
            };
            if (dialogService.ShowOpenFileDialog(this, OpenSettings) == true)
                ReadScriptFile(OpenSettings.FileName);
        }

        private RelayCommand saveCommand;
        public RelayCommand SaveCommand => this.InitCommand(ref saveCommand, OnSave, CanSave);

        private bool CanSave() => SelectedItem is IEditorViewModel;
        private void OnSave() {
            if (SelectedItem is IEditorViewModel item) {
                if (item.FileName == null)
                    OnSaveAs();
                else
                    File.WriteAllText(item.FileName, item.Script);
            }
        }

        private RelayCommand saveAsCommand;
        public RelayCommand SaveAsCommand => this.InitCommand(ref saveAsCommand, OnSaveAs, CanSaveAs);

        private bool CanSaveAs() => SelectedItem is IEditorViewModel;
        private void OnSaveAs() {
            if (SelectedItem is IEditorViewModel item) {
                SaveFileDialogSettings SaveSettings = new SaveFileDialogSettings() {
                    DefaultExt = ".vpy",
                    Filter = FileFilter,
                    CheckFileExists = false,
                    CheckPathExists = true,
                    OverwritePrompt = true
                };
                if (dialogService.ShowSaveFileDialog(this, SaveSettings) == true) {
                    File.WriteAllText(SaveSettings.FileName, item.Script);
                    item.FileName = SaveSettings.FileName;
                    item.DisplayName = Path.GetFileName(item.FileName);
                }
            }
        }

        private RelayCommand runCommand;
        public RelayCommand RunCommand => this.InitCommand(ref runCommand, OnRun, CanRun);

        private bool CanRun() => SelectedItem is IEditorViewModel;
        private void OnRun() {
            AddTab(ViewModelLocator.Instance.GetViewer(), ++viewerIndex, SelectedItem.Script, "Viewer " + viewerIndex.ToString());
        }

        private RelayCommand goToCommand;
        public RelayCommand GoToCommand => this.InitCommand(ref goToCommand, OnGoTo, CanGoTo);

        private bool CanGoTo() => SelectedItem is IViewerViewModel;
        private void OnGoTo() {
            if (SelectedItem is IViewerViewModel viewer) {
                object Result = InputHelper.RequestInput<int>(this, dialogService, 
                    "Go To Frame...", "Enter frame number:", (int)viewer.Position.TotalSeconds, new Func<int, bool>((e) => {
                        return e >= 0 && e <= (int)viewer.Duration.TotalSeconds;
                }));
                if (Result != null)
                    viewer.Position = TimeSpan.FromSeconds((int)Result);
            }
        }

        private RelayCommand toggleMultiThreadedCommand;
        public RelayCommand ToggleMultiThreadedCommand => this.InitCommand(ref toggleMultiThreadedCommand, OnToggleMultiThreaded, CanToggleMultiThreaded);

        private bool CanToggleMultiThreaded() => true;
        private void OnToggleMultiThreaded() {
            IsMultiThreaded = !IsMultiThreaded;
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

        private RelayCommand<int> selectEditorCommand;
        public RelayCommand<int> SelectEditorCommand => this.InitCommand<int>(ref selectEditorCommand, OnSelectEditor, CanSelectEditor);

        private bool CanSelectEditor(int index) => index < ScriptList.OfType<IEditorViewModel>().Count();
        private void OnSelectEditor(int index) {
            var Editors = ScriptList.OfType<IEditorViewModel>();
            if (index < Editors.Count())
                SelectedItem = Editors.ElementAt(index);
        }

        private RelayCommand<int> selectViewerCommand;
        public RelayCommand<int> SelectViewerCommand => this.InitCommand<int>(ref selectViewerCommand, OnSelectViewer, CanSelectViewer);

        private bool CanSelectViewer(int index) => SelectedItem is IViewerViewModel && index < ScriptList.OfType<IViewerViewModel>().Count();
        private void OnSelectViewer(int index) {
            var Viewers = ScriptList.OfType<IViewerViewModel>();
            if (index < Viewers.Count())
                SelectedItem = Viewers.ElementAt(index);
        }

        private RelayCommand zoomInCommand;
        public RelayCommand ZoomInCommand => this.InitCommand(ref zoomInCommand, OnZoomIn, CanZoomIn);

        private bool CanZoomIn() => true;
        private void OnZoomIn() {
            if (ZoomScaleToFit) {
                Zoom = 1;
                ZoomScaleToFit = false;
            } else
                Zoom *= ZoomIncrement;

        }

        private RelayCommand zoomOutCommand;
        public RelayCommand ZoomOutCommand => this.InitCommand(ref zoomOutCommand, OnZoomOut, CanZoomOut);

        private bool CanZoomOut() => true;
        private void OnZoomOut() {
            if (ZoomScaleToFit) {
                Zoom = 1;
                ZoomScaleToFit = false;
            } else
                Zoom /= ZoomIncrement;
        }

        private RelayCommand helpCommand;
        public RelayCommand HelpCommand => this.InitCommand(ref helpCommand, OnHelp, CanHelp);

        private bool CanHelp() => true;
        private void OnHelp() {
            dialogService.ShowDialog(this, ViewModelLocator.Instance.Help);
        }

        private RelayCommand renameCommand;
        public RelayCommand RenameCommand => this.InitCommand(ref renameCommand, OnRename, CanRename);

        private bool CanRename() => SelectedItem.CanEditHeader;
        private void OnRename() {
            SelectedItem.IsEditingHeader = true;
        }

        #endregion


        public void Window_DropFile(DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files) {
                    ReadScriptFile(file);
                }
            }
        }

        public void Window_PreviewDragOver(DragEventArgs e) {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        /// <summary>
        /// Keys 0-9 allow switching between viewer tabs but this also handles those keys for the editor textbox. Allow those keys to pass through.
        /// </summary>
        public void Editor_PreviewKeyDown(Control sender, System.Windows.Input.KeyEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.None && e.Key >= Key.D0 && e.Key <= Key.D9) {
                string KeyText = new KeyConverter().ConvertToString(e.Key);
                sender.RaiseEvent(new TextCompositionEventArgs(InputManager.Current.PrimaryKeyboardDevice,
                    new TextComposition(InputManager.Current, sender, KeyText)) {
                    RoutedEvent = TextCompositionManager.TextInputEvent
                });
            }
        }

        public void Header_PreviewLeftMouseButtonDown(IScriptViewModel sender, MouseButtonEventArgs e) {
            if (sender == SelectedItem && sender.CanEditHeader && !sender.IsEditingHeader) {
                sender.IsEditingHeader = true;
                e.Handled = true;
            }
        }

        public void Viewer_MediaLoaded() {
            if (selectedItem is ViewerViewModel viewer)
                viewer.Position = playerPosition;
        }

        private void Viewer_RequestClose(object sender, EventArgs e) {
            IScriptViewModel Model = sender as IScriptViewModel;
            Model.Script = null;
            // Remove and select previous tab.
            int Pos = ScriptList.IndexOf(Model);
            // SelectedItem = ScriptList[Pos - 1];
            ScriptList.RemoveAt(Pos);
        }

        public bool ReadScriptFile(string file) {
            try {
                string FileContent = File.ReadAllText(file);
                IEditorViewModel ViewModel = ViewModelLocator.Instance.GetEditor();
                ViewModel.FileName = file;
                AddTab(ViewModel, ++editorIndex, FileContent, Path.GetFileName(file));
                return true;
            } catch (Exception ex) {
                dialogService.ShowMessageBox(this, ex.Message, "Error loading file");
                return false;
            }
        }

        private void AddTab(IScriptViewModel viewModel, int index, string script, string title) {
            if (title != null)
                viewModel.DisplayName = title;
            viewModel.Index = index;
            viewModel.Script = script;
            viewModel.RequestClose += Viewer_RequestClose;
            ScriptList.Add(viewModel);
            SelectedItem = viewModel;
        }
    }
}

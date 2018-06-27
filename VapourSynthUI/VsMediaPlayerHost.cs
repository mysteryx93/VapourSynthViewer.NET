using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EmergenceGuardian.MediaPlayerUI;
using EmergenceGuardian.VapourSynthViewer;

namespace EmergenceGuardian.VapourSynthUI {
    [TemplatePart(Name = VsMediaPlayerHost.PART_Host, Type = typeof(Grid))]
    [TemplatePart(Name = VsMediaPlayerHost.PART_Img, Type = typeof(Image))]
    public class VsMediaPlayerHost : PlayerBase {
        public const string PART_Host = "PART_Host";
        public Grid PartHost => GetTemplateChild(PART_Host) as Grid;
        public const string PART_Img = "PART_Img";
        public Image PartImg => GetTemplateChild(PART_Img) as Image;

        private int pos;
        private int posRequested;
        private VsScript scriptApi;
        private VsVideoInfo vi;
        private VsOutput output;
        private readonly object outputLock = new object();
        private VsFormat format;
        private int threads = 0;
        private bool loop = false;
        private bool limitFps = true;
        WriteableBitmap Bmp;
        private bool isPlaying = false;
        private string autoLoadFile;
        private string autoLoadScript;

        static VsMediaPlayerHost() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VsMediaPlayerHost), new FrameworkPropertyMetadata(typeof(VsMediaPlayerHost)));
        }

        public VsMediaPlayerHost() {
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            //Unloaded += (s2, e2) => Stop();
            Dispatcher.ShutdownStarted += (s2, e2) => Stop();
        }

        public override FrameworkElement InnerControl => PartHost;

        //public static DependencyPropertyKey IsVideoVisiblePropertyKey = DependencyProperty.RegisterReadOnly("IsVideoVisible", typeof(bool), typeof(VsMediaPlayerHost),
        //    new PropertyMetadata(true));
        //public static DependencyProperty IsVideoVisibleProperty = IsVideoVisiblePropertyKey.DependencyProperty;
        //public bool IsVideoVisible { get => (bool)GetValue(IsVideoVisibleProperty); private set => SetValue(IsVideoVisiblePropertyKey, value); }

        public static DependencyPropertyKey IsErrorVisiblePropertyKey = DependencyProperty.RegisterReadOnly("IsErrorVisible", typeof(bool), typeof(VsMediaPlayerHost),
            new PropertyMetadata(false));
        public static DependencyProperty IsErrorVisibleProperty = IsErrorVisiblePropertyKey.DependencyProperty;
        public bool IsErrorVisible { get => (bool)GetValue(IsErrorVisibleProperty); private set => SetValue(IsErrorVisiblePropertyKey, value); }

        public static DependencyPropertyKey ErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly("ErrorMessage", typeof(string), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null));
        public static DependencyProperty ErrorMessageProperty = ErrorMessagePropertyKey.DependencyProperty;
        public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); private set => SetValue(ErrorMessagePropertyKey, value); }

        public static DependencyPropertyKey VideoSourcePropertyKey = DependencyProperty.RegisterReadOnly("VideoSource", typeof(WriteableBitmap), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null));
        public static DependencyProperty VideoSourceProperty = VideoSourcePropertyKey.DependencyProperty;
        public WriteableBitmap VideoSource { get => (WriteableBitmap)GetValue(VideoSourceProperty); private set => SetValue(VideoSourcePropertyKey, value); }

        public static DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null, OnPathChanged, CoercePath));
        public string Path { get => (string)GetValue(PathProperty); set => SetValue(PathProperty, value); }
        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            if (DesignerProperties.GetIsInDesignMode(P))
                return;
            lock (P.outputLock) {
                if (e.NewValue == null)
                    P.Stop();
                else if (!string.IsNullOrWhiteSpace((string)e.NewValue)) {
                    P.LoadScript((string)e.NewValue, null);
                }
            }
        }
        private static object CoercePath(DependencyObject d, object baseValue) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            // Path and Script cannot both be set at once.
            if (P.Script != null)
                return null;
            else
                return baseValue;
        }

        public static DependencyProperty ScriptProperty = DependencyProperty.Register("Script", typeof(string), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null, OnScriptChanged));
        public string Script { get => (string)GetValue(ScriptProperty); set => SetValue(ScriptProperty, value); }
        private static void OnScriptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            if (DesignerProperties.GetIsInDesignMode(P))
                return;
            lock (P.outputLock) {
                if (e.NewValue == null)
                    P.Stop();
                else if (!string.IsNullOrWhiteSpace((string)e.NewValue)) {
                    P.LoadScript(null, (string)e.NewValue);
                }
            }
        }
        private static object CoerceScript(DependencyObject d, object baseValue) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            // Path and Script cannot both be set at once.
            if (P.Path != null)
                return null;
            else
                return baseValue;
        }

        public override TimeSpan Position {
            get {
                lock (outputLock) {
                    return TimeSpan.FromSeconds(pos);
                }
            }
            set {
                lock (outputLock) {
                    if (output != null && IsMediaLoaded) {
                        int NewPos = (int)value.TotalSeconds;
                        if (posRequested != NewPos) {
                            pos = NewPos;
                            posRequested = NewPos;
                            output.ClearQueue();
                            if (IsPlaying)
                                FillQueue();
                            else
                                AddToQueue(true);
                        }
                    } else {
                        pos = (int)value.TotalSeconds;
                        posRequested = (int)value.TotalSeconds;
                    }
                }
                base.PositionChanged();
            }
        }

        public override TimeSpan Duration {
            get => TimeSpan.FromSeconds(vi?.NumFrames ?? 1);
        }

        public override bool IsPlaying {
            get {
                lock (outputLock) {
                    return isPlaying;
                }
            }
            set {
                if (output != null) {
                    lock (outputLock) {
                        if (output != null) {
                            isPlaying = value;
                            if (value)
                                FillQueue();
                            else
                                posRequested -= output.ClearQueue();
                        }
                    }
                    RaisePropertyChanged("IsPlaying");
                }
            }
        }

        public override int Volume {
            get => 0;
            set {
                //InvokePropertyChanged("Volume");
            }
        }

        public override int SpeedInt {
            get => 0;
            set {
                //InvokePropertyChanged("SpeedInt");
            }
        }

        public override float SpeedFloat {
            get => 1;
            set {
                //InvokePropertyChanged("SpeedFloat");
            }
        }

        public override bool Loop {
            get => loop;
            set {
                loop = value;
                RaisePropertyChanged("Loop");
            }
        }

        public bool LimitFps {
            get => limitFps;
            set {
                limitFps = value;
                if (output != null) {
                    lock (output) {
                        if (output != null && vi != null) {
                            if (limitFps && vi.FpsDen > 0)
                                output.MaxFps = (double)vi.FpsNum / vi.FpsDen;
                            else
                                output.MaxFps = 0;
                        }
                    }
                }
                RaisePropertyChanged("LimitFps");
            }
        }

        /// <summary>
        /// Gets or sets the amount of threads to use for processing. 0 to use ProcessorCount.
        /// </summary>
        public int Threads {
            get => threads;
            set {
                if (value < 0)
                    throw new ArgumentException("Threads cannot be negative.");
                threads = value;
            }
        }

        private int GetThreadCount() => threads > 0 ? threads : Environment.ProcessorCount;

        public void SetDllPath(string path) {
            VsHelper.SetDllPath(path);
        }

        private void LoadScript(string file, string script) {
            try {
                // Initialize script.
                lock (outputLock) {
                    if (output != null) {
                        // If a script is already running, stop, wait until media is unloaded and re-run LoadScript.
                        autoLoadFile = file;
                        autoLoadScript = script;
                        Stop();
                        return;
                    }

                    pos = -1;
                    posRequested = 0;
                    ErrorMessage = null;
                    IsErrorVisible = false;
                    if (script != null)
                        scriptApi = VsScript.LoadScript(script);
                    else
                        scriptApi = VsScript.LoadFile(file);
                    output = scriptApi.GetOutput(0);
                    output.FrameDone += Output_FrameDone;
                    output.FrameReady += Output_FrameReady;
                    vi = output.VideoInfo;
                    format = vi.Format;
                    output.SetThreadCount(GetThreadCount());
                }
                Bmp = new WriteableBitmap(vi.Width, vi.Height, 96, 96, PixelFormats.Bgr32, null);
                VideoSource = Bmp;
                if (LimitFps)
                    LimitFps = LimitFps; // this sets output.MaxFps
                base.MediaLoaded();
            } catch (Exception ex) {
                scriptApi?.Dispose();
                scriptApi = null;
                output?.Dispose();
                output = null;
                DisplayError(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
        }

        public void DisplayError(string err) {
            Stop();
            ErrorMessage = err;
            VideoSource = null;
            IsErrorVisible = true;
        }

        public override void Stop() {
            base.Stop();
            if (output != null) {
                lock (outputLock) {
                    if (output != null) {
                        output.ClearQueue(async () => {
                            await Dispatcher.BeginInvoke(new Action(() => {
                                VideoSource = null;
                                IsErrorVisible = false;
                                ErrorMessage = null;
                                Title = null;
                            }));
                            output?.Dispose();
                            output = null;
                            scriptApi?.Dispose();
                            scriptApi = null;
                            await Dispatcher.BeginInvoke(new Action(() => {
                                base.MediaUnloaded();
                                Position = TimeSpan.Zero;
                                if (autoLoadFile != null || autoLoadScript != null) {
                                    LoadScript(autoLoadFile, autoLoadScript);
                                    autoLoadFile = null;
                                    autoLoadScript = null;
                                }
                            }));
                        });
                    }
                }
            }
        }

        public void StopCallback() {

        }

        // This function is guaranteed to be called only once at a time.
        public void FillQueue() {
            if (output != null) {
                lock (outputLock) {
                    if (output != null) {
                        int AddCount = Math.Max(GetThreadCount() - output.GetQueueLength(VsFrameState.Requested), 0);
                        for (int i = 0; i < AddCount; i++) {
                            AddToQueue(false);
                        }
                    }
                }
            }
        }

        public void AddToQueue(bool force) {
            if (output != null) {
                lock (outputLock) {
                    if (output != null) {
                        if ((force || IsPlaying) && posRequested + 1 < vi.NumFrames)
                            output?.GetFrameAsync(posRequested++);
                    }
                }
            }
        }

        public void Output_FrameDone(object sender, VsFrameStatus e) {
            lock (outputLock) {
                if (output == null)
                    return;
            }

            if (e.Error == null)
                AddToQueue(false);
            else
                DisplayError(e.Error);
        }

        // This callback is guaranteed to be raised in the same order as requested.
        public void Output_FrameReady(object sender, VsFrameStatus e) {
            lock (outputLock) {
                if (output == null)
                    return;
            }

            pos = e.Index;
            VsPlane plane = e.Frame.GetPlane(0);

            Dispatcher.Invoke(new Action(() => {
                try {
                    Bmp.Lock();
                    VsHelper.BitBlt(Bmp.BackBuffer, Bmp.BackBufferStride, plane.Ptr, plane.Stride, plane.Width * 4, plane.Height);
                    Bmp.AddDirtyRect(new Int32Rect(0, 0, plane.Width, plane.Height));
                } finally {
                    Bmp.Unlock();
                }
                base.PositionChanged();
            }));
        }
    }
}

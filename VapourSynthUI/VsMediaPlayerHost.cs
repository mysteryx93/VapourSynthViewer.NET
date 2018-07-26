using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EmergenceGuardian.MediaPlayerUI;
using EmergenceGuardian.VapourSynthApi;
using EmergenceGuardian.WpfExtensions;

namespace EmergenceGuardian.VapourSynthUI {
    [TemplatePart(Name = VsMediaPlayerHost.PART_Host, Type = typeof(Grid))]
    [TemplatePart(Name = VsMediaPlayerHost.PART_ImageFit, Type = typeof(Image))]
    [TemplatePart(Name = VsMediaPlayerHost.PART_Zoom, Type = typeof(ZoomViewer))]
    public class VsMediaPlayerHost : PlayerBase {

        #region Declarations / Constructor

        static VsMediaPlayerHost() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VsMediaPlayerHost), new FrameworkPropertyMetadata(typeof(VsMediaPlayerHost)));
        }

        public const string PART_Host = "PART_Host";
        public Grid PartHost => GetTemplateChild(PART_Host) as Grid;
        public const string PART_ImageFit = "PART_ImageFit";
        public Image PartImageFit => GetTemplateChild(PART_ImageFit) as Image;
        public const string PART_Zoom = "PART_Zoom";
        public ZoomViewer PartZoom => GetTemplateChild(PART_Zoom) as ZoomViewer;

        private int posRequested;
        private VsScript scriptApi;
        private VsVideoInfo vi;
        private VsOutput output;
        private readonly object outputLock = new object();
        private VsFormat format;
        WriteableBitmap Bmp;
        private string autoLoadFile;
        private string autoLoadScript;

        #endregion


        #region Properties

        // IsErrorVisible
        public static readonly DependencyPropertyKey IsErrorVisiblePropertyKey = DependencyProperty.RegisterReadOnly("IsErrorVisible", typeof(bool), typeof(VsMediaPlayerHost),
            new PropertyMetadata(false));
        private static readonly DependencyProperty IsErrorVisibleProperty = IsErrorVisiblePropertyKey.DependencyProperty;
        public bool IsErrorVisible { get => (bool)GetValue(IsErrorVisibleProperty); protected set => SetValue(IsErrorVisiblePropertyKey, value); }

        // ErrorMessage
        public static readonly DependencyPropertyKey ErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly("ErrorMessage", typeof(string), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null));
        private static readonly DependencyProperty ErrorMessageProperty = ErrorMessagePropertyKey.DependencyProperty;
        public string ErrorMessage { get => (string)GetValue(ErrorMessageProperty); protected set => SetValue(ErrorMessagePropertyKey, value); }

        // VideoSource
        public static readonly DependencyPropertyKey VideoSourcePropertyKey = DependencyProperty.RegisterReadOnly("VideoSource", typeof(WriteableBitmap), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null));
        private static readonly DependencyProperty VideoSourceProperty = VideoSourcePropertyKey.DependencyProperty;
        public WriteableBitmap VideoSource { get => (WriteableBitmap)GetValue(VideoSourceProperty); protected set => SetValue(VideoSourcePropertyKey, value); }

        // LimitFps
        public static readonly DependencyProperty LimitFpsProperty = DependencyProperty.Register("LimitFps", typeof(bool), typeof(VsMediaPlayerHost),
            new PropertyMetadata(true, LimitFpsChanged));
        public bool LimitFps { get => (bool)GetValue(LimitFpsProperty); set => SetValue(LimitFpsProperty, value); }
        private static void LimitFpsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            P.LimitFpsChanged((bool)e.NewValue);
        }
        protected void LimitFpsChanged(bool value) {
            if (output != null) {
                lock (output) {
                    if (output != null && vi != null) {
                        if (LimitFps && vi.FpsDen > 0)
                            output.MaxFps = (double)vi.FpsNum / vi.FpsDen;
                        else
                            output.MaxFps = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the amount of threads to use for processing. 0 to use ProcessorCount.
        /// </summary>
        public int Threads { get => (int)GetValue(ThreadsProperty); set => SetValue(ThreadsProperty, value); }
        public static readonly DependencyProperty ThreadsProperty = DependencyProperty.Register("Threads", typeof(int), typeof(VsMediaPlayerHost),
            new PropertyMetadata(0, ThreadsChanged, CoerceThreads));
        private static void ThreadsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            if (P.output != null) {
                lock (P.output) {
                    if (P.output != null) {
                        P.output.SetThreadCount(P.GetThreadCount());
                    }
                }
            }
        }
        private static object CoerceThreads(DependencyObject d, object baseValue) => Math.Max((int)baseValue, 0);

        // ScrollVerticalOffset
        public static readonly DependencyProperty ScrollVerticalOffsetProperty = DependencyProperty.Register("ScrollVerticalOffset", typeof(double), typeof(VsMediaPlayerHost),
            new PropertyMetadata(-1.0));
        public double ScrollVerticalOffset { get => (double)GetValue(ScrollVerticalOffsetProperty); set => SetValue(ScrollVerticalOffsetProperty, value); }

        // ScrollHorizontalOffset
        public static readonly DependencyProperty ScrollHorizontalOffsetProperty = DependencyProperty.Register("ScrollHorizontalOffset", typeof(double), typeof(VsMediaPlayerHost),
            new PropertyMetadata(-1.0));
        public double ScrollHorizontalOffset { get => (double)GetValue(ScrollHorizontalOffsetProperty); set => SetValue(ScrollHorizontalOffsetProperty, value); }

        // Path
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null, PathChanged, CoercePath));
        public string Path { get => (string)GetValue(PathProperty); set => SetValue(PathProperty, value); }
        private static async void PathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            if (DesignerProperties.GetIsInDesignMode(P))
                return;
            await Task.Yield();
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

        // Script
        public static readonly DependencyProperty ScriptProperty = DependencyProperty.Register("Script", typeof(string), typeof(VsMediaPlayerHost),
            new PropertyMetadata(null, ScriptChanged));
        public string Script { get => (string)GetValue(ScriptProperty); set => SetValue(ScriptProperty, value); }
        private static async void ScriptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            VsMediaPlayerHost P = d as VsMediaPlayerHost;
            if (DesignerProperties.GetIsInDesignMode(P))
                return;
            await Task.Yield(); // Ensure other properties are set before loading script.
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

        // ZoomIncrement
        public static readonly DependencyProperty ZoomIncrementProperty = DependencyProperty.Register("ZoomIncrement", typeof(double), typeof(VsMediaPlayerHost),
            new FrameworkPropertyMetadata(1.2, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double ZoomIncrement { get => (double)GetValue(ZoomIncrementProperty); set => SetValue(ZoomIncrementProperty, value); }

        // MinZoom
        public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register("MinZoom", typeof(double), typeof(VsMediaPlayerHost),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double MinZoom { get => (double)GetValue(MinZoomProperty); set => SetValue(MinZoomProperty, value); }

        // MaxZoom
        public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register("MaxZoom", typeof(double), typeof(VsMediaPlayerHost),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double MaxZoom { get => (double)GetValue(MaxZoomProperty); set => SetValue(MaxZoomProperty, value); }

        // Zoom
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(VsMediaPlayerHost),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public double Zoom { get => (double)GetValue(ZoomProperty); set => SetValue(ZoomProperty, value); }

        // ZoomScaleToFit
        public static readonly DependencyProperty ZoomScaleToFitProperty = DependencyProperty.Register("ZoomScaleToFit", typeof(bool), typeof(VsMediaPlayerHost),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public bool ZoomScaleToFit { get => (bool)GetValue(ZoomScaleToFitProperty); set => SetValue(ZoomScaleToFitProperty, value); }

        // SquarePixels
        public static readonly DependencyProperty SquarePixelsProperty = DependencyProperty.Register("SquarePixels", typeof(bool), typeof(VsMediaPlayerHost),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));
        public bool SquarePixels { get => (bool)GetValue(SquarePixelsProperty); set => SetValue(SquarePixelsProperty, value); }

        #endregion


        #region Overrides

        public override FrameworkElement InnerControl => PartHost;

        /// <summary>
        /// Occurs when the template is being applied.
        /// </summary>
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();

            // Note: this event could happen more than once if switching theme.
            if (!DesignerProperties.GetIsInDesignMode(this)) {
                Loaded += delegate {
                    Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
                    PartImageFit.PreviewMouseWheel += PartImageFit_PreviewMouseWheel;
                    PartImageFit.MouseRightButtonDown += PartImageFit_MouseRightButtonDown;
                };
                Unloaded += delegate {
                    Dispatcher.ShutdownStarted -= Dispatcher_ShutdownStarted;
                    PartImageFit.PreviewMouseWheel -= PartImageFit_PreviewMouseWheel;
                    PartImageFit.MouseRightButtonDown -= PartImageFit_MouseRightButtonDown;
                    //Stop(); Object can be unloaded for many reasons, we can't stop here.
                };
            }
        }

        private void PartImageFit_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (PartZoom.AllowReset)
                PartZoom.Reset();
        }

        private void PartImageFit_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (PartZoom.AllowZoom && ZoomScaleToFit) {
                Zoom = 1;
                ZoomScaleToFit = false;
            }
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e) {
            Stop();
        }

        /// <summary>
        /// Round position to seconds.
        /// </summary>
        protected override TimeSpan CoercePosition(TimeSpan value) {
            value = base.CoercePosition(value);
            return TimeSpan.FromSeconds((int)value.TotalSeconds);
        }

        /// <summary>
        /// Seeks to specified position when isSeeking is true.
        /// </summary>
        /// <param name="value">The position to seek to.</param>
        /// <param name="isSeeking">Whether the position change is a manual seek.</param>
        protected override void PositionChanged(TimeSpan value, bool isSeeking) {
            if (isSeeking) {
                lock (outputLock) {
                    if (output != null && IsMediaLoaded) {
                        int NewPos = (int)value.TotalSeconds;
                        if (posRequested != NewPos) {
                            posRequested = NewPos - 1; // It will increment in AddToQueue
                            output.ClearQueue();
                            if (IsPlaying)
                                FillQueue();
                            else
                                AddToQueue(true);
                        }
                    } else {
                        posRequested = (int)value.TotalSeconds;
                    }
                }
            }
            base.PositionChanged(value, isSeeking);
        }

        /// <summary>
        /// Pauses or resumes playback.
        /// </summary>
        /// <param name="value">True to resume, false to pause.</param>
        protected override void IsPlayingChanged(bool value) {
            if (output != null) {
                lock (outputLock) {
                    if (output != null) {
                        if (value)
                            FillQueue();
                        else
                            posRequested -= output.ClearQueue();
                    }
                }
            }
            base.IsPlayingChanged(value);
        }

        /// <summary>
        /// Stops playback and unloads file.
        /// </summary>
        /// <param name="callback"Call this method when stopping operation is done.</param>
        public override void Stop() {
            base.Stop();
            if (output != null) {
                lock (outputLock) {
                    if (output != null) {
                        output.ClearQueue(async () => {
                            await Dispatcher.BeginInvoke(new Action(() => {
                                VideoSource = null;
                                //IsErrorVisible = false;
                                //ErrorMessage = null;
                                Title = null;
                            }));
                            output?.Dispose();
                            output = null;
                            scriptApi?.Dispose();
                            scriptApi = null;
                            await Dispatcher.BeginInvoke(new Action(() => {
                                base.MediaUnloaded();
                                SetPositionNoSeek(TimeSpan.Zero);
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

        #endregion


        #region VapourSynth

        /// <summary>
        /// Returns the amount of processing threads to use, defined by Threads. If 0, use ProcessorCount.
        /// </summary>
        private int GetThreadCount() => Threads > 0 ? Threads : Environment.ProcessorCount;

        /// <summary>
        /// Sets the path where to load VapourSynth DLLs.
        /// </summary>
        /// <param name="path">The path containing VapourSynth DLLs.</param>
        public void SetDllPath(string path) {
            VsHelper.SetDllPath(path);
        }

        /// <summary>
        /// Displays specified error message.
        /// </summary>
        /// <param name="err">The error to display.</param>
        public void DisplayError(string err) {
            Stop();
            ErrorMessage = err;
            VideoSource = null;
            IsErrorVisible = true;
        }

        /// <summary>
        /// Loads specified file or script and starts playback. You can only specify one of the two argument.
        /// </summary>
        /// <param name="file">The file path to load.</param>
        /// <param name="script">The script text to load.</param>
        private async void LoadScript(string file, string script) {
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

                    SetPositionNoSeek(TimeSpan.Zero);
                    Duration = TimeSpan.FromSeconds(1);
                    posRequested = -1;
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
                    Duration = TimeSpan.FromSeconds(vi.NumFrames - 1);
                    format = vi.Format;
                    output.SetThreadCount(GetThreadCount());
                }
                Bmp = new WriteableBitmap(vi.Width, vi.Height, 96, 96, PixelFormats.Bgr32, null);
                VideoSource = Bmp;
                LimitFpsChanged(LimitFps);
                base.MediaLoaded();
                if (Position == TimeSpan.Zero)
                    PositionChanged(TimeSpan.Zero, true);
                await Task.Yield();
                ScrollVerticalOffset = ScrollVerticalOffset;
                ScrollHorizontalOffset = ScrollHorizontalOffset;
            } catch (Exception ex) {
                scriptApi?.Dispose();
                scriptApi = null;
                output?.Dispose();
                output = null;
                DisplayError(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }
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
                        if ((force || IsPlaying) && posRequested < vi.NumFrames - 1)
                            output?.GetFrameAsync(++posRequested);
                    }
                }
            }
        }

        public void Output_FrameDone(object sender, VsFrameStatus e) {
            lock (outputLock) {
                if (output == null)
                    return;
            }

            Dispatcher.Invoke(() => {
                if (e.Error == null)
                    AddToQueue(false);
                else
                    DisplayError(e.Error);
            });
        }

        // This callback is guaranteed to be raised in the same order as requested.
        public void Output_FrameReady(object sender, VsFrameStatus e) {
            lock (outputLock) {
                if (output == null)
                    return;
            }

            VsPlane plane = e.Frame.GetPlane(0);

            Dispatcher.Invoke(new Action(() => {
                if (!IsErrorVisible) {
                    try {
                        Bmp.Lock();
                        VsHelper.BitBlt(Bmp.BackBuffer, Bmp.BackBufferStride, plane.Ptr, plane.Stride, plane.Width * 4, plane.Height);
                        Bmp.AddDirtyRect(new Int32Rect(0, 0, plane.Width, plane.Height));
                    } finally {
                        Bmp.Unlock();
                    }
                    SetPositionNoSeek(TimeSpan.FromSeconds(e.Index));
                }
            }));
        }

        #endregion

    }
}

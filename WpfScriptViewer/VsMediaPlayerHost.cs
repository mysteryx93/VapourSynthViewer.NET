using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EmergenceGuardian.MediaPlayerUI;
using EmergenceGuardian.VapourSynthViewer;

namespace WpfScriptViewer {
	[TemplatePart(Name = VsMediaPlayerHost.PART_Host, Type = typeof(Grid))]
	[TemplatePart(Name = VsMediaPlayerHost.PART_Img, Type = typeof(Image))]
	[TemplatePart(Name = VsMediaPlayerHost.PART_Error, Type = typeof(Label))]
	public class VsMediaPlayerHost : PlayerBase {
		public const string PART_Host = "PART_Host";
		public Grid PartHost => GetTemplateChild(PART_Host) as Grid;
		public const string PART_Img = "PART_Img";
		public Image PartImg => GetTemplateChild(PART_Img) as Image;
		public const string PART_Error = "PART_Error";
		public Label PartError => GetTemplateChild(PART_Error) as Label;

		private int pos;
		private int posRequested;
		private VsScriptApi scriptApi;
		private VsVideoInfo vi;
		private VsOutput output;
		private object outputLock = new object();
		private VsFormat format;
		private int threads = 8;
		private string source;
		private bool loop = false;
		private bool limitFps = true;
		WriteableBitmap Bmp;
		private bool isPlaying = false;


		// This property is thread-safe.
		//public bool IsPlaying {
		//	get => Interlocked.CompareExchange(ref isPlayingSafe, 1, 1) == 1;
		//	set {
		//		if (value) Interlocked.CompareExchange(ref isPlayingSafe, 1, 0);
		//		else Interlocked.CompareExchange(ref isPlayingSafe, 0, 1);
		//	}
		//}

		static VsMediaPlayerHost() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VsMediaPlayerHost), new FrameworkPropertyMetadata(typeof(VsMediaPlayerHost)));
		}

		public VsMediaPlayerHost() {
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			Unloaded += UserControl_Unloaded;
			Dispatcher.ShutdownStarted += (s2, e2) => UserControl_Unloaded(s2, null);
		}

		private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
			if (output != null) {
				lock (outputLock) {
					if (output != null) {
						output.Dispose();
						output = null;
						//scriptApi.Dispose();
						//scriptApi = null;
					}
				}
			}
		}

		public override FrameworkElement InnerControl => PartHost;

		public override void Load(string source) {
			Load(source, null);
		}

		public override TimeSpan Position {
			get {
				lock (outputLock) {
					return TimeSpan.FromSeconds(pos);
				}
			}
			set {
				lock (outputLock) {
					if (output != null) {
						int NewPos = (int)value.TotalSeconds;
						if (posRequested != NewPos) {
							posRequested = NewPos;
							output.ClearQueue();
							if (isPlaying)
								FillQueue();
							else
								AddToQueue(true);
						}
					} else {
						pos = (int)value.TotalSeconds;
						posRequested = (int)value.TotalSeconds;
					}
				}
				InvokePropertyChanged("Position");
			}
		}

		public override TimeSpan Duration {
			get => TimeSpan.FromSeconds(vi?.NumFrames ?? 1);
		}

		public override bool Paused {
			get {
				lock (outputLock) {
					return isPlaying;
				}
			}
			set {
				if (output != null) {
					lock (outputLock) {
						if (output != null) {
							isPlaying = !value;
							if (value)
								posRequested -= output.ClearQueue();
							else
								FillQueue();
						}
					}
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
				InvokePropertyChanged("Loop");
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
				InvokePropertyChanged("LimitFps");
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
			VsScriptApi.SetDllPath(path);
		}



		public void Load(string source, string title) {
			this.source = source;
			if (title == null)
				Title = System.IO.Path.GetFileName(source);
			try {
				// Initialize script.
				lock (outputLock) {
					pos = -1;
					posRequested = 0;
					VsScriptApi.SetDllPath(@"C:\Program Files (x86)\VapourSynth\core64");
					scriptApi = VsScriptApi.LoadFile(@"C:\GitHub\VapourSynth Viewer .NET\test.vpy", true);
					output = scriptApi.GetOutput(0);
					output.FrameDone += output_FrameDone;
					output.FrameReady += output_FrameReady;
					vi = output.VideoInfo;
					format = vi.Format;
					output.SetThreadCount(GetThreadCount());
				}
				Bmp = new WriteableBitmap(
					vi.Width, vi.Height, 96, 96, PixelFormats.Bgr32, null);
				if (LimitFps)
					LimitFps = LimitFps; // this sets output.MaxFps
				base.MediaLoaded();
			} catch (Exception ex) {
				DisplayError(ex.Message);
			}
		}

		public void DisplayError(string err) {
			Stop();
			Dispatcher.Invoke(() => {
				PartImg.Visibility = Visibility.Collapsed;
				PartError.Content = err;
				PartError.Visibility = Visibility.Visible;
			});
		}

		public override void Stop() {
			if (output != null) {
				lock (outputLock) {
					if (output != null) {
						output.ClearQueueWait(() => {
							output.Dispose();
							output = null;
							scriptApi.Dispose();
							scriptApi = null;
							Dispatcher.Invoke(() => {
								PartImg.Source = null;
								PartImg.Visibility = Visibility.Visible;
								PartError.Visibility = Visibility.Collapsed;
								base.MediaUnloaded();
								Title = null;
								Position = TimeSpan.Zero;
							});
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
						int AddCount = Math.Max(GetThreadCount() - output.QueueLength, 0);
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
						if ((force || isPlaying) && posRequested + 1 < vi.NumFrames)
							output?.GetFrameAsync(posRequested++);
					}
				}
			}
		}

		public void output_FrameDone(object sender, FrameStatus e) {
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
		public void output_FrameReady(object sender, FrameStatus e) {
			lock (outputLock) {
				if (output == null)
					return;
			}

			pos = e.Index;
			VsPlane plane = e.Frame.GetPlane(0);

			Dispatcher.Invoke(() => {
				try {
					Bmp.Lock();
					VsScriptApi.BitBlt(Bmp.BackBuffer, Bmp.BackBufferStride, plane.Ptr, plane.Stride, plane.Width * 4, plane.Height);
					Bmp.AddDirtyRect(new Int32Rect(0, 0, plane.Width, plane.Height));
				} finally {
					Bmp.Unlock();
				}
				PartImg.Source = Bmp;
			});
			base.PositionChanged();
		}
	}
}

using System;
using System.ComponentModel;
using System.Windows;
using EmergenceGuardian.MediaPlayerUI;

namespace EmergenceGuardian.VapourSynthUI {
	public class VsMediaPlayer : MediaPlayerWpf {
		static VsMediaPlayer() {
			// DefaultStyleKeyProperty.OverrideMetadata(typeof(MpvMediaPlayer), new FrameworkPropertyMetadata(typeof(MpvMediaPlayer)));
		}

		public VsMediaPlayer() {
        }

        public override void OnApplyTemplate() {
			base.OnApplyTemplate();
            
            // Overwrite default values.
            UI.IsVolumeVisible = false;
            UI.IsSpeedVisible = false;
            UI.ChangeVolumeOnMouseWheel = false;
            UI.IsStopVisible = false;
            UI.IsLoopVisible = false;
            UI.PositionDisplay = TimeDisplayStyles.Seconds;

            if (DesignerProperties.GetIsInDesignMode(this))
				return;

            var PlayerHost = Host ?? new VsMediaPlayerHost();
            if (DllPath != null)
                PlayerHost.SetDllPath(DllPath);
            if (Content == null)
                Content = PlayerHost;
        }

		// DllPath
		public static DependencyProperty DllPathProperty = DependencyProperty.Register("DllPath", typeof(string), typeof(VsMediaPlayer));
		public string DllPath { get => (string)GetValue(DllPathProperty); set => SetValue(DllPathProperty, value); }

		public VsMediaPlayerHost Host {
			get => Content as VsMediaPlayerHost;
			set => Content = value;
		}
	}
}

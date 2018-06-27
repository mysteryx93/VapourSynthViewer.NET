using System;
using System.ComponentModel;
using System.Windows;
using EmergenceGuardian.MediaPlayerUI;

namespace EmergenceGuardian.VapourSynthUI {
	public class VsMediaPlayer2 : MediaPlayerWpf {
		static VsMediaPlayer2() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VsMediaPlayer2), new FrameworkPropertyMetadata(typeof(VsMediaPlayer2)));
			IsVolumeVisibleProperty.OverrideMetadata(typeof(VsMediaPlayer2), new FrameworkPropertyMetadata(false));
			IsSpeedVisibleProperty.OverrideMetadata(typeof(VsMediaPlayer2), new FrameworkPropertyMetadata(false));
			IsLoopVisibleProperty.OverrideMetadata(typeof(VsMediaPlayer2), new FrameworkPropertyMetadata(false));
			PositionDisplayProperty.OverrideMetadata(typeof(VsMediaPlayer2), new FrameworkPropertyMetadata(TimeDisplayStyles.Seconds));
		}

		public VsMediaPlayer2() {
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			if (DesignerProperties.GetIsInDesignMode(this))
				return;

			var PlayerHost = new VsMediaPlayerHost();
			base.Host = PlayerHost;
		}

		public new VsMediaPlayerHost Host {
			get => base.Host as VsMediaPlayerHost;
			set => base.Host = value;
		}

        // Path
        public static DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(VsMediaPlayer2),
            new PropertyMetadata(null));
        public string Path { get => (string)GetValue(PathProperty); set => SetValue(PathProperty, value); }

        // Script
        public static DependencyProperty ScriptProperty = DependencyProperty.Register("Script", typeof(string), typeof(VsMediaPlayer2),
            new PropertyMetadata(null));
        public string Script { get => (string)GetValue(ScriptProperty); set => SetValue(ScriptProperty, value); }
    }
}

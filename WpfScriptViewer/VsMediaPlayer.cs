using System;
using System.ComponentModel;
using System.Windows;
using EmergenceGuardian.MediaPlayerUI;

namespace WpfScriptViewer {
	public class VsMediaPlayer : MediaPlayerWpf {
		static VsMediaPlayer() {
			DefaultStyleKeyProperty.OverrideMetadata(typeof(VsMediaPlayer), new FrameworkPropertyMetadata(typeof(VsMediaPlayer)));
			IsVolumeVisibleProperty.OverrideMetadata(typeof(VsMediaPlayer), new FrameworkPropertyMetadata(false));
			IsSpeedVisibleProperty.OverrideMetadata(typeof(VsMediaPlayer), new FrameworkPropertyMetadata(false));
			IsLoopVisibleProperty.OverrideMetadata(typeof(VsMediaPlayer), new FrameworkPropertyMetadata(false));
			PositionDisplayProperty.OverrideMetadata(typeof(VsMediaPlayer), new FrameworkPropertyMetadata(TimeDisplayStyles.Seconds));
		}

		public VsMediaPlayer() {
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
	}
}

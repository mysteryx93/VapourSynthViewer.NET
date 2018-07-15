using System;

namespace EmergenceGuardian.VapourSynthApi {
    public class VsFrameStatus : EventArgs {
        public VsFrameStatus() { }
        public VsFrameStatus(int index) {
            this.Index = index;
        }

        public int Index { get; set; }
        public VsFrame Frame { get; set; }
        public string Error { get; set; }
		public VsFrameState State { get; set; } = VsFrameState.Requested;
    }

	public enum VsFrameState {
		Requested,
		Cancelled,
		Completed
	}
}

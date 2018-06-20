using System;

namespace EmergenceGuardian.VapourSynthViewer {
    public class FrameStatus : EventArgs {
        public FrameStatus() { }
        public FrameStatus(int index) {
            this.Index = index;
        }

        public int Index;
        public VsFrame Frame;
        public string Error;
    }
}

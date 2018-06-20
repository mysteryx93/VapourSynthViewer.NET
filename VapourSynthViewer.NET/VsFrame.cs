using System;

namespace EmergenceGuardian.VapourSynthViewer {
    public class VsFrame : IDisposable {
        private VsOutput output;
        private IntPtr frame;
		public int Index { get; private set; }

        private VsFrame() { }
        internal VsFrame(VsOutput output, IntPtr frame, int index) {
            this.output = output;
            this.frame = frame;
			this.Index = index;
        }

        public void Dispose() {
            output.Api.freeFrame(frame);
			//System.Diagnostics.Debug.WriteLine("VsFrame Dispose {0}", index);
		}

        public VsPlane GetPlane(int plane) {
            return new VsPlane(output, frame, plane);
        }
    }
}

using System;

namespace EmergenceGuardian.VapourSynthViewer {
    public class VsFrame : IDisposable {
        private VsOutput output;
        private IntPtr frame;
		public int Index { get; private set; }
		public static event EventHandler<int> Requested;
		public static event EventHandler<VsFrame> Allocated;
		public static event EventHandler<VsFrame> Deallocated;

		internal static void RaiseRequested(int index) {
			Requested?.Invoke(null, index);
		}

        private VsFrame() { }
        internal VsFrame(VsOutput output, IntPtr frame, int index) {
            this.output = output;
            this.frame = frame;
			this.Index = index;
			VsFrame.Allocated?.Invoke(this, this);
        }

        public void Dispose() {
            output.Api.freeFrame(frame);
			VsFrame.Deallocated?.Invoke(this, this);
		}

        public VsPlane GetPlane(int plane) {
            return new VsPlane(output, frame, plane);
        }
    }
}

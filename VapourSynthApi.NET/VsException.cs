using System;
using System.Collections.Generic;
using System.Text;

namespace EmergenceGuardian.VapourSynthViewer {
    public class VsException : Exception {
        public VsException(string msg) : base(msg) {
        }
    }
}

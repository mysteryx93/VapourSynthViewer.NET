using System;
using System.Collections.Generic;
using System.Text;

namespace EmergenceGuardian.VapourSynthApi {
    public class VsException : Exception {
        public VsException(string msg) : base(msg) {
        }
    }
}

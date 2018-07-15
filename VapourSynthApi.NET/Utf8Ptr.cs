using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EmergenceGuardian.VapourSynthApi {
    public class Utf8Ptr {
        public IntPtr ptr = IntPtr.Zero;
        private Utf8Ptr() { }

        public Utf8Ptr(string s) {
            if (s != null) {
                var bytes = Encoding.UTF8.GetByteCount(s);
                var buffer = new byte[bytes + 1];
                Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, 0);
                ptr = Marshal.AllocCoTaskMem(bytes + 1);
                Marshal.Copy(buffer, 0, ptr, bytes + 1);
            }
        }

        ~Utf8Ptr() {
            if (ptr != IntPtr.Zero)
                Marshal.FreeCoTaskMem(ptr);
        }

        public static string FromUtf8Ptr(IntPtr ptr) {
            return FromUtf8Ptr(ptr, -1);
        }

        public static string FromUtf8Ptr(IntPtr ptr, int maxLength) {
            if (ptr != IntPtr.Zero) {
                int Length = 0;
                if (maxLength >= 0)
                    while (Marshal.ReadByte(ptr, Length++) != 0 && Length <= maxLength) { }
                else
                    while (Marshal.ReadByte(ptr, Length++) != 0) { }
                Length--;
                byte[] Data = new byte[Length];
                Marshal.Copy(ptr, Data, 0, Length);
                return Encoding.UTF8.GetString(Data);
            } else
                return null;
        }
    }
}

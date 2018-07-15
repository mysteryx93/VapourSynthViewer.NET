using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace EmergenceGuardian.VapourSynthViewer {
    public class VsScript : IDisposable {
        /// <summary>
        /// Automatically calls Init and Finalize. Must call VsHelper.SetDllPath before referencing VsScript class.
        /// </summary>
        static VsScript() {
            VsHelper.Init();
        }

        private readonly IntPtr handle = IntPtr.Zero;

        private VsScript() { }

        /// <summary>
        /// Initializes a new instance of the VsScript class. This constructor will not assign any unmanaged resource.
        /// </summary>
        /// <param name="handle">The handle of the script.</param>
        public VsScript(IntPtr handle) {
            this.handle = handle;
        }

        /// <summary>
        /// Releases all resources associated with the script. Make sure all frames have been released first.
        /// </summary>
        public void Dispose() {
            VsInvoke.vsscript_freeScript(handle);
        }

        /// <summary>
        /// Returns the error message returned by the script, or null.
        /// </summary>
        private String GetError() {
            IntPtr Err = VsInvoke.vsscript_getError(handle);
            return Utf8Ptr.FromUtf8Ptr(Err);
        }

        /// <summary>
        /// Returns the first script video output.
        /// </summary>
        public VsOutput GetOutput() {
            return GetOutput(0);
        }

        /// <summary>
        /// Returns specified script video output.
        /// </summary>
        /// <param name="index">The output index specified in 'set_output'.</param>
        public VsOutput GetOutput(int index) {
            return new VsOutput(handle, index);
        }

        /// <summary>
        /// Clears the first script video output.
        /// </summary>
        public int ClearOutput() {
            return ClearOutput(0);
        }

        /// <summary>
        /// Clears specified script video output.
        /// </summary>
        /// <param name="index">The output index specified in 'set_output'.</param>
        public int ClearOutput(int index) {
            return VsInvoke.vsscript_clearOutput(handle, index);
        }




        private static string convertCompatScript;

        /// <summary>
        /// Creates an empty script environment.
        /// </summary>
        public static VsScript CreateEmpty() {
            IntPtr H = IntPtr.Zero;
            VsInvoke.vsscript_createScript(ref H);
            return new VsScript(H);
        }

        /// <summary>
        /// Loads a script directly from a file. It will not be converted to COMPATBGR32.
        /// </summary>
        /// <param name="path">The path of the script file to open.</param>
        /// <param name="setWorkingDir">If true, current working directory will be changed to the path of the script.</param>
        public static VsScript LoadFileDirect(string path, bool setWorkingDir) {
            IntPtr H = IntPtr.Zero;
            if (VsInvoke.vsscript_evaluateFile(ref H, new Utf8Ptr(path).ptr, setWorkingDir ? VsInvoke.FlagSetWorkingDir : 0) == 0) {
                return new VsScript(H);
            } else {
                string Err = new VsScript(H).GetError();
                VsInvoke.vsscript_freeScript(H);
                throw new VsException(Err);
            }
        }

        /// <summary>
        /// Loads a script from a file and converts it to COMPATBGR32 format.
        /// </summary>
        /// <param name="path">The path of the script file to open.</param>
        public static VsScript LoadFile(string path) {
            string Script = File.ReadAllText(path);
            return LoadScript(Script, true);
        }

        /// <summary>
        /// Loads a script from a file.
        /// </summary>
        /// <param name="path">The path of the script file to open.</param>
        /// <param name="convertToCompatBgr32">If true, the script output will be converted to COMPATBGR32.</param>
        public static VsScript LoadFile(string path, bool convertToCompatBgr32) {
            string Script = File.ReadAllText(path);
            return LoadScript(Script, convertToCompatBgr32);
        }

        /// <summary>
        /// Loads specified script and converts it to COMPATBGR32 format.
        /// </summary>
        /// <param name="script">The script to load.</param>
        public static VsScript LoadScript(string script) {
            return LoadScript(script, true);
        }

        /// <summary>
        /// Loads specified script.
        /// </summary>
        /// <param name="script">The script to load.</param>
        /// <param name="convertToCompatBgr32">If true, the script output will be converted to COMPATBGR32.</param>
        public static VsScript LoadScript(string script, bool convertToCompatBgr3) {
            if (convertToCompatBgr3)
                script = AppendConvertCompoatScript(script);

            IntPtr H = IntPtr.Zero;
            if (VsInvoke.vsscript_evaluateScript(ref H, new Utf8Ptr(script).ptr, IntPtr.Zero, 0) == 0) {
                return new VsScript(H);
            } else {
                string Err = new VsScript(H).GetError();
                VsInvoke.vsscript_freeScript(H);
                throw new VsException(Err);
            }
        }

        /// <summary>
        /// Appends code to convert the script to COMPATBGR32 format.
        /// </summary>
        /// <param name="script">The script to convert.</param>
        private static string AppendConvertCompoatScript(string script) {
            if (convertCompatScript == null)
                convertCompatScript = GetConvertCompatScript();
            StringBuilder Result = new StringBuilder(script);
            Result.AppendLine().AppendLine();
            Result.Append(convertCompatScript);
            return Result.ToString();
        }

        /// <summary>
        /// Returns the content of the COMPATBGR32.vpy resource file.
        /// </summary>
        private static string GetConvertCompatScript() {
            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("EmergenceGuardian.VapourSynthViewer.COMPATBGR32.vpy"))) {
                return reader.ReadToEnd();
            }
        }
    }
}

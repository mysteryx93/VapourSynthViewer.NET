using System;
using System.IO;
using System.Runtime.InteropServices;

namespace EmergenceGuardian.VapourSynthApi {
    internal class VsInvoke {
        //const string VsDll = "vapoursynth.dll";
        const string VsScriptDll = "vsscript.dll";

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        internal static string DllPath;
        /// <summary>
        /// Loads VapourSynth DLLs from specified path. Call this before calling any other functions if DLLs can't be found by default.
        /// </summary>
        /// <param name="path">The path containing the DLLs.</param>
        public static void SetDllPath(string path) {
            DllPath = path;
            LoadLibrary(Path.Combine(DllPath, VsScriptDll));
        }

        public const int VSSCRIPT_API_MAJOR = 3;
        public const int VSSCRIPT_API_MINOR = 2;
        public const int VSSCRIPT_API_VERSION = ((VSSCRIPT_API_MAJOR << 16) | (VSSCRIPT_API_MINOR));
        public const int FlagSetWorkingDir = 1;

        /// <summary>
        /// Get the api version.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_getApiVersion();

        /// <summary>
        /// Initialize the available scripting runtimes, returns zero on failure.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_init();

        /// <summary>
        /// Free all scripting runtimes
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_finalize();


        /*
       * Pass a pointer to a null handle to create a new one
       * The values returned by the query functions are only valid during the lifetime of the VSScript
       * scriptFilename is if the error message should reference a certain file, NULL allowed in vsscript_evaluateScript()
       * core is to pass in an already created instance so that mixed environments can be used,
       * NULL creates a new core that can be fetched with vsscript_getCore() later OR implicitly uses the one associated with an already existing handle when passed
       * If efSetWorkingDir is passed to flags the current working directory will be changed to the path of the script
       * note that if scriptFilename is NULL in vsscript_evaluateScript() then __file__ won't be set and the working directory won't be changed
       * Set efSetWorkingDir to get the default and recommended behavior
       */


        /// <summary>
        /// Loads a script.
        /// </summary>
        /// <param name="handle">Pass a pointer to a null handle to create a new one.</param>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_evaluateScript(ref IntPtr handle, IntPtr script, IntPtr scriptFileName, int flags);

        /// <summary>
        /// Loads a script from a file.
        /// </summary>
        /// <param name="handle">Pass a pointer to a null handle to create a new one.</param>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_evaluateFile(ref IntPtr handle, IntPtr scriptFilename, int flags);

        /// <summary>
        /// Create an empty environment for use in later invocations, mostly useful to set script variables before execution.
        /// </summary>
        /// <param name="handle">Pass a pointer to a null handle to create a new one.</param>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_createScript(ref IntPtr handle);

        /// <summary>
        /// Frees resources associated with the script.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern void vsscript_freeScript(IntPtr handle);

        /// <summary>
        /// Returns errors thrown by the script.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern IntPtr vsscript_getError(IntPtr handle);

        /// <summary>
        /// Returns the frame at specified position.
        /// The node returned must be freed using freeNode() before calling vsscript_freeScript().
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern IntPtr vsscript_getOutput(IntPtr handle, int index);

        /// <summary>
        /// Returns the frame at specified position. The alpha node pointer will only be set if an alpha clip has been set in the script.
        /// The node returned must be freed using freeNode() before calling vsscript_freeScript().
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern IntPtr vsscript_getOutput2(IntPtr handle, int index, IntPtr alpha);

        /// <summary>
        /// 
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_clearOutput(IntPtr handle, int index);

        /// <summary>
        /// Returns the core associated with the script. The core is valid as long as the environment exists.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern IntPtr vsscript_getCore(IntPtr handle);

        /// <summary>
        /// Convenience function for retrieving a vsapi pointer.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern IntPtr vsscript_getVSApi2(int version);

        /// <summary>
        /// Variables names that are not set or not of a convertible type will return an error.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern int vsscript_getVariable(IntPtr handle, IntPtr name, IntPtr dstMap);


        [DllImport(VsScriptDll)]
        public static extern int vsscript_setVariable(IntPtr handle, IntPtr vars);

        [DllImport(VsScriptDll)]
        public static extern int vsscript_clearVariable(IntPtr handle, IntPtr name);

        /// <summary>
        /// Tries to clear everything set in an environment, normally it is better to simply free an environment completely and create a new one.
        /// </summary>
        [DllImport(VsScriptDll)]
        public static extern void vsscript_clearEnvironment(IntPtr handle);
    }
}

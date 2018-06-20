using System;
using System.Collections.Generic;
using System.Text;

namespace EmergenceGuardian.VapourSynthViewer {
    public struct VsApiInvoke {
        /// <summary>
        /// VSCore *(VS_CC *createCore)(int threads)
        /// </summary>
        public CreateCoreFunction createCore;
        /// <summary>
        /// void (VS_CC *freeCore)(VSCore *core)
        /// </summary>
        public FreeCoreFunction freeCore;
        /// <summary>
        /// const VSCoreInfo *(VS_CC *getCoreInfo)(VSCore *core)
        /// </summary>
        public GetCoreInfoFunction getCoreInfo;
        /// <summary>
        /// const VSFrameRef *(VS_CC *cloneFrameRef)(const VSFrameRef *f)
        /// </summary>
        public CloneFrameRefFunction cloneFrameRef;
        /// <summary>
        /// VSNodeRef *(VS_CC *cloneNodeRef)(VSNodeRef *node)
        /// </summary>
        public CloneNodeRefFunction cloneNodeRef;
        /// <summary>
        /// VSFuncRef *(VS_CC *cloneFuncRef)(VSFuncRef *f)
        /// </summary>
        public CloneFuncRefFunction cloneFuncRef;
        /// <summary>
        /// void (VS_CC *freeFrame)(const VSFrameRef *f)
        /// </summary>
        public FreeFrameFunction freeFrame;
        /// <summary>
        /// void (VS_CC *freeNode)(VSNodeRef *node)
        /// </summary>
        public freeNodeFunction freeNode;
        /// <summary>
        /// void (VS_CC *freeFunc)(VSFuncRef *f)
        /// </summary>
        public FreeFuncFunction freeFunc;
        /// <summary>
        /// VSFrameRef *(VS_CC *newVideoFrame)(const VSFormat *format, int width, int height, const VSFrameRef *propSrc, VSCore *core)
        /// </summary>
        public NewVideoFrameFunction newVideoFrame;
        /// <summary>
        /// VSFrameRef *(VS_CC *copyFrame)(const VSFrameRef *f, VSCore *core)
        /// </summary>
        public CopyFrameFunction copyFrame;
        /// <summary>
        /// void (VS_CC *copyFrameProps)(const VSFrameRef *src, VSFrameRef *dst, VSCore *core)
        /// </summary>
        public CopyFramePropsFunction copyFrameProps;
        /// <summary>
        /// void (VS_CC *registerFunction)(const char *name, const char *args, VSPublicFunction argsFunc, void *functionData, VSPlugin *plugin)
        /// </summary>
        public RegisterFunctionFunction registerFunction;
        /// <summary>
        /// VSPlugin *(VS_CC *getPluginById)(const char *identifier, VSCore *core)
        /// </summary>
        public GetPluginByIdFunction getPluginById;
        /// <summary>
        /// VSPlugin *(VS_CC *getPluginByNs)(const char *ns, VSCore *core)
        /// </summary>
        public GetPluginByNsFunction getPluginByNs;
        /// <summary>
        /// VSMap *(VS_CC *getPlugins)(VSCore *core)
        /// </summary>
        public GetPluginsFunction getPluginsFunction;
        /// <summary>
        /// VSMap *(VS_CC *getFunctions)(VSPlugin *plugin)
        /// </summary>
        public GetFunctionsFunction getFunctions;
        /// <summary>
        /// void (VS_CC *createFilter)(const VSMap *in, VSMap *out, const char *name, VSFilterInit init, VSFilterGetFrame getFrame, VSFilterFree free, int filterMode, int flags, void *instanceData, VSCore *core)
        /// </summary>
        public CreateFilterFunction createFilter;
        /// <summary>
        /// void (VS_CC *setError)(VSMap *map, const char *errorMessage)
        /// </summary>
        public SetErrorFunction setError;
        /// <summary>
        /// const char *(VS_CC *getError)(const VSMap *map)
        /// </summary>
        public GetErrorFunction getError;
        /// <summary>
        /// void (VS_CC *setFilterError)(const char *errorMessage, VSFrameContext *frameCtx)
        /// </summary>
        public SetFilterErrorFunction setFilterError;
        /// <summary>
        /// VSMap *(VS_CC *invoke)(VSPlugin *plugin, const char *name, const VSMap *args)
        /// </summary>
        public InvokeFunction invoke;
        /// <summary>
        /// const VSFormat *(VS_CC *getFormatPreset)(int id, VSCore *core)
        /// </summary>
        public GetFormatPresetFunction getFormatPreset;
        /// <summary>
        /// const VSFormat *(VS_CC *registerFormat)(int colorFamily, int sampleType, int bitsPerSample, int subSamplingW, int subSamplingH, VSCore *core)
        /// </summary>
        public RegisterFormatFunction registerFormat;
        /// <summary>
        /// const VSFrameRef *(VS_CC *getFrame)(int n, VSNodeRef *node, char *errorMsg, int bufSize)
        /// </summary>
        public GetFrameFunction getFrame;
        /// <summary>
        /// void (VS_CC *getFrameAsync)(int n, VSNodeRef *node, VSFrameDoneCallback callback, void *userData)
        /// </summary>
        public GetFrameAsyncFunction getFrameAsync;
        /// <summary>
        /// void (VS_CC *getFrameAsync)(int n, VSNodeRef *node, VSFrameDoneCallback callback, void *userData)
        /// </summary>
        public GetFrameFilterFunction getFrameFilter;
        /// <summary>
        /// void (VS_CC *requestFrameFilter)(int n, VSNodeRef *node, VSFrameContext *frameCtx)
        /// </summary>
        public RequestFrameFilterFunction requestFrameFilter;
        /// <summary>
        /// void (VS_CC *queryCompletedFrame)(VSNodeRef **node, int *n, VSFrameContext *frameCtx)
        /// </summary>
        public QueryCompletedFrameFunction queryCompletedFrame;
        /// <summary>
        /// void (VS_CC *releaseFrameEarly)(VSNodeRef *node, int n, VSFrameContext *frameCtx)
        /// </summary>
        public ReleaseFrameEarlyFunction releaseFrameEarly;
        /// <summary>
        /// int (VS_CC *getStride)(const VSFrameRef *f, int plane)
        /// </summary>
        public GetStrideFunction getStride;
        /// <summary>
        /// const uint8_t *(VS_CC *getReadPtr)(const VSFrameRef *f, int plane)
        /// </summary>
        public GetReadPtrFunction getReadPtr;
        /// <summary>
        /// uint8_t *(VS_CC *getWritePtr)(VSFrameRef *f, int plane)
        /// </summary>
        public GetWritePtrFunction getwritePtr;
        /// <summary>
        /// VSFuncRef *(VS_CC *createFunc)(VSPublicFunction func, void *userData, VSFreeFuncData free, VSCore *core, const VSAPI *vsapi)
        /// </summary>
        public CreateFuncFunction createFunc;
        /// <summary>
        /// void (VS_CC *callFunc)(VSFuncRef *func, const VSMap *in, VSMap *out, VSCore *core, const VSAPI *vsapi)
        /// </summary>
        public CallFuncFunction callFunc;
        /// <summary>
        /// VSMap *(VS_CC *createMap)(void)
        /// </summary>
        public CreateMapFunction createMap;
        /// <summary>
        /// void (VS_CC *freeMap)(VSMap *map)
        /// </summary>
        public FreeMapFunction freeMap;
        /// <summary>
        /// void (VS_CC *clearMap)(VSMap *map)
        /// </summary>
        public ClearMapFunction clearMap;
        /// <summary>
        /// const VSVideoInfo *(VS_CC *getVideoInfo)(VSNodeRef *node)
        /// </summary>
        public GetVideoInfoFunction getVideoInfo;
        /// <summary>
        /// void (VS_CC *setVideoInfo)(const VSVideoInfo *vi, int numOutputs, VSNode *node)
        /// </summary>
        public SetVideoInfoFunction setVideoInfo;
        /// <summary>
        /// const VSFormat *(VS_CC *getFrameFormat)(const VSFrameRef *f)
        /// </summary>
        public GetFrameFormatFunction getFrameFormat;
        /// <summary>
        /// int (VS_CC *getFrameWidth)(const VSFrameRef *f, int plane)
        /// </summary>
        public GetFrameWidthFunction getFrameWidth;
        /// <summary>
        /// int (VS_CC *getFrameHeight)(const VSFrameRef *f, int plane)
        /// </summary>
        public GetFrameHeightFunction getFrameHeight;
        /// <summary>
        /// const VSMap *(VS_CC *getFramePropsRO)(const VSFrameRef *f)
        /// </summary>
        public GetFramePropsROFunction getFramePropsRO;
        /// <summary>
        /// VSMap *(VS_CC *getFramePropsRW)(VSFrameRef *f)
        /// </summary>
        public GetFramePropsRWFunction getFramePropsRW;
        /// <summary>
        /// int (VS_CC *propNumKeys)(const VSMap *map)
        /// </summary>
        public PropNumKeysFunction propNumKeys;
        /// <summary>
        /// const char *(VS_CC *propGetKey)(const VSMap *map, int index)
        /// </summary>
        public PropGetKeyFunction propGetKey;
        /// <summary>
        /// int (VS_CC *propNumElements)(const VSMap *map, const char *key)
        /// </summary>
        public PropNumElementsFunction propNumElements;
        /// <summary>
        /// char (VS_CC *propGetType)(const VSMap *map, const char *key)
        /// </summary>
        public PropGetTypeFunction propGetType;
        /// <summary>
        /// int64_t(VS_CC *propGetInt)(const VSMap *map, const char *key, int index, int *error)
        /// </summary>
        public PropGetIntFunction propGetInt;
        /// <summary>
        /// double(VS_CC *propGetFloat)(const VSMap *map, const char *key, int index, int *error)
        /// </summary>
        public PropGetFloatFunction propGetFloat;
        /// <summary>
        /// const char *(VS_CC *propGetData)(const VSMap *map, const char *key, int index, int *error)
        /// </summary>
        public PropGetDataFunction propGetData;
        /// <summary>
        /// int (VS_CC *propGetDataSize)(const VSMap *map, const char *key, int index, int *error)
        /// </summary>
        public PropGetDataSizeFunction propGetDataSize;
        /// <summary>
        /// VSNodeRef *(VS_CC *propGetNode)(const VSMap *map, const char *key, int index, int *error)
        /// </summary>
        public PropGetNodeFunction propGetNode;
        /// <summary>
        /// const VSFrameRef *(VS_CC *propGetFrame)(const VSMap *map, const char *key, int index, int *error)
        /// </summary>
        public PropGetFrameFunction propGetFrame;
        /// <summary>
        /// VSFuncRef *(VS_CC *propGetFunc)(const VSMap *map, const char *key, int index, int *error)
        /// </summary>
        public PropGetFuncFunction propGetFunc;
        /// <summary>
        /// int (VS_CC *propDeleteKey)(VSMap *map, const char *key)
        /// </summary>
        public PropDeleteKeyFunction propDeleteKey;
        /// <summary>
        /// int (VS_CC *propSetInt)(VSMap *map, const char *key, int64_t i, int append)
        /// </summary>
        public PropSetIntFunction propSetInt;
        /// <summary>
        /// int (VS_CC *propSetFloat)(VSMap *map, const char *key, double d, int append)
        /// </summary>
        public PropSetFloatFunction propSetFloat;
        /// <summary>
        /// int (VS_CC *propSetData)(VSMap *map, const char *key, const char *data, int size, int append)
        /// </summary>
        public PropSetDataFunction propSetData;
        /// <summary>
        /// int (VS_CC *propSetNode)(VSMap *map, const char *key, VSNodeRef *node, int append)
        /// </summary>
        public PropSetNodeFunction propSetNode;
        /// <summary>
        /// int (VS_CC *propSetFrame)(VSMap *map, const char *key, const VSFrameRef *f, int append)
        /// </summary>
        public PropSetFrameFunction propSetFrame;
        /// <summary>
        /// int (VS_CC *propSetFunc)(VSMap *map, const char *key, VSFuncRef *func, int append)
        /// </summary>
        public PropSetFuncFunction propSetFunc;
        /// <summary>
        /// int64_t (VS_CC *setMaxCacheSize)(int64_t bytes, VSCore *core)
        /// </summary>
        public SetMaxCacheSizeFunction setMaxCacheSize;
        /// <summary>
        /// int (VS_CC *getOutputIndex)(VSFrameContext *frameCtx)
        /// </summary>
        public GetOutputIndexFunction getOutputIndex;
        /// <summary>
        /// VSFrameRef *(VS_CC *newVideoFrame2)(const VSFormat *format, int width, int height, const VSFrameRef **planeSrc, const int *planes, const VSFrameRef *propSrc, VSCore *core)
        /// </summary>
        public NewVideoFrame2Function newVideoFrame2;
        /// <summary>
        /// void (VS_CC *setMessageHandler)(VSMessageHandler handler, void *userData)
        /// </summary>
        public SetMessageHandlerFunction setMessageHandler;
        /// <summary>
        /// int (VS_CC *setThreadCount)(int threads, VSCore *core)
        /// </summary>
        public SetThreadCountFunction setThreadCount;
        /// <summary>
        /// const char *(VS_CC *getPluginPath)(const VSPlugin *plugin)
        /// </summary>
        public GetPluginPathFunction getPluginPath;
        /// <summary>
        /// const int64_t *(VS_CC *propGetIntArray)(const VSMap *map, const char *key, int *error)
        /// </summary>
        public PropGetIntArrayFunction propGetIntArray;
        /// <summary>
        /// const double *(VS_CC *propGetFloatArray)(const VSMap *map, const char *key, int *error)
        /// </summary>
        public PropGetFloatArrayFunction propGetFloatArray;
        /// <summary>
        /// int (VS_CC *propSetIntArray)(VSMap *map, const char *key, const int64_t *i, int size)
        /// </summary>
        public PropSetIntArrayFunction propSetIntArray;
        /// <summary>
        /// int (VS_CC *propSetFloatArray)(VSMap *map, const char *key, const double *d, int size)
        /// </summary>
        public PropSetFloatArrayFunction propSetFloatArray;
        /// <summary>
        /// void (VS_CC *logMessage)(int msgType, const char *msg)
        /// </summary>
        public LogMessageFunction logMessage;

        public delegate IntPtr CreateCoreFunction(IntPtr self, int a, float b);
        public delegate void FreeCoreFunction(IntPtr core);
        public delegate IntPtr GetCoreInfoFunction(IntPtr core);
        public delegate IntPtr CloneFrameRefFunction(IntPtr f);
        public delegate IntPtr CloneNodeRefFunction(IntPtr node);
        public delegate IntPtr CloneFuncRefFunction(IntPtr f);
        public delegate void FreeFrameFunction(IntPtr f);
        public delegate void freeNodeFunction(IntPtr node);
        public delegate void FreeFuncFunction(IntPtr f);
        public delegate IntPtr NewVideoFrameFunction(IntPtr format, int width, int height, IntPtr propSrc, IntPtr core);
        public delegate IntPtr CopyFrameFunction(IntPtr f, IntPtr core);
        public delegate void CopyFramePropsFunction(IntPtr src, IntPtr dst, IntPtr core);
        public delegate void RegisterFunctionFunction(IntPtr name, IntPtr args, IntPtr argsFunc, IntPtr functionData, IntPtr plugin);
        public delegate IntPtr GetPluginByIdFunction(IntPtr identifier, IntPtr core);
        public delegate IntPtr GetPluginByNsFunction(IntPtr ns, IntPtr core);
        public delegate IntPtr GetPluginsFunction(IntPtr core);
        public delegate IntPtr GetFunctionsFunction(IntPtr plugin);
        public delegate void CreateFilterFunction(IntPtr mapIn, IntPtr mapOut, IntPtr name, IntPtr init, IntPtr getFrame, IntPtr free, int filterMode, int flags, IntPtr instanceData, IntPtr core);
        public delegate void SetErrorFunction(IntPtr map, IntPtr errorMessage);
        public delegate IntPtr GetErrorFunction(IntPtr map);
        public delegate void SetFilterErrorFunction(IntPtr errorMessage, IntPtr frameCtx);
        public delegate IntPtr InvokeFunction(IntPtr plugin, IntPtr name, IntPtr args);
        public delegate IntPtr GetFormatPresetFunction(int id, IntPtr core);
        public delegate IntPtr RegisterFormatFunction(int colorFamily, int sampleType, int bitsPerSample, int subSamplingW, int subSamplingH, IntPtr core);
        public delegate IntPtr GetFrameFunction(int n, IntPtr node, IntPtr errorMsg, int bufSize);
        public delegate void GetFrameAsyncFunction(int n, IntPtr node, IntPtr callback, IntPtr userData);
        public delegate IntPtr GetFrameFilterFunction(int n, IntPtr node, IntPtr frameCtx);
        public delegate void RequestFrameFilterFunction(int n, IntPtr node, IntPtr frameCtx);
        public delegate void QueryCompletedFrameFunction(IntPtr node, IntPtr n, IntPtr frameCtx);
        public delegate void ReleaseFrameEarlyFunction(IntPtr node, int n, IntPtr frameCtx);
        public delegate int GetStrideFunction(IntPtr f, int plane);
        public delegate IntPtr GetReadPtrFunction(IntPtr f, int plane);
        public delegate IntPtr GetWritePtrFunction(IntPtr f, int plane);
        public delegate IntPtr CreateFuncFunction(IntPtr func, IntPtr userData, IntPtr free, IntPtr core, IntPtr vsapi);
        public delegate void CallFuncFunction(IntPtr func, IntPtr mapIn, IntPtr mapOut, IntPtr core, IntPtr vsapi);
        public delegate IntPtr CreateMapFunction(IntPtr api);
        public delegate void FreeMapFunction(IntPtr map);
        public delegate void ClearMapFunction(IntPtr map);
        public delegate IntPtr GetVideoInfoFunction(IntPtr node);
        public delegate void SetVideoInfoFunction(IntPtr vi, int numOutputs, IntPtr node);
        public delegate IntPtr GetFrameFormatFunction(IntPtr f);
        public delegate int GetFrameWidthFunction(IntPtr f, int plane);
        public delegate int GetFrameHeightFunction(IntPtr f, int plane);
        public delegate IntPtr GetFramePropsROFunction(IntPtr f);
        public delegate IntPtr GetFramePropsRWFunction(IntPtr f);
        public delegate int PropNumKeysFunction(IntPtr map);
        public delegate IntPtr PropGetKeyFunction(IntPtr map, int index);
        public delegate int PropNumElementsFunction(IntPtr map, IntPtr key);
        public delegate char PropGetTypeFunction(IntPtr map, IntPtr key);
        public delegate long PropGetIntFunction(IntPtr map, IntPtr key, int index, IntPtr error);
        public delegate double PropGetFloatFunction(IntPtr core);
        public delegate IntPtr PropGetDataFunction(IntPtr map, IntPtr key, int index, IntPtr error);
        public delegate int PropGetDataSizeFunction(IntPtr map, IntPtr key, int index, IntPtr error);
        public delegate IntPtr PropGetNodeFunction(IntPtr map, IntPtr key, int index, IntPtr error);
        public delegate IntPtr PropGetFrameFunction(IntPtr map, IntPtr key, int index, IntPtr error);
        public delegate IntPtr PropGetFuncFunction(IntPtr map, IntPtr key, int index, IntPtr error);
        public delegate int PropDeleteKeyFunction(IntPtr map, IntPtr key);
        public delegate int PropSetIntFunction(IntPtr map, IntPtr key, long i, int append);
        public delegate int PropSetFloatFunction(IntPtr map, IntPtr key, double d, int append);
        public delegate int PropSetDataFunction(IntPtr map, IntPtr key, IntPtr data, int size, int append);
        public delegate int PropSetNodeFunction(IntPtr map, IntPtr key, IntPtr node, int append);
        public delegate int PropSetFrameFunction(IntPtr map, IntPtr key, IntPtr f, int append);
        public delegate int PropSetFuncFunction(IntPtr map, IntPtr key, IntPtr func, int append);
        public delegate long SetMaxCacheSizeFunction(long bytes, IntPtr core);
        public delegate int GetOutputIndexFunction(IntPtr frameCtx);
        public delegate IntPtr NewVideoFrame2Function(IntPtr format, int width, int height, IntPtr planeSrc, IntPtr planes, IntPtr propSrc, IntPtr core);
        public delegate void SetMessageHandlerFunction(IntPtr handler, IntPtr userData);
        public delegate int SetThreadCountFunction(int threads, IntPtr core);
        public delegate IntPtr GetPluginPathFunction(IntPtr plugin);
        public delegate IntPtr PropGetIntArrayFunction(IntPtr map, IntPtr key, IntPtr error);
        public delegate IntPtr PropGetFloatArrayFunction(IntPtr map, IntPtr key, IntPtr error);
        public delegate int PropSetIntArrayFunction(IntPtr map, IntPtr key, IntPtr i, int size);
        public delegate int PropSetFloatArrayFunction(IntPtr map, IntPtr key, IntPtr d, int size);
        public delegate void LogMessageFunction(int msgType, IntPtr msg);
    }
}

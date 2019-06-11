using UnityEngine;

namespace Egret3DExportTools
{
    public static class MyLog
    {
        public static void Log(object message)
        {
            if(ExportSetting.instance.common.debugLog)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(object message)
        {
            if(ExportSetting.instance.common.debugLog)
            {
                Debug.LogWarning(message);
            }            
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }
    }
}
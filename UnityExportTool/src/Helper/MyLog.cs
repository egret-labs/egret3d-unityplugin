using UnityEngine;

namespace Egret3DExportTools
{
    public static class MyLog
    {
        public static void Log(object message)
        {
            if(ExportToolsSetting.debugLog)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(object message)
        {
            if(ExportToolsSetting.debugLog)
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
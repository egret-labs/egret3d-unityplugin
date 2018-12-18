using UnityEngine;

namespace Egret3DExportTools
{
    public static class MyLog
    {
        public static void Log(object message)
        {
            if(ExportToolsSetting.instance.debugLog)
            {
                Debug.Log(message);
            }
        }

        public static void LogWarning(object message)
        {
            if(ExportToolsSetting.instance.debugLog)
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
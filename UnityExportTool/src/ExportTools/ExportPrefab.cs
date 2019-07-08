using System.IO;
using UnityEngine;
namespace Egret3DExportTools
{
    public static class ExportPrefab
    {
        /**
         * 导出资源
         */
        public static void Export(GameObject curObj, string exportPath)
        {
            SerializeObject.Clear();
            string prefabPath = "Assets/" + curObj.name + ".prefab.json";
            //如果是Unity预制体那么就导出所在目录，如果是场景的一个普通GameObject,那么导出Assets下

            #if UNITY_2018_4_OR_NEWER
                if (UnityEditor.PrefabUtility.GetPrefabAssetType(curObj) == UnityEditor.PrefabAssetType.Variant)
                {
                    UnityEngine.Object parentObject = UnityEditor.PrefabUtility.GetPrefabInstanceHandle(curObj);
                    prefabPath = UnityEditor.AssetDatabase.GetAssetPath(parentObject) + ".json";
                }
            #else
                if (UnityEditor.PrefabUtility.GetPrefabType(curObj) == UnityEditor.PrefabType.PrefabInstance)
                {
                    UnityEngine.Object parentObject = UnityEditor.PrefabUtility.GetPrefabParent(curObj);
                    prefabPath = UnityEditor.AssetDatabase.GetAssetPath(parentObject) + ".json";
                }
            #endif
            //保存路径
            PathHelper.SetSceneOrPrefabPath(prefabPath);
            //预制体坐标归零，直接改坐标
            var savePosition = curObj.transform.localPosition;
            if (ExportSetting.instance.common.posToZero)
            {
                curObj.transform.localPosition = Vector3.zero;
            }

            SerializeObject.SerializeEntity(curObj);

            SerializeContext.Export(exportPath, prefabPath);
            curObj.transform.localPosition = savePosition;
        }
    }
}
using System.IO;
using UnityEngine;
namespace Egret3DExportTools
{
    public class ExportPrefabTools
    {
        /**
         * 导出资源
         */
        public static void ExportPrefab(GameObject curObj, string exportPath = "")
        {
            string prefabPath = "Assets/" + curObj.name + ".prefab.json";
            //如果是Unity预制体那么就导出所在目录，如果是场景的一个普通GameObject,那么导出Assets下
            if (UnityEditor.PrefabUtility.GetPrefabType(curObj) == UnityEditor.PrefabType.PrefabInstance)
            {
                UnityEngine.Object parentObject = UnityEditor.PrefabUtility.GetPrefabParent(curObj);
                prefabPath = UnityEditor.AssetDatabase.GetAssetPath(parentObject) + ".json";
            }
            //保存路径
            PathHelper.SetSceneOrPrefabPath(prefabPath);
            //预制体坐标归零，直接改坐标
            var savePosition = curObj.transform.localPosition;
            if (ExportToolsSetting.instance.prefabResetPos)
            {
                curObj.transform.localPosition = Vector3.zero;
            }

            SerializeObject.SerializeEntity(curObj);
            var relativePath = ExportConfig.instance.GetExportPath(prefabPath);
            var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(exportPath, relativePath));
            var fileDirectory = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
            if (!System.IO.Directory.Exists(fileDirectory))
            {
                System.IO.Directory.CreateDirectory(fileDirectory);
            }
            var gltfFile = File.CreateText(filePath);
            SerializeObject.currentData.Serialize(gltfFile);
            gltfFile.Close();

            curObj.transform.localPosition = savePosition;
        }
    }
}
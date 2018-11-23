using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Egret3DExportTools
{
    public class ExportSceneTools
    {
        public static void ExportScene(List<GameObject> roots, string exportPath = "")
        {
            string sceneName = PathHelper.CurSceneName;
            //导出场景
            try
            {
                ExportImageTools.instance.Clear();
                ResourceManager.instance.Clean();
                //路径
                string scenePath = sceneName + ".scene.json";
                PathHelper.SetSceneOrPrefabPath(scenePath);
                //Scene
                var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                MyJson_Object sceneJson = new MyJson_Object();
                sceneJson.SetUUID(scene.GetHashCode().ToString());//用场景名称的hashCode
                sceneJson.SetUnityID(scene.GetHashCode());
                sceneJson.SetClass("paper.Scene");
                sceneJson.SetString("name", sceneName.Substring(sceneName.LastIndexOf('/') + 1));

                sceneJson.SetColor("ambientColor", RenderSettings.ambientLight);
                sceneJson.SetNumber("lightmapIntensity", UnityEditor.Lightmapping.indirectOutputScale);
                //allGameObjects
                var gameObjectsJson = new MyJson_Array();
                sceneJson["gameObjects"] = gameObjectsJson;
                GameObject[] allObjs = GameObject.FindObjectsOfType<GameObject>();
                for (int i = 0; i < allObjs.Length; i++)
                {
                    gameObjectsJson.AddHashCode(allObjs[i]);
                }
                //lightmaps
                sceneJson["lightmaps"] = ExportSceneTools.AddLightmaps(exportPath);
                ResourceManager.instance.AddObjectJson(sceneJson);
                //序列化
                foreach (var root in roots)
                {
                    SerializeObject.Serialize(root);
                }
                ResourceManager.instance.ExportFiles(scenePath, exportPath);
                MyLog.Log("----场景导出成功----");
            }
            catch (System.Exception e)
            {
                MyLog.LogError(sceneName + "  : 导出失败-----------" + e.StackTrace);
            }
        }

        private static MyJson_Array AddLightmaps(string exportPath)
        {
            var lightmapsJson = new MyJson_Array();
            foreach (var lightmapData in LightmapSettings.lightmaps)
            {
                Texture2D lmTexture = lightmapData.lightmapColor;
                int lmID = lmTexture.GetInstanceID();
                if (!ResourceManager.instance.HaveCache(lmID))
                {
                    //格式转换
                    string suffix  = "png";
                    string relativePath = UnityEditor.AssetDatabase.GetAssetPath(lmTexture);
                    MyLog.Log("导出lightmap:" + relativePath);
                    string exrPath = Path.Combine(Application.dataPath, relativePath.Replace("Assets/", ""));
                    string ralPngPath = PathHelper.CheckFileName(relativePath.Substring(0, relativePath.LastIndexOf('/') + 1) + lmTexture.name + "." + suffix);
                    string pngPath = PathHelper.CheckFileName(Path.Combine(exportPath, ralPngPath));
                    // string ralPngPath = relativePath.Substring(0, relativePath.LastIndexOf('/') + 1) + lmTexture.name + "." + suffix;
                    // string pngPath = Path.Combine(exportPath, ralPngPath);

                    exrPath = exrPath.Replace("\\", "/");
                    pngPath = pngPath.Replace("\\", "/");
                    var bs = ExportImageTools.instance.EncodeToPNG(lmTexture);
                    ResourceManager.instance.AddFileBuffer(ralPngPath, bs);
                    // ExportImageTools.instance.AddExr(exrPath, pngPath);
                    //添加到 compList 和 assetsList
                    ResourceManager.instance.SaveTextureFormat(lmTexture, ralPngPath, ralPngPath + ".image", false);
                    string name = lmTexture.name + ".image.json";
                    var imgdescPath = PathHelper.CheckFileName(ralPngPath.Substring(0, ralPngPath.LastIndexOf("/") + 1) + name);

                    var assetIndex = ResourceManager.instance.AddAssetUrl(imgdescPath);

                    var assetItem = new MyJson_Tree();
                    assetItem.SetInt("asset", assetIndex);
                    lightmapsJson.Add(assetItem);
                }
            }
            // ExportImageTools.instance.ExrToPng();

            return lightmapsJson;
        }
    }
}
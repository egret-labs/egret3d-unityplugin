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
            SerializeObject.Clear();

            SerializeObject.currentData.Clear();
            //路径
            string scenePath = sceneName + ".scene.json";
            PathHelper.SetSceneOrPrefabPath(scenePath);

            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            var entity = SerializeObject.currentData.CreateEntity();

            var sceneComp = SerializeObject.currentData.CreateComponent(SerializeClass.Scene);
            sceneComp.properties.SetString("name", sceneName.Substring(sceneName.LastIndexOf('/') + 1));
            entity.AddComponent(sceneComp);

            var treeComp = SerializeObject.currentData.CreateComponent(SerializeClass.TreeNode);
            treeComp.properties.SetString("name", "Root");
            entity.AddComponent(treeComp);

            // 环境光和光照贴图
            var sceneLightComp = SerializeObject.currentData.CreateComponent(SerializeClass.SceneLight);
            sceneLightComp.properties.SetColor("ambientColor", RenderSettings.ambientLight);
            sceneLightComp.properties.SetNumber("lightmapIntensity", UnityEditor.Lightmapping.indirectOutputScale);
            sceneLightComp.SetLightmaps(exportPath);
            entity.AddComponent(sceneLightComp);

            // 雾
            if (RenderSettings.fog)
            {
                var fogComp = SerializeObject.currentData.CreateComponent(SerializeClass.Fog);
                if (RenderSettings.fogMode == FogMode.Linear)
                {
                    fogComp.properties.SetInt("mode", 0);
                    fogComp.properties.SetNumber("near", RenderSettings.fogStartDistance);
                    fogComp.properties.SetNumber("far", RenderSettings.fogEndDistance);
                }
                else
                {
                    fogComp.properties.SetInt("mode", 1);
                    fogComp.properties.SetNumber("far", RenderSettings.fogDensity);
                }
                fogComp.properties.SetColor("color", RenderSettings.fogColor);
                entity.AddComponent(fogComp);
            }

            foreach (var child in roots)
            {
                var childEntity = SerializeObject.SerializeEntity(child);
                if (childEntity != null)
                {
                    treeComp.AddChild(childEntity.transform);
                }
            }

            {
                var relativePath = ExportConfig.instance.GetExportPath(scenePath);
                var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(exportPath, relativePath));
                var fileDirectory = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
                if (!System.IO.Directory.Exists(fileDirectory))
                {
                    System.IO.Directory.CreateDirectory(fileDirectory);
                }
                var gltfFile = File.CreateText(filePath);
                SerializeObject.currentData.Serialize(gltfFile);
                gltfFile.Close();
                MyLog.Log("---导出文件:" + relativePath);
            }

            {
                foreach (var asset in SerializeObject.assetsData.Values)
                {
                    var relativePath = ExportConfig.instance.GetExportPath(asset.uri);
                    var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(exportPath, relativePath));
                    var fileDirectory = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
                    if (!System.IO.Directory.Exists(fileDirectory))
                    {
                        System.IO.Directory.CreateDirectory(fileDirectory);
                    }
                    System.IO.File.WriteAllBytes(filePath, asset.buffer);
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Egret3DExportTools
{
    public static class ExportScene
    {
        public static void Export(List<GameObject> roots, string exportPath)
        {
            string sceneName = PathHelper.CurSceneName;
            SerializeObject.Clear();
            //路径
            string scenePath = sceneName + ".scene.json";
            PathHelper.SetSceneOrPrefabPath(scenePath);

            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            var sceneEntity = SerializeObject.currentData.CreateEntity();
            
            var sceneComp = SerializeObject.currentData.CreateComponent(SerializeClass.Scene);
            sceneComp.properties.SetString("name", sceneName.Substring(sceneName.LastIndexOf('/') + 1));
            sceneEntity.AddComponent(sceneComp);
            
            var treeComp = SerializeObject.currentData.CreateComponent(SerializeClass.TreeNode);
            treeComp.properties.SetString("name", "Root");
            treeComp.properties.SetReference("scene", sceneComp.uuid);
            sceneEntity.AddComponent(treeComp);

            // 环境光和光照贴图
            var sceneLightComp = SerializeObject.currentData.CreateComponent(SerializeClass.SceneLight);
            sceneLightComp.properties.SetColor("ambientColor", RenderSettings.ambientLight);
            sceneLightComp.properties.SetNumber("lightmapIntensity", UnityEditor.Lightmapping.indirectOutputScale);
            sceneLightComp.properties.SetLightmaps(exportPath);
            sceneEntity.AddComponent(sceneLightComp);

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
                sceneEntity.AddComponent(fogComp);
            }

            foreach (var child in roots)
            {
                var childEntity = SerializeObject.SerializeEntity(child);
                if (childEntity != null)
                {
                    treeComp.AddChild(childEntity.transform);
                }
            }

            SerializeContext.Export(exportPath, scenePath);
        }
    }
}
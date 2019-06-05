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
            ExportImageTools.instance.Clear();
            ResourceManager.instance.Clean();
            SerializeObject.Clear();

            SerializeObject.currentData.Clear();
            //路径
            string scenePath = sceneName + ".scene.json";
            PathHelper.SetSceneOrPrefabPath(scenePath);

            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

            var entity = SerializeObject.currentData.CreateEntity();

            var sceneComp = ComponentData.Create(SerializeClass.Scene);
            sceneComp.SetString("name", sceneName.Substring(sceneName.LastIndexOf('/') + 1));
            entity.AddComponent(sceneComp);

            var treeComp = ComponentData.Create(SerializeClass.TreeNode);
            treeComp.SetString("name", "Root");
            entity.AddComponent(treeComp);

            // 环境光和光照贴图
            var sceneLightComp = ComponentData.Create(SerializeClass.SceneLight);
            sceneLightComp.SetColor("ambientColor", RenderSettings.ambientLight);
            sceneLightComp.SetNumber("lightmapIntensity", UnityEditor.Lightmapping.indirectOutputScale);
            sceneLightComp.SetLightmaps(exportPath);
            entity.AddComponent(sceneLightComp);

            // 雾
            if (RenderSettings.fog)
            {
                var fogComp = ComponentData.Create(SerializeClass.Fog);
                if (RenderSettings.fogMode == FogMode.Linear)
                {
                    fogComp.SetInt("mode", 0);
                    fogComp.SetNumber("near", RenderSettings.fogStartDistance);
                    fogComp.SetNumber("far", RenderSettings.fogEndDistance);
                }
                else
                {
                    fogComp.SetInt("mode", 1);
                    fogComp.SetNumber("density", RenderSettings.fogDensity);
                }
                fogComp.SetColor("color", RenderSettings.fogColor);
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

                var relativePath = scenePath;
                relativePath = relativePath.Replace("Assets", ExportConfig.instance.rootDir);
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
                    var relativePath = asset.uri;
                    relativePath = relativePath.Replace("Assets", ExportConfig.instance.rootDir);
                    var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(exportPath, relativePath));
                    var fileDirectory = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
                    if (!System.IO.Directory.Exists(fileDirectory))
                    {
                        System.IO.Directory.CreateDirectory(fileDirectory);
                    }
                    System.IO.File.WriteAllBytes(filePath, asset.buffer);
                }
            }

            //创建路径
            // var fileDirectory = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
            // if (!System.IO.Directory.Exists(fileDirectory))
            // {
            //     System.IO.Directory.CreateDirectory(fileDirectory);
            // }
            // System.IO.File.WriteAllBytes(filePath, fileBuffer.Value);

        }
        // public static void ExportScene2(List<GameObject> roots, string exportPath = "")
        // {
        //     string sceneName = PathHelper.CurSceneName;
        //     ExportImageTools.instance.Clear();
        //     ResourceManager.instance.Clean();
        //     //路径
        //     string scenePath = sceneName + ".scene.json";
        //     PathHelper.SetSceneOrPrefabPath(scenePath);

        //     var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();

        //     MyJson_Object sceneEntity = new MyJson_Object();
        //     sceneEntity.SetSerializeClass(scene.GetHashCode(), SerializeClass.GameEntity);
        //     ResourceManager.instance.AddObjectJson(sceneEntity);

        //     MyJson_Array compList = new MyJson_Array();
        //     sceneEntity["components"] = compList;



        //     {
        //         MyJson_Object sceneComp = new MyJson_Object();
        //         sceneComp.SetSerializeClass(sceneComp.GetHashCode(), SerializeClass.Scene);
        //         sceneComp.SetString("name", sceneName.Substring(sceneName.LastIndexOf('/') + 1));

        //         compList.AddHashCode(sceneComp);
        //         ResourceManager.instance.AddCompJson(sceneComp);


        //         MyJson_Object treeNode = new MyJson_Object();
        //         treeNode.SetSerializeClass(treeNode.GetHashCode(), SerializeClass.TreeNode);
        //         treeNode.SetString("name", "Root");
        //         treeNode.SetHashCode("scene", sceneComp);

        //         MyJson_Array children = new MyJson_Array();
        //         treeNode["children"] = children;
        //         //
        //         compList.AddHashCode(treeNode);
        //         ResourceManager.instance.AddCompJson(treeNode);

        //         {
        //             // 环境光和光照贴图
        //             MyJson_Object sceneLight = new MyJson_Object();
        //             sceneLight.SetSerializeClass(sceneLight.GetHashCode(), SerializeClass.SceneLight);

        //             sceneLight.SetColor("ambientColor", RenderSettings.ambientLight);

        //             sceneLight["lightmaps"] = ExportSceneTools.AddLightmaps(exportPath);
        //             sceneLight.SetNumber("lightmapIntensity", UnityEditor.Lightmapping.indirectOutputScale);

        //             //
        //             compList.AddUUID(sceneLight);
        //             ResourceManager.instance.AddCompJson(sceneLight);
        //         }

        //         {
        //             // 雾
        //             if (RenderSettings.fog)
        //             {
        //                 MyJson_Object fog = new MyJson_Object();
        //                 fog.SetSerializeClass(fog.GetHashCode(), SerializeClass.Fog);

        //                 if (RenderSettings.fogMode == FogMode.Linear)
        //                 {
        //                     fog.SetInt("mode", 0);
        //                     fog.SetNumber("near", RenderSettings.fogStartDistance);
        //                     fog.SetNumber("far", RenderSettings.fogEndDistance);
        //                 }
        //                 else
        //                 {
        //                     fog.SetInt("mode", 1);
        //                     fog.SetNumber("density", RenderSettings.fogDensity);
        //                 }

        //                 fog.SetColor("color", RenderSettings.fogColor);
        //                 //
        //                 compList.AddUUID(fog);
        //                 ResourceManager.instance.AddCompJson(fog);
        //             }
        //         }

        //         //序列化
        //         foreach (var child in roots)
        //         {
        //             var childJson = SerializeObject.Serialize(child);
        //             if (childJson != null)
        //             {
        //                 children.AddHashCode(childJson);
        //             }
        //         }
        //     }


        //     // try
        //     {

        //         ResourceManager.instance.ExportFiles(scenePath, exportPath);
        //         MyLog.Log("----场景导出成功----");
        //     }
        //     // catch (System.Exception e)
        //     // {
        //     //     MyLog.LogError(sceneName + "  : 导出失败-----------" + e.StackTrace);
        //     // }
        // }

        // public static MyJson_Array AddLightmaps(string exportPath)
        // {
        //     var lightmapsJson = new MyJson_Array();
        //     foreach (var lightmapData in LightmapSettings.lightmaps)
        //     {
        //         Texture2D lmTexture = lightmapData.lightmapColor;
        //         var imgdescPath = SerializeObject.Serialize(lmTexture, AssetType.Texture);
        //         var assetIndex = ResourceManager.instance.AddAssetUrl(imgdescPath);

        //         var assetItem = new MyJson_Tree();
        //         assetItem.SetInt("asset", assetIndex);
        //         lightmapsJson.Add(assetItem);
        //     }

        //     return lightmapsJson;
        // }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Egret3DExportTools
{
    public static class SerializeClass
    {
        public const string GameEntity = "GameEntity";
        public const string Scene = "Scene";
        public const string Fog = "Fog";
        public const string SceneLight = "SceneLight";
        public const string Animation = "Animation";
        public const string BoxCollider = "BoxCollider";
        public const string SphereCollider = "SphereCollider";
        public const string Camera = "Camera";
        public const string MeshFilter = "MeshFilter";
        public const string MeshRenderer = "MeshRenderer";
        public const string ParticleComponent = "ParticleComponent";
        public const string ParticleRenderer = "ParticleRenderer";
        public const string SkinnedMeshRenderer = "SkinnedMeshRenderer";
        public const string TreeNode = "TreeNode";
        public const string Transform = "Transform";
        public const string DirectionalLight = "DirectionalLight";
        public const string SpotLight = "SpotLight";
        public const string LightShadow = "LightShadow";
        public const string AssetEntity = "AssetEntity";
        public const string Defines = "Defines";
        public const string Skybox = "Skybox";
        public const string StaticBatching = "StaticBatching";

        //        
        public const string Texture2D = "Texture2D";
        // public const string Texture2DArray = "Texture2DArray";
        public const string Cubemap = "Cubemap";
        public const string Material = "Material";
        public const string Mesh = "Mesh";
        public const string AnimationClip = "AnimationClip";
    }

    public static class SerializeObject
    {
        public static SerializeContext currentData = new SerializeContext();
        public static Dictionary<string, AssetData> assetsData = new Dictionary<string, AssetData>();
        public static Transform currentTarget;
        private readonly static Dictionary<string, List<ISerializer>> componentParsers = new Dictionary<string, List<ISerializer>>();
        private readonly static Dictionary<string, ISerializer> assetParsers = new Dictionary<string, ISerializer>();
        /**
         * 注册可序列化组件。
         * @param parser 序列化组件实例。
         * @param compType 对应Unity的类型。
         * @param className 对应Egret3d的类名(例如：egret3d.Animation)。
         */
        private static void RegisterComponent(ISerializer parser, System.Type compType, string className)
        {
            parser.compType = compType;
            parser.className = className;
            if (!componentParsers.ContainsKey(compType.Name))
            {
                componentParsers[compType.Name] = new List<ISerializer>();
            }
            componentParsers[compType.Name].Add(parser);
        }

        private static void RegisterAsset(ISerializer parser, System.Type compType, string className)
        {
            parser.compType = compType;
            parser.className = className;
            if (!assetParsers.ContainsKey(compType.Name))
            {
                assetParsers.Add(compType.Name, parser);
            }
        }
        public static void Clear()
        {
            currentData.Clear();
            assetsData.Clear();
        }
        /**
        *是否可以序列化该组件
        */
        private static bool IsComponentSupport(string className)
        {
            return componentParsers.ContainsKey(className);
        }
        public static void Initialize()
        {
            //初始化组件管理器
            componentParsers.Clear();
            RegisterComponent(new AnimatorSerializer(), typeof(UnityEngine.Animator), SerializeClass.Animation);
            RegisterComponent(new AnimationSerializer(), typeof(UnityEngine.Animation), SerializeClass.Animation);
            RegisterComponent(new BoxColliderSerializer(), typeof(UnityEngine.BoxCollider), SerializeClass.BoxCollider);
            RegisterComponent(new SphereColliderSerializer(), typeof(UnityEngine.SphereCollider), SerializeClass.SphereCollider);
            RegisterComponent(new CameraSerializer(), typeof(UnityEngine.Camera), SerializeClass.Camera);
            RegisterComponent(new MeshFilterSerializer(), typeof(UnityEngine.MeshFilter), SerializeClass.MeshFilter);
            RegisterComponent(new MeshRendererSerializer(), typeof(UnityEngine.MeshRenderer), SerializeClass.MeshRenderer);
            RegisterComponent(new ParticleSystemSerializer(), typeof(UnityEngine.ParticleSystem), SerializeClass.ParticleComponent);
            RegisterComponent(new ParticleRendererSerializer(), typeof(UnityEngine.ParticleSystemRenderer), SerializeClass.ParticleRenderer);
            RegisterComponent(new SkinnedMeshRendererSerializer(), typeof(UnityEngine.SkinnedMeshRenderer), SerializeClass.SkinnedMeshRenderer);
            RegisterComponent(new TransformSerializer(), typeof(UnityEngine.Transform), SerializeClass.Transform);
            RegisterComponent(new DirectionalLightSerializer(), typeof(UnityEngine.Light), SerializeClass.DirectionalLight);
            RegisterComponent(new SpotLightSerializer(), typeof(UnityEngine.Light), SerializeClass.SpotLight);
            RegisterComponent(new SkyboxSerializer(), typeof(UnityEngine.Skybox), SerializeClass.Skybox);

            //初始化资源
            assetParsers.Clear();
            RegisterAsset(new GLTFTextureSerializer(), typeof(UnityEngine.Texture2D), SerializeClass.Texture2D);
            RegisterAsset(new GLTFTextureArraySerializer(), typeof(Texture2DArrayData), SerializeClass.Texture2D);
            RegisterAsset(new GLTFCubemapSerializer(), typeof(UnityEngine.Cubemap), SerializeClass.Cubemap);
            RegisterAsset(new GLTFMeshSerializer(), typeof(UnityEngine.Mesh), SerializeClass.Mesh);
            RegisterAsset(new GLTFMaterialSerializer(), typeof(UnityEngine.Material), SerializeClass.Material);
            RegisterAsset(new GLTFAnimationClipSerializer(), typeof(UnityEngine.AnimationClip), SerializeClass.AnimationClip);
        }
        public static EntityData SerializeEntity(GameObject obj)
        {
            //未激活的不导出
            if ((!ExportSetting.instance.common.exportUnactivatedObject && !obj.activeInHierarchy))
            {
                MyLog.Log(obj.name + "对象未激活");
                return null;
            }

            if (obj.GetComponent<RectTransform>() != null)
            {
                return null;
            }
            MyLog.Log("对象:" + obj.name);
            var entityData = currentData.CreateEntity();
            var components = obj.GetComponents<Component>();

            var index = 0;//TODO
            foreach (var comp in components)
            {
                if (comp is Animator)
                {
                    components[index] = components[0];
                    components[0] = comp;
                }

                index++;
            }

            foreach (var comp in components)
            {
                if (comp == null)
                {
                    MyLog.LogWarning("空的组件");
                    continue;
                }
                string compClass = comp.GetType().Name;
                // MyLog.Log("组件:" + compClass);
                if (!ExportSetting.instance.common.exportUnactivatedComp)
                {
                    //利用反射查看组件是否激活，某些组件的enabled不再继承链上，只能用反射，比如BoxCollider
                    var property = comp.GetType().GetProperty("enabled");
                    if (property != null && !((bool)property.GetValue(comp, null)))
                    {
                        MyLog.Log(obj.name + "组件未激活");
                        continue;
                    }
                }

                if (!SerializeObject.IsComponentSupport(compClass))
                {
                    MyLog.LogWarning("不支持的组件： " + compClass);
                    continue;
                }
                var compData = SerializeObject.SerializeComponent(obj, compClass, comp, entityData);
                if (compData != null)
                {
                    entityData.AddComponent(compData);
                    MyLog.Log("--导出组件:" + compClass);
                }
                else
                {
                    MyLog.LogWarning("组件： " + compClass + " 导出失败");
                }
            }

            return entityData;
        }
        private static ComponentData SerializeComponent(UnityEngine.GameObject obj, string compClass, UnityEngine.Component comp, EntityData entityData)
        {
            currentTarget = obj.transform;
            var parserList = componentParsers[compClass];
            if (parserList == null)
            {
                return null;
            }

            foreach (var parser in parserList)
            {
                if (!parser.Match(comp))
                {
                    continue;
                }
                var compData = SerializeObject.currentData.CreateComponent(parser.className);
                compData.entity = entityData;
                parser.Serialize(comp, compData);
                return compData;
            }
            currentTarget = null;

            return null;
        }
        public static AssetData SerializeAsset(UnityEngine.Object obj)
        {
            var path = PathHelper.GetAssetPath(obj);
            if (assetsData.ContainsKey(path))
            {
                return assetsData[path];
            }

            var assetData = AssetData.Create(path);
            assetsData.Add(path, assetData);
            var parser = assetParsers[obj.GetType().Name];

            parser.Serialize(obj, assetData);
            return assetData;
        }
    }
}

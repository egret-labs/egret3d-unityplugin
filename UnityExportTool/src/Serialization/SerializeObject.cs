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
        public const string Material = "Material";
        public const string Defines = "Defines";
    }

    public enum AssetType
    {
        Texture,
        Material,
        Mesh,
        Animation
    }
    public static class SerializeObject
    {
        public static SerializedData currentData = new SerializedData();
        public static Dictionary<string, AssetData> assetsData = new Dictionary<string, AssetData>();
        public static Transform currentTarget;
        private readonly static Dictionary<string, List<IComponentSerializer>> componentParsers = new Dictionary<string, List<IComponentSerializer>>();
        private readonly static Dictionary<AssetType, IAssetSerializer> assetParsers = new Dictionary<AssetType, IAssetSerializer>();
        /**
         * 注册可序列化组件。
         * @param parser 序列化组件实例。
         * @param compType 对应Unity的类型。
         * @param className 对应Egret3d的类名(例如：egret3d.Animation)。
         */
        private static void RegisterComponent(IComponentSerializer parser, System.Type compType, string className)
        {
            parser.compType = compType;
            parser.className = className;
            if (!componentParsers.ContainsKey(compType.Name))
            {
                componentParsers[compType.Name] = new List<IComponentSerializer>();
            }
            componentParsers[compType.Name].Add(parser);
        }

        private static void RegisterAsset(IAssetSerializer parser, AssetType type)
        {
            assetParsers.Add(type, parser);
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

            //初始化资源
            assetParsers.Clear();
            RegisterAsset(new GLTFTextureSerializer(), AssetType.Texture);
            RegisterAsset(new GLTFMeshSerializer(), AssetType.Mesh);
            RegisterAsset(new GLTFMaterialSerializer(), AssetType.Material);
            RegisterAsset(new GLTFAnimationSerializer(), AssetType.Animation);
        }
        public static EntityData SerializeEntity(GameObject obj)
        {
            //未激活的不导出
            if ((!ExportToolsSetting.instance.exportUnactivatedObject && !obj.activeInHierarchy))
            {
                MyLog.Log(obj.name + "对象未激活");
                return null;
            }

            if (obj.GetComponent<RectTransform>() != null)
            {
                return null;
            }

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
                MyLog.Log("组件:" + compClass);
                if (!ExportToolsSetting.instance.exportUnactivatedComp)
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
        public static AssetData SerializeAsset(UnityEngine.Object obj, string path, AssetType type)
        {
            if (assetsData.ContainsKey(path))
            {
                return assetsData[path];
            }

            var assetData = AssetData.Create(path);
            assetsData.Add(path, assetData);
            var parser = assetParsers[type];

            parser.Serialize(obj, assetData);
            return assetData;
        }
    }
}

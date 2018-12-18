using System;
using System.Collections.Generic;
using UnityEngine;

namespace Egret3DExportTools
{
    public static class SerializeObject
    {
        private readonly static Int32[] LAYER = { 0x000002, 0x000004, 0x000008, 0x000010, 0x000020, 0x000040, 0x000080, 0x0000f0, 0x000100, 0x000200, 0x000400, 0x000800, 0x000f00 };
        private readonly static Dictionary<string, List<IComponentParser>> componentParsers = new Dictionary<string, List<IComponentParser>>();

        public static void Initialize()
        {
            //初始化组件管理器
            componentParsers.Clear();
            // RegComponentParser(new AniPlayerParser(),               typeof(FB.PosePlus.AniPlayer),              "egret3d.Animation");
            RegComponentParser(new AnimatorParser(), typeof(UnityEngine.Animator), "egret3d.Animation");
            RegComponentParser(new AnimationParser(), typeof(UnityEngine.Animation), "egret3d.Animation");
            RegComponentParser(new BoxColliderParser(), typeof(UnityEngine.BoxCollider), "egret3d.BoxCollider");
            RegComponentParser(new SphereColliderParser(), typeof(UnityEngine.SphereCollider), "egret3d.SphereCollider");
            RegComponentParser(new CameraParser(), typeof(UnityEngine.Camera), "egret3d.Camera"); ;
            RegComponentParser(new MeshFilterParser(), typeof(UnityEngine.MeshFilter), "egret3d.MeshFilter");
            RegComponentParser(new MeshRendererParser(), typeof(UnityEngine.MeshRenderer), "egret3d.MeshRenderer"); ;
            RegComponentParser(new ParticleSystemParser(), typeof(UnityEngine.ParticleSystem), "egret3d.particle.ParticleComponent");
            RegComponentParser(new ParticleSystemRendererParser(), typeof(UnityEngine.ParticleSystemRenderer), "egret3d.particle.ParticleRenderer");
            RegComponentParser(new SkinnedMeshRendererParser(), typeof(UnityEngine.SkinnedMeshRenderer), "egret3d.SkinnedMeshRenderer");
            RegComponentParser(new TransformParser(), typeof(UnityEngine.Transform), "egret3d.Transform");
            RegComponentParser(new DirectionalLightParser(), typeof(UnityEngine.Light), "egret3d.DirectionalLight");
            RegComponentParser(new SpotLightParser(), typeof(UnityEngine.Light), "egret3d.SpotLight");
        }
        /**
         * 注册可序列化组件。
         * @param parser 序列化组件实例。
         * @param compType 对应Unity的类型。
         * @param className 对应Egret3d的类名(例如：egret3d.Animation)。
         */
        public static void RegComponentParser(IComponentParser parser, System.Type compType, string className)
        {
            parser.compType = compType;
            parser.className = className;
            if (!componentParsers.ContainsKey(compType.Name))
            {
                componentParsers[compType.Name] = new List<IComponentParser>();
            }
            componentParsers[compType.Name].Add(parser);
        }
        /**
        *是否可以序列化该组件
        */
        public static bool IsComponentSupport(string className)
        {
            return componentParsers.ContainsKey(className);
        }
        public static void Serialize(GameObject obj)
        {
            //未激活的不导出
            if ((!ExportToolsSetting.instance.exportUnactivatedObject && !obj.activeInHierarchy))
            {
                MyLog.Log(obj.name + "对象未激活");
                return;
            }

            if (obj.GetComponent<RectTransform>() != null)
            {
                return;
            }
            MyLog.Log("导出对象:" + obj.name);
            MyJson_Object item = new MyJson_Object();
            item.SetUUID(obj.GetInstanceID().ToString());
            item.SetUnityID(obj.GetInstanceID());
            item.SetClass("paper.GameObject");
            item.SetString("name", obj.name);
            item.SetString("tag", obj.tag);
            var layerMask = 1 << obj.layer;
            item.SetInt("layer", layerMask);
            // item.SetInt("layer", LAYER[obj.layer >= LAYER.Length ? 0 : obj.layer]);;
            item.SetBool("isStatic", obj.isStatic);

            var componentsItem = new MyJson_Array();
            item["components"] = componentsItem;
            ResourceManager.instance.AddObjectJson(item);

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

            //遍历填充组件
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
                if (SerializeObject.SerializedComponent(obj, compClass, comp))
                {
                    MyLog.Log("--导出组件:" + compClass);
                    componentsItem.AddHashCode(comp);
                }
                else
                {
                    MyLog.LogWarning("组件： " + compClass + " 导出失败");
                }
            }
            //遍历子对象
            if (obj.transform.childCount > 0)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    var child = obj.transform.GetChild(i).gameObject;
                    Serialize(child);
                }
            }
        }
        /**
        *序列化组件数据
        */
        private static bool SerializedComponent(UnityEngine.GameObject obj, string compClass, UnityEngine.Component comp)
        {
            var parserList = componentParsers[compClass];
            if (parserList == null)
            {
                return false;
            }

            var flag = false;
            foreach (var parser in parserList)
            {
                var compJson = new MyJson_Object();
                //组件必须拥有的属性
                compJson.SetComponent(comp.GetInstanceID(), parser.className);
                if (!parser.WriteToJson(obj, comp, compJson))
                {
                    continue;
                }

                ResourceManager.instance.AddCompJson(compJson);
                flag = true;
            }

            return flag;
        }
    }
}

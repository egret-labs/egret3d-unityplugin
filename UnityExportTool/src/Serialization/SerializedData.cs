namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using UnityEngine;

    public interface ISerizileData
    {
        string uuid { get; set; }
        string __class { get; set; }
    }

    public abstract class SerizileData : ISerizileData
    {
        public const string KEY_UUID = "uuid";
        public const string KEY_COMPONENTS = "components";
        public const string KEY_CHILDREN = "children";
        public const string KEY_CLASS = "__class";
        public const string KEY_ASSET = "__asset";
        public const string KEY_EXTRAS = "__extras";


        protected string _uuid;
        protected string _className;
        public string uuid { get { return this._uuid; } set { this._uuid = value; } }
        public string __class { get { return this._className; } set { this._className = value; } }
    }
    public class EntityData : SerizileData
    {
        public UnityEngine.GameObject unityEntity;
        public ComponentData transform;
        public List<ComponentData> components = new List<ComponentData>();

        public void AddComponent(ComponentData comp)
        {
            if (!this.components.Contains(comp))
            {
                this.components.Add(comp);
            }

            if (comp.__class == SerializeClass.Transform || comp.__class == SerializeClass.TreeNode)
            {
                if (this.transform != null)
                {
                    MyLog.LogWarning("一个实体多个transform组件");
                }
                this.transform = comp;
            }

            comp.entity = this;
        }

        public void Serizile(SerializedData data, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(SerizileData.KEY_UUID);
            writer.WriteValue(data.GetUUID(this.GetHashCode()));

            writer.WritePropertyName(SerizileData.KEY_CLASS);
            writer.WriteValue(this.__class);

            writer.WritePropertyName(SerizileData.KEY_COMPONENTS);
            writer.WriteStartArray();
            foreach (var comp in this.components)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(SerizileData.KEY_UUID);
                writer.WriteValue(data.GetUUID(comp.GetHashCode()));
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    public class ComponentData : SerizileData
    {
        public EntityData entity;
        public Dictionary<string, IJsonNode> props = new Dictionary<string, IJsonNode>();
        public static ComponentData Create(string className)
        {
            ComponentData comp = new ComponentData();
            comp.__class = className;
            return comp;
        }

        private void SerializeProperty(string name, IJsonNode propertyValue, SerializedData data, JsonWriter writer)
        {
            if(name != string.Empty)
            {
                writer.WritePropertyName(name);
            }
            
            if (propertyValue is MyJson_Reference)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(KEY_UUID);
                writer.WriteValue(data.GetUUID(propertyValue.value.GetHashCode()));
                writer.WriteEndObject();
            }
            else if (propertyValue is MyJson_Asset)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(KEY_ASSET);
                writer.WriteValue(data.GetAssetIndex((string)propertyValue.value));
                writer.WriteEndObject();
            }
            else if (propertyValue is MyJson_Tree)
            {
                var dic = propertyValue as Dictionary<string, IJsonNode>;
                writer.WriteStartObject();
                foreach (var item in dic)
                {
                    this.SerializeProperty(item.Key, item.Value, data, writer);
                }
                writer.WriteEndObject();
            }
            else if (propertyValue is MyJson_Array)
            {
                writer.WriteStartArray();
                foreach (var json in (propertyValue as List<IJsonNode>))
                {
                    this.SerializeProperty(string.Empty, json, data, writer);
                }
                writer.WriteEndArray();
            }
            else
            {
                writer.WriteValue(propertyValue.value);
            }
        }

        public void AddChild(ComponentData child)
        {
            IJsonNode children;
            if (!this.props.TryGetValue(KEY_CHILDREN, out (children)))
            {
                children = new MyJson_Array();
                this.props.Add(KEY_CHILDREN, children);
            }
            (children as MyJson_Array).Add(new MyJson_Reference(child));
        }

        public void Serizile(SerializedData data, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(KEY_UUID);
            writer.WriteValue(data.GetUUID(this.GetHashCode()));

            writer.WritePropertyName(KEY_CLASS);
            writer.WriteValue(this.__class);

            foreach (var name in this.props.Keys)
            {
                var propertyValue = this.props[name];
                // Debug.Log("name:" + name + " value:" + propertyValue);
                this.SerializeProperty(name, propertyValue, data, writer);
            }

            writer.WriteEndObject();
        }

    }

    public class AssetData
    {
        public static AssetData Create(string uri)
        {
            var asset = new AssetData();
            asset.uri = uri;
            return asset;
        }
        public string uri = string.Empty;
        public byte[] buffer;
    }

    public class SerializedData
    {
        public const string VERSION = "5";//资源版本号
        public List<AssetData> assets = new List<AssetData>();
        public List<EntityData> entities = new List<EntityData>();

        private int _uuidIndex = 0;
        private readonly Dictionary<int, int> _uuidDic = new Dictionary<int, int>();

        public void Clear()
        {
            this.assets.Clear();
            this.entities.Clear();

            this._uuidIndex = 0;
            this._uuidDic.Clear();
        }

        public string GetUUID(int unityHash)
        {
            int newHash;
            if (this._uuidDic.TryGetValue(unityHash, out newHash))
            {
                return newHash.ToString();
            }
            newHash = this._uuidIndex++;
            this._uuidDic[unityHash] = newHash;
            return newHash.ToString();
        }

        public int GetAssetIndex(string url)
        {
            for (int i = 0, l = this.assets.Count; i < l; i++)
            {
                var asset = this.assets[i];
                if (asset.uri == url)
                {
                    return i;
                }
            }

            return -1;
        }

        public EntityData CreateEntity()
        {
            var entity = new EntityData();
            entity.__class = SerializeClass.GameEntity;
            this.entities.Add(entity);
            return entity;
        }

        public void AddAsset(AssetData asset)
        {
            for (int i = 0, l = this.assets.Count; i < l; i++)
            {
                var item = this.assets[i];
                if (item.uri == asset.uri)
                {
                    this.assets[i] = asset;
                    return;
                }
            }

            this.assets.Add(asset);
        }

        public AssetData CreateAsset(string uri)
        {
            foreach (var item in this.assets)
            {
                if (item.uri == uri)
                {
                    return item;
                }
            }
            //
            var asset = new AssetData();
            asset.uri = uri;
            this.assets.Add(asset);
            return asset;
        }

        public virtual void Serialize(TextWriter textWriter)
        {
            JsonWriter jsonWriter = new JsonTextWriter(textWriter);
            if (Egret3DExportTools.ExportToolsSetting.instance.jsonFormatting)
            {
                jsonWriter.Formatting = Formatting.Indented;
            }
            else
            {
                jsonWriter.Formatting = Formatting.None;
            }
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("assets");
            jsonWriter.WriteStartArray();
            foreach (var asset in this.assets)
            {
                jsonWriter.WriteValue(asset.uri.Replace("Assets", ExportConfig.instance.rootDir));
            }
            jsonWriter.WriteEndArray();

            jsonWriter.WritePropertyName("entities");
            jsonWriter.WriteStartArray();
            foreach (var entity in this.entities)
            {
                entity.Serizile(this, jsonWriter);
            }
            jsonWriter.WriteEndArray();

            jsonWriter.WritePropertyName("components");
            jsonWriter.WriteStartArray();
            foreach (var entity in this.entities)
            {
                foreach (var comp in entity.components)
                {
                    comp.Serizile(this, jsonWriter);
                }
            }
            jsonWriter.WriteEndArray();

            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(VERSION);

            jsonWriter.WriteEndObject();
        }
    }

    public static class ComponentDataWriter
    {

        public static void SetInt(this ComponentData comp, string key, int value)
        {
            comp.props[key] = new MyJson_Number(value);
        }
        public static void SetNumber(this ComponentData comp, string key, float value, int? digits = null)
        {
            comp.props[key] = new MyJson_Number(System.Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits));
        }
        public static void SetNumber(this ComponentData comp, string key, double value, int? digits = null)
        {
            comp.props[key] = new MyJson_Number(System.Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits));
        }
        public static void SetString(this ComponentData comp, string key, string value)
        {
            comp.props[key] = new MyJson_String(value);
        }
        public static void SetReference(this ComponentData comp, string key, System.Object value)
        {
            comp.props[key] = new MyJson_Reference(value);
        }
        public static void SetBool(this ComponentData comp, string key, bool value)
        {
            comp.props[key] = new MyJson_Number(value);
        }
        public static void SetEnum(this ComponentData comp, string key, System.Enum value, bool toString = false)
        {
            if (toString)
            {
                comp.props[key] = new MyJson_String(value);
            }
            else
            {
                comp.props[key] = new MyJson_Number(value);
            }
        }

        //-------------------------------扩展部分--------------------------------
        /**
         * 组件的公共部分，Vector2
         */
        public static void SetVector2(this ComponentData comp, string desc, UnityEngine.Vector2 data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            comp.props[desc] = cItemArr;
        }
        /**
         * 组件的公共部分，Vector3
         */
        public static void SetVector3(this ComponentData comp, string desc, UnityEngine.Vector3 data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.z, digits);
            comp.props[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Vector4
         */
        public static void SetVector4(this ComponentData comp, string desc, UnityEngine.Vector4 data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.z, digits);
            cItemArr.AddNumber(data.w, digits);

            comp.props[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Quaternion
         */
        public static void SetQuaternion(this ComponentData comp, string desc, UnityEngine.Quaternion data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.z, digits);
            cItemArr.AddNumber(data.w, digits);

            comp.props[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Color
         */
        public static void SetColor(this ComponentData comp, string desc, UnityEngine.Color data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.r, digits);
            cItemArr.AddNumber(data.g, digits);
            cItemArr.AddNumber(data.b, digits);
            cItemArr.AddNumber(data.a, digits);

            comp.props[desc] = cItemArr;
        }
        public static void SetColor3(this ComponentData comp, string desc, UnityEngine.Color data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.r, digits);
            cItemArr.AddNumber(data.g, digits);
            cItemArr.AddNumber(data.b, digits);

            comp.props[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Rect
         */
        public static void SetRect(this ComponentData comp, string desc, UnityEngine.Rect data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.width, digits);
            cItemArr.AddNumber(data.height, digits);

            comp.props[desc] = cItemArr;
        }
        public static void SetUVTransform(this ComponentData comp, string desc, UnityEngine.Vector4 data, int? digits = null)
        {
            var tx = data.z;
            var ty = data.w;
            var sx = data.x;
            var sy = data.y;
            var cx = 0.0f;
            var cy = 0.0f;
            var rotation = 0.0f;
            var c = System.Math.Cos(rotation);
            var s = System.Math.Sin(rotation);

            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(sx * c);
            cItemArr.AddNumber(sx * s);
            cItemArr.AddNumber(-sx * (c * cx + s * cy) + cx + tx);
            cItemArr.AddNumber(-sy * s);
            cItemArr.AddNumber(sy * c);
            cItemArr.AddNumber(-sy * (-s * cx + c * cy) + cy + ty);
            cItemArr.AddNumber(0.0);
            cItemArr.AddNumber(0.0);
            cItemArr.AddNumber(1.0);

            comp.props[desc] = cItemArr;
        }

        public static void SetTexture(this ComponentData comp, UnityEngine.GameObject obj, UnityEngine.Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }
        }

        public static void SetMesh(this ComponentData comp, UnityEngine.GameObject obj, UnityEngine.Mesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            var path = PathHelper.GetPath(mesh);
            var asset = SerializeObject.SerializeAsset(mesh, path, AssetType.Mesh);

            comp.props["mesh"] = new MyJson_Asset(path);

            SerializeObject.currentData.AddAsset(asset);
        }

        public static void SetMaterials(this ComponentData comp, UnityEngine.GameObject obj, UnityEngine.Material[] materials, bool isParticleMat = false, bool isAnimationMat = false)
        {
            var materialsItem = new MyJson_Array();
            comp.props["materials"] = materialsItem;
            //写材质
            foreach (var material in materials)
            {
                if (material == null)
                {
                    UnityEngine.Debug.LogWarning(obj.gameObject.name + " 材质缺失，请检查资源");
                    continue;
                }

                var path = PathHelper.GetPath(material);
                var asset = SerializeObject.SerializeAsset(material, path, AssetType.Material);

                materialsItem.Add(new MyJson_Asset(path));

                SerializeObject.currentData.AddAsset(asset);
            }
        }
        public static void SetAnimation(this ComponentData comp, UnityEngine.GameObject obj, UnityEngine.AnimationClip[] animationClips)
        {
            var exportAnimations = new MyJson_Array();
            comp.props["_animations"] = exportAnimations;

            foreach (var animationClip in animationClips)
            {
                var path = PathHelper.GetPath(animationClip);
                var asset = SerializeObject.SerializeAsset(animationClip, path, AssetType.Animation);

                exportAnimations.Add(new MyJson_Asset(path));

                SerializeObject.currentData.AddAsset(asset);
            }
        }

        public static void SetLightmaps(this ComponentData comp, string exportPath)
        {
            var lightmapsJson = new MyJson_Array();
            foreach (var lightmapData in LightmapSettings.lightmaps)
            {
                Texture2D lightmap = lightmapData.lightmapColor;

                var path = PathHelper.GetPath(lightmap);
                var asset = SerializeObject.SerializeAsset(lightmap, path, AssetType.Texture);

                lightmapsJson.Add(new MyJson_Asset(path));
                SerializeObject.currentData.AddAsset(asset);
            }

            comp.props["lightmaps"] = lightmapsJson;
        }
    }
}
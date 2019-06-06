namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
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

        public SerializedData data;

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
        public JObject properties = new JObject();
        private void SerializeProperty(string name, JToken property, JsonWriter writer)
        {
            // Debug.Log("name:" + name + " value:" + property + " type:" + property.Type);
            if (name != string.Empty)
            {
                writer.WritePropertyName(name);
            }

            if (property.Type == JTokenType.Array)
            {
                writer.WriteStartArray();
                foreach (var p in property)
                {
                    this.SerializeProperty("", p, writer);
                }

                writer.WriteEndArray();
            }
            else if (property.Type == JTokenType.Object)
            {
                writer.WriteStartObject();
                foreach (JProperty p in property)
                {
                    this.SerializeProperty(p.Name, p, writer);
                }
                writer.WriteEndObject();
            }
            else if (property.Type == JTokenType.Property)
            {
                writer.WriteValue((property as JProperty).Value);
            }
            else
            {
                writer.WriteValue(property);
            }
        }

        public void AddChild(ComponentData child)
        {
            JToken children;
            if (!this.properties.TryGetValue(KEY_CHILDREN, out children))
            {
                children = new JArray();
                this.properties.Add(KEY_CHILDREN, children);
            }

            (children as JArray).Add(new JObject(new JProperty(KEY_UUID, child.uuid)));
        }


        public void Serizile(SerializedData data, JsonWriter writer)
        {
            // writer.WriteStartObject();

            // writer.WritePropertyName(KEY_UUID);
            // writer.WriteValue(data.GetUUID(this.GetHashCode()));

            // writer.WritePropertyName(KEY_CLASS);
            // writer.WriteValue(this.__class);

            // foreach (var name in this.props.Keys)
            // {
            //     var propertyValue = this.props[name];
            //     // Debug.Log("name:" + name + " value:" + propertyValue);
            //     this.SerializeProperty(name, propertyValue, data, writer);
            // }

            // writer.WriteEndObject();

            writer.WriteStartObject();

            writer.WritePropertyName(KEY_UUID);
            writer.WriteValue(data.GetUUID(this.GetHashCode()));

            writer.WritePropertyName(KEY_CLASS);
            writer.WriteValue(this.__class);

            foreach (var property in this.properties)
            {
                this.SerializeProperty(property.Key, property.Value, writer);

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
            entity.data = this;
            entity.uuid = this.GetUUID(entity.GetHashCode());
            entity.__class = SerializeClass.GameEntity;
            this.entities.Add(entity);
            return entity;
        }

        public ComponentData CreateComponent(string className)
        {
            ComponentData comp = new ComponentData();
            comp.uuid = this.GetUUID(comp.GetHashCode());
            comp.__class = className;
            return comp;
        }

        public int AddAsset(AssetData asset)
        {
            for (int i = 0, l = this.assets.Count; i < l; i++)
            {
                var item = this.assets[i];
                if (item.uri == asset.uri)
                {
                    this.assets[i] = asset;
                    return i;
                }
            }

            this.assets.Add(asset);

            return this.assets.Count - 1;
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
        public static void SetMesh(this ComponentData comp, UnityEngine.GameObject obj, UnityEngine.Mesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            var path = PathHelper.GetPath(mesh);
            var asset = SerializeObject.SerializeAsset(mesh, path, AssetType.Mesh);

            // comp.props["mesh"] = new MyJson_Asset(path);

            var assetIndex = SerializeObject.currentData.AddAsset(asset);
            comp.properties.SetAsset("mesh", assetIndex);
        }

        public static void SetMaterials(this ComponentData comp, UnityEngine.GameObject obj, UnityEngine.Material[] materials, bool isParticleMat = false, bool isAnimationMat = false)
        {
            var materialsItem = new JArray();
            comp.properties.Add("materials", materialsItem);
            // comp.props["materials"] = materialsItem;
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

                // materialsItem.Add(new MyJson_Asset(path));
                // SerializeObject.currentData.AddAsset(asset);

                var assetIndex = SerializeObject.currentData.AddAsset(asset);
                materialsItem.AddAsset(assetIndex);
            }
        }
        public static void SetAnimation(this ComponentData comp, UnityEngine.GameObject obj, UnityEngine.AnimationClip[] animationClips)
        {
            var exportAnimations = new JArray();
            comp.properties.Add("_animations", exportAnimations);
            // comp.props["_animations"] = exportAnimations;

            foreach (var animationClip in animationClips)
            {
                var path = PathHelper.GetPath(animationClip);
                var asset = SerializeObject.SerializeAsset(animationClip, path, AssetType.Animation);

                // exportAnimations.Add(new MyJson_Asset(path));
                // SerializeObject.currentData.AddAsset(asset);

                var assetIndex = SerializeObject.currentData.AddAsset(asset);
                exportAnimations.AddAsset(assetIndex);
            }
        }

        public static void SetLightmaps(this ComponentData comp, string exportPath)
        {
            var lightmapsJson = new JArray();
            comp.properties.Add("lightmaps", lightmapsJson);
            // comp.props["lightmaps"] = lightmapsJson;
            foreach (var lightmapData in LightmapSettings.lightmaps)
            {
                Texture2D lightmap = lightmapData.lightmapColor;

                var path = PathHelper.GetPath(lightmap);
                var asset = SerializeObject.SerializeAsset(lightmap, path, AssetType.Texture);

                // lightmapsJson.Add(new MyJson_Asset(path));
                // SerializeObject.currentData.AddAsset(asset);

                var assetIndex = SerializeObject.currentData.AddAsset(asset);
                lightmapsJson.AddAsset(assetIndex);
            }

        }
    }
}
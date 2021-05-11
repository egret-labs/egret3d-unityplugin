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

        void Serialize(SerializeContext data, JsonWriter writer);
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
        public JObject properties = new JObject();

        public abstract void Serialize(SerializeContext data, JsonWriter writer);
    }
    public class EntityData : SerizileData
    {
        public ComponentData treeNode;
        public List<ComponentData> components = new List<ComponentData>();

        public void AddComponent(ComponentData comp)
        {
            if (!this.components.Contains(comp))
            {
                this.components.Add(comp);
            }

            if (comp.__class == SerializeClass.TreeNode)
            {
                if (this.treeNode != null)
                {
                    MyLog.LogWarning("一个实体多个transform组件");
                }
                this.treeNode = comp;
            }

            comp.entity = this;
        }

        public override void Serialize(SerializeContext data, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(SerizileData.KEY_UUID);
            writer.WriteValue(this.uuid);

            writer.WritePropertyName(SerizileData.KEY_CLASS);
            writer.WriteValue(this.__class);

            writer.WritePropertyName(SerizileData.KEY_COMPONENTS);
            writer.WriteStartArray();
            foreach (var comp in this.components)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(SerizileData.KEY_UUID);
                writer.WriteValue(comp.uuid);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }

    public class ComponentData : SerizileData
    {
        public EntityData entity;
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
                    this.SerializeProperty(string.Empty, p, writer);
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
                var value = (property as JProperty).Value;
                if (value is JContainer)
                {
                    this.SerializeProperty(string.Empty, value, writer);
                }
                else
                {
                    writer.WriteValue(value);
                }
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


        public override void Serialize(SerializeContext data, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(KEY_UUID);
            writer.WriteValue(this.uuid);

            writer.WritePropertyName(KEY_CLASS);
            writer.WriteValue(this.__class);

            foreach (var property in this.properties)
            {
                this.SerializeProperty(property.Key, property.Value, writer);
            }

            writer.WriteEndObject();
        }

    }

    public class AssetData : SerizileData
    {
        public static AssetData Create(string uri)
        {
            var asset = new AssetData();
            asset.uri = uri;
            return asset;
        }
        public string uri = string.Empty;
        public byte[] buffer;

        public override void Serialize(SerializeContext data, JsonWriter writer)
        {
            writer.WriteValue(ExportSetting.instance.GetExportPath(this.uri));
        }
    }

    public class SerializeContext
    {
        public static void Export(string baseDir, string exportPath)
        {
            {
                var relativePath = ExportSetting.instance.GetExportPath(exportPath);
                var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(baseDir, relativePath));
                var fileDir = PathHelper.GetFileDirectory(filePath);
                if (!System.IO.Directory.Exists(fileDir))
                {
                    System.IO.Directory.CreateDirectory(fileDir);
                }
                var gltfFile = File.CreateText(filePath);
                SerializeObject.currentData.Serialize(gltfFile);
                gltfFile.Close();
                MyLog.Log("---导出文件:" + relativePath);
            }

            {
                foreach (var asset in SerializeObject.assetsData.Values)
                {
                    if (asset == null || asset.buffer == null)
                    {
                        Debug.Log("资源为空，跳过:" + asset.uri);
                        continue;
                    }
                    var relativePath = ExportSetting.instance.GetExportPath(asset.uri);
                    var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(baseDir, relativePath));
                    var fileDir = PathHelper.GetFileDirectory(filePath);
                    if (!System.IO.Directory.Exists(fileDir))
                    {
                        System.IO.Directory.CreateDirectory(fileDir);
                    }
                    System.IO.File.WriteAllBytes(filePath, asset.buffer);
                }
            }
        }
        public const string VERSION = "5";//资源版本号
        public List<AssetData> assets = new List<AssetData>();
        public List<EntityData> entities = new List<EntityData>();

        private int _uuidIndex = 0;
        private readonly Dictionary<int, int> _uuidDic = new Dictionary<int, int>();

        private string GetUUID(System.Object obj)
        {
            int unityHash = obj.GetHashCode();
            int newHash;
            if (this._uuidDic.TryGetValue(unityHash, out newHash))
            {
                return newHash.ToString();
            }
            newHash = this._uuidIndex++;
            this._uuidDic[unityHash] = newHash;
            return newHash.ToString();
        }

        public void Clear()
        {
            this.assets.Clear();
            this.entities.Clear();

            this._uuidIndex = 0;
            this._uuidDic.Clear();
        }

        public EntityData CreateEntity()
        {
            var entity = new EntityData();
            entity.uuid = this.GetUUID(entity);
            entity.__class = SerializeClass.GameEntity;
            this.entities.Add(entity);
            return entity;
        }

        public ComponentData CreateComponent(string className)
        {
            var comp = new ComponentData();
            comp.uuid = this.GetUUID(comp);
            comp.__class = className;
            return comp;
        }

        public int AddAsset(AssetData asset)
        {
            if (asset == null || asset.buffer == null)
            {
                return this.assets.Count - 1;
            }

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

        public virtual void Serialize(TextWriter textWriter)
        {
            JsonWriter jsonWriter = new JsonTextWriter(textWriter);
            jsonWriter.Formatting = ExportSetting.instance.common.jsonFormatting ? Formatting.Indented : Formatting.None;
            jsonWriter.WriteStartObject();
            // assets
            {
                jsonWriter.WritePropertyName("assets");
                jsonWriter.WriteStartArray();
                foreach (var asset in this.assets)
                {
                    asset.Serialize(this, jsonWriter);
                }
                jsonWriter.WriteEndArray();
            }
            // entities
            {
                jsonWriter.WritePropertyName("entities");
                jsonWriter.WriteStartArray();
                foreach (var entity in this.entities)
                {
                    entity.Serialize(this, jsonWriter);
                }
                jsonWriter.WriteEndArray();
            }
            // components
            {
                jsonWriter.WritePropertyName("components");
                jsonWriter.WriteStartArray();
                foreach (var entity in this.entities)
                {
                    foreach (var comp in entity.components)
                    {
                        comp.Serialize(this, jsonWriter);
                    }
                }
                jsonWriter.WriteEndArray();
            }
            // version
            {
                jsonWriter.WritePropertyName("version");
                jsonWriter.WriteValue(VERSION);
            }

            jsonWriter.WriteEndObject();
        }
    }
}
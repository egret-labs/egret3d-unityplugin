using System;
using System.Collections.Generic;
using UnityEngine;
using GLTF.Schema;
using System.IO;

namespace Egret3DExportTools
{
    /**
     * 组件管理器接口
     */
    public interface ISerializer
    {
        Type compType { set; get; }
        string className { set; get; }
        bool Match(UnityEngine.Object component);
        void Serialize(UnityEngine.Object component, SerizileData compData);
    }

    public abstract class Serializer : ISerializer
    {
        protected Type _compType;
        protected string _className;

        public virtual bool Match(UnityEngine.Object component)
        {
            return true;
        }

        public virtual void Serialize(UnityEngine.Object component, SerizileData compData)
        {            
        }

        public Type compType { get => _compType; set => _compType = value; }
        public string className { get => _className; set => _className = value; }
    }

    public abstract class ComponentSerializer : Serializer
    {
        protected virtual void Serialize(Component component, ComponentData compData)
        {
        }

        protected virtual bool Match(Component component)
        {
            return true;
        }

        public override bool Match(UnityEngine.Object component)
        {
            return this.Match(component as Component);
        }

        public override void Serialize(UnityEngine.Object component, SerizileData compData)
        {
            this.Serialize(component as Component, compData as ComponentData);
        }
    }

    public abstract class AssetSerializer : Serializer
    {
        protected GLTFRoot _root;
        protected BufferId _bufferId;
        protected GLTF.Schema.Buffer _buffer;
        protected BinaryWriter _bufferWriter;
        public Transform _target;
        protected virtual void Serialize(UnityEngine.Object sourceAsset)
        {

        }
        protected virtual void InitGLTFRoot()
        {
            this._root = new GLTFRoot
            {
                Accessors = new List<Accessor>(),
                Asset = new Asset
                {
                    Version = "2.0",
                    Generator = "Unity plugin for egret",
                    Extensions = new Dictionary<string, IExtension>(),
                },
                ExtensionsRequired = new List<string>(),
                ExtensionsUsed = new List<string>(),
                Extensions = new Dictionary<string, IExtension>() { { "egret", new AssetVersionExtension() { version = "5.0", minVersion = "5.0" } } },
            };
        }

        public override void Serialize(UnityEngine.Object sourceAsset, SerizileData asset)
        {
            this._target = SerializeObject.currentTarget;
            this.InitGLTFRoot();
            this.Serialize(sourceAsset);

            if (this._bufferWriter != null)
            {
                //二进制数据
                var ms = this._bufferWriter.BaseStream as MemoryStream;
                var binBuffer = ms.ToArray();
                this._bufferWriter.Close();

                this._buffer.Uri = "";
                this._buffer.ByteLength = binBuffer.Length;

                ms = new MemoryStream();
                var streamWriter = new StreamWriter(ms);
                this._root.Serialize(streamWriter);
                streamWriter.Close();
                var jsonBuffer = ms.ToArray();

                var writer = new BinaryWriter(new MemoryStream());
                (asset as AssetData).buffer = GLTFExtension.WriteBinary(jsonBuffer, binBuffer, writer);
                writer.Close();
            }
            else
            {
                //JSON数据                
                var writer = new StringWriter();
                this._root.Serialize(writer);
                var jsonBuffer = System.Text.Encoding.UTF8.GetBytes(writer.ToString());
                (asset as AssetData).buffer = jsonBuffer;
                writer.Close();
            }

            this._bufferWriter = null;
        }
    }
}
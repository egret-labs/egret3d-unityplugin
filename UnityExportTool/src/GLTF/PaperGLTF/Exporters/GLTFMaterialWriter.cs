namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;

    using Egret3DExportTools;

    public enum MaterialType
    {
        Diffuse,
        Lambert,
        Phong,
        Standard,
        StandardSpecular,
        StandardRoughness,
        Particle,
        Custom
    }

    public struct Define
    {
        public string name;
        public string content;
    }

    public class MaterialData
    {
        public readonly Dictionary<string, IJsonNode> values = new Dictionary<string, IJsonNode>();
        public readonly List<EnableState> enables = new List<EnableState>();
        public readonly Dictionary<string, IJsonNode> functions = new Dictionary<string, IJsonNode>();
        public readonly List<Define> defines = new List<Define>();
        public string shaderAsset = string.Empty;

        public void CleanUp()
        {
            this.values.Clear();
            this.enables.Clear();
            this.functions.Clear();
            this.defines.Clear();
            this.shaderAsset = string.Empty;
        }
    }

    public class MaterialWriter : GLTFExporter
    {
        protected readonly MaterialData data = new MaterialData();
        private readonly bool _isParticle = false;
        private readonly bool _isAnimation = false;
        private readonly UnityEngine.Material _target;

        private readonly Dictionary<MaterialType, BaseMaterialParser> parsers = new Dictionary<MaterialType, BaseMaterialParser>();

        public MaterialWriter(UnityEngine.Material target, bool isParticle, bool isAnimation = false) : base()
        {
            this._target = target;
            this._isParticle = isParticle;
            this._isAnimation = isAnimation;

            //
            this.register(MaterialType.Diffuse, new DiffuseParser(), "builtin/meshbasic.shader.json");
            this.register(MaterialType.Lambert, new LambertParser(), "builtin/meshlambert.shader.json");
            this.register(MaterialType.Phong, new PhongParser(), "builtin/meshphong.shader.json");
            this.register(MaterialType.Standard, new StandardParser(), "builtin/meshphysical.shader.json");
            this.register(MaterialType.StandardRoughness, new StandardRoughnessParser(), "builtin/meshphysical.shader.json");
            this.register(MaterialType.StandardSpecular, new StandardSpecularParser(), "builtin/meshphysical.shader.json");
            this.register(MaterialType.Particle, new ParticleParser(), "builtin/particle.shader.json");
            this.register(MaterialType.Custom, new CustomParser(), "");
        }

        private void register(MaterialType type, BaseMaterialParser parse, string shaderAsset)
        {
            parse.Init(shaderAsset);
            this.parsers.Add(type, parse);
        }

        private BaseMaterialParser getParser(MaterialType type)
        {
            return this.parsers[type];
        }

        protected override void Init()
        {
            base.Init();

            this._root = new GLTFRoot
            {
                Accessors = new List<Accessor>(),
                Asset = new Asset
                {
                    Version = "2.0",
                    Generator = "egret",
                    Extensions = new Dictionary<string, IExtension>(),
                },
                Materials = new List<GLTF.Schema.Material>(),
            };
        }

        protected override void WriteJson(MyJson_Tree gltfJson)
        {
            base.WriteJson(gltfJson);
            var source = this._target;
            var data = this.data;
            //
            data.CleanUp();

            var parser = this.getParser(this.GetMaterialType());
            parser.Parse(source, data);

            {
                var materials = new MyJson_Array();
                var material = new MyJson_Tree();
                materials.Add(material);
                gltfJson.Add("materials", materials);

                material.SetString("name", source.name);

                var extensions = new MyJson_Tree();
                material.Add("extensions", extensions);
                var KHR_techniques_webgl = new MyJson_Tree();
                extensions.Add("KHR_techniques_webgl", KHR_techniques_webgl);
                KHR_techniques_webgl.SetInt("technique", 0);

                var values = new MyJson_Tree();
                KHR_techniques_webgl.Add("values", values);

                foreach (var pair in this.data.values)
                {
                    values.Add(pair.Key, pair.Value);
                }
            }

            {
                var extensions = gltfJson["extensions"] as MyJson_Tree;
                var KHR_techniques_webgl = new MyJson_Tree();
                extensions.Add("KHR_techniques_webgl", KHR_techniques_webgl);

                var techniques = new MyJson_Array();
                KHR_techniques_webgl.Add("techniques", techniques);

                var technique = new MyJson_Tree();
                techniques.Add(technique);

                if (this.data.enables.Count + this.data.functions.Count > 0)
                {
                    var states = new MyJson_Tree();
                    technique.Add("states", states);
                    var enable = new MyJson_Array();
                    states.Add("enable", enable);

                    foreach (var v in this.data.enables)
                    {
                        enable.AddInt((int)v);
                    }

                    var functions = new MyJson_Tree();
                    states.Add("functions", functions);
                    foreach (var pair in this.data.functions)
                    {
                        functions.Add(pair.Key, pair.Value);
                    }
                }

                //TODO gltf 必须带
                var uniforms = new MyJson_Tree();
                technique.Add("uniforms", uniforms);
            }

            {
                var extensions = gltfJson["extensions"] as MyJson_Tree;
                var egret = extensions["egret"] as MyJson_Tree;

                var assets = new MyJson_Array();
                assets.AddUri(this.data.shaderAsset);
                egret.Add("assets", assets);

                var entities = new MyJson_Array();
                egret.Add("entities", entities);
                var entity = new MyJson_Tree();
                entities.Add(entity);
                entity.SetSerializeClass(entity.GetHashCode(), SerializeClass.AssetEntity);
                var entityComps = new MyJson_Array();
                entity.Add("components", entityComps);

                var components = new MyJson_Array();
                egret.Add("components", components);

                var material = new MyJson_Tree();
                material.SetSerializeClass(material.GetHashCode(), SerializeClass.Material);
                material.SetInt("glTF", 0);
                var asset = new MyJson_Tree();
                asset.SetAsset(0);
                material.Add("shader", asset);
                components.Add(material);
                entityComps.AddUUID(material);

                if (this.data.defines.Count > 0)
                {
                    var defines = new MyJson_Tree();
                    defines.SetSerializeClass(defines.GetHashCode(), SerializeClass.Defines);

                    var defineStrs = new MyJson_Array();
                    defines.Add("defines", defineStrs);
                    foreach (var define in this.data.defines)
                    {
                        defineStrs.AddString(define.name + define.content);
                    }

                    entityComps.AddUUID(defines);
                }
            }
        }

        private MaterialType GetMaterialType()
        {
            var shaderName = this._target.shader.name;
            var customShaderConfig = ExportConfig.instance.getCustomShader(shaderName);

            if (this._isParticle)
            {
                return MaterialType.Particle;
            }

            if (customShaderConfig != null)
            {
                return MaterialType.Custom;
            }

            {
                var lightType = ExportToolsSetting.instance.lightType;
                switch (lightType)
                {
                    case ExportLightType.Lambert:
                        {
                            return MaterialType.Lambert;
                        }
                    case ExportLightType.Phong:
                        {
                            return MaterialType.Phong;
                        }
                    case ExportLightType.Standard:
                        {
                            switch (shaderName)
                            {
                                case "Standard (Specular setup)":
                                    {
                                        return MaterialType.StandardSpecular;
                                    }
                                case "Standard (Roughness setup)":
                                    {
                                        return MaterialType.StandardRoughness;
                                    }
                                default:
                                    {
                                        return MaterialType.Standard;
                                    }
                            }
                        }
                }
            }

            return MaterialType.Diffuse;
        }
    }
}

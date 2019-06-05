namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;

    using Egret3DExportTools;
    using Newtonsoft.Json.Linq;

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
        public List<JProperty> values = new List<JProperty>();
        public Functions functions = new Functions();


        // public readonly Dictionary<string, IJsonNode> values = new Dictionary<string, IJsonNode>();
        public readonly List<EnableState> enables = new List<EnableState>();
        // public readonly Dictionary<string, IJsonNode> functions = new Dictionary<string, IJsonNode>();
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

    public class GLTFMaterialSerializer : GLTFSerializer
    {
        protected readonly MaterialData data = new MaterialData();
        private bool _isParticle = false;
        private UnityEngine.Material _material;

        private readonly Dictionary<MaterialType, BaseMaterialParser> parsers = new Dictionary<MaterialType, BaseMaterialParser>();

        public GLTFMaterialSerializer()
        {
            this.parsers.Clear();

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

        protected override void InitGLTFRoot()
        {
            base.InitGLTFRoot();

            this._root = new GLTFRoot
            {
                Accessors = new List<Accessor>(),
                Asset = new Asset
                {
                    Version = "2.0",
                    Generator = "Unity plugin for egret",
                    Extensions = new Dictionary<string, IExtension>(),
                },
                ExtensionsRequired = new List<string>() { "KHR_techniques_webgl", "egret" },
                ExtensionsUsed = new List<string>() { "KHR_techniques_webgl", "egret" },
                Extensions = new Dictionary<string, IExtension>() { },
                Materials = new List<GLTF.Schema.Material>(),
            };
        }

        protected override void _Serialize(UnityEngine.Object sourceAsset)
        {
            this._material = sourceAsset as UnityEngine.Material;

            this._isParticle = this._target.GetComponent<ParticleSystem>() != null;
            var source = this._material;
            var data = this.data;
            //
            data.CleanUp();

            var parser = this.getParser(this.GetMaterialType());
            parser.Parse(source, data);

            var materialGLTF = new GLTF.Schema.Material();
            materialGLTF.Name = source.name;

            materialGLTF.Extensions = new Dictionary<string, IExtension>(){
                {
                    "KHR_techniques_webgl",
                    new KhrTechniquesWebglMaterialExtension() { technique = 0, values = data.values }
                }
            };

            var techniqueExt = new KhrTechniqueWebglGlTfExtension();
            var technique = new Techniques();
            technique.states = new States();
            technique.states.enable = data.enables.ToArray();
            technique.states.functions = data.functions;
            techniqueExt.techniques.Add(technique);
            this._root.Extensions.Add("egret", new MaterialAssetExtension()
            {
                version = "5.0",
                minVersion = "5.0",
                asset = this.data.shaderAsset,
                defines = this.data.defines,
            });
            this._root.Extensions.Add("KHR_techniques_webgl", techniqueExt);

            this._root.Materials.Add(materialGLTF);

            // var writer = new StringWriter();
            // this._root.Serialize(writer);

            // asset.buffer = System.Text.Encoding.UTF8.GetBytes(writer.ToString());
        }

        private MaterialType GetMaterialType()
        {
            var shaderName = this._material.shader.name;
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

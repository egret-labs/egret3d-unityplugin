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
        public Techniques technique;
        public JObject values;
        public MaterialAssetExtension asset;
    }

    public class GLTFMaterialSerializer : GLTFSerializer
    {
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

            this._root.ExtensionsRequired.Add(KhrTechniquesWebglMaterialExtension.EXTENSION_NAME);
            this._root.ExtensionsRequired.Add(AssetVersionExtension.EXTENSION_NAME);

            this._root.ExtensionsUsed.Add(KhrTechniquesWebglMaterialExtension.EXTENSION_NAME);
            this._root.ExtensionsUsed.Add(AssetVersionExtension.EXTENSION_NAME);

            this._root.Materials = new List<GLTF.Schema.Material>();
        }

        protected override void _Serialize(UnityEngine.Object sourceAsset)
        {
            this._material = sourceAsset as UnityEngine.Material;

            this._isParticle = this._target.GetComponent<ParticleSystem>() != null;
            var source = this._material;
            var data = new MaterialData();
            var technique = new Techniques();
            var materialExtension = new KhrTechniquesWebglMaterialExtension();
            var assetExtension = new MaterialAssetExtension() { version = "5.0", minVersion = "5.0" };
            //
            data.values = materialExtension.values;
            data.technique = technique;
            data.asset = assetExtension;

            var parser = this.getParser(this.GetMaterialType());
            parser.Parse(source, data);

            var materialGLTF = new GLTF.Schema.Material();
            materialGLTF.Name = source.name;

            materialGLTF.Extensions = new Dictionary<string, IExtension>() { { KhrTechniquesWebglMaterialExtension.EXTENSION_NAME, materialExtension } };

            var techniqueExt = new KhrTechniqueWebglGlTfExtension();

            techniqueExt.techniques.Add(technique);
            if(this._root.Extensions.ContainsKey(AssetVersionExtension.EXTENSION_NAME))
            {
                this._root.Extensions.Remove(AssetVersionExtension.EXTENSION_NAME);
            }
            this._root.Extensions.Add(AssetVersionExtension.EXTENSION_NAME, assetExtension);
            this._root.Extensions.Add(KhrTechniquesWebglMaterialExtension.EXTENSION_NAME, techniqueExt);
            this._root.Materials.Add(materialGLTF);
        }

        private MaterialType GetMaterialType()
        {
            var shaderName = this._material.shader.name;
            var customShaderConfig = ExportConfig.instance.GetCustomShader(shaderName);

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

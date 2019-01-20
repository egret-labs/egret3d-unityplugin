namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;

    using Egret3DExportTools;

    public class MaterialWriter : GLTFExporter
    {
        private bool _isParticle = false;
        private bool _isAnimation = false;
        private UnityEngine.Material _target;
        public MaterialWriter(UnityEngine.Material target, bool isParticle, bool isAnimation = false) : base()
        {
            this._target = target;
            this._isParticle = isParticle;
            this._isAnimation = isAnimation;
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
                    Generator = "paper",
                    Extensions = new Dictionary<string, IExtension>(),
                },
                Materials = new List<GLTF.Schema.Material>(),
            };
        }

        public override byte[] WriteGLTF()
        {
            var target = this._target;
            var gltfJson = new MyJson_Tree();
            //
            var assetJson = new MyJson_Tree();
            assetJson.SetInt("version", 2);
            gltfJson.Add("asset", assetJson);

            var materialsJson = new MyJson_Array();
            gltfJson.Add("materials", materialsJson);

            var extensionsRequired = new MyJson_Array();
            extensionsRequired.AddString("KHR_techniques_webgl");
            extensionsRequired.AddString("paper");
            gltfJson.Add("extensionsRequired", extensionsRequired);

            var extensionsUsed = new MyJson_Array();
            extensionsUsed.AddString("KHR_techniques_webgl");
            extensionsUsed.AddString("paper");
            gltfJson.Add("extensionsUsed", extensionsUsed);
            //
            gltfJson.SetInt("version", 4);

            //Unifrom or Defines
            var materialType = this.GetMaterialType();
            var writer = MaterialWriterFactory.Create(materialType, target);

            var materialItemJson = writer.Write();
            materialsJson.Add(materialItemJson);
            writer.Clean();

            //
            gltfJson.isWithFormat = true;
            var jsonStr = gltfJson.ToString();

            return System.Text.Encoding.UTF8.GetBytes(jsonStr);
        }

        private MaterialType GetMaterialType()
        {
            var shaderName = this._target.shader.name;
            var customShaderConfig = ExportConfig.instance.IsCustomShader(shaderName);
            
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

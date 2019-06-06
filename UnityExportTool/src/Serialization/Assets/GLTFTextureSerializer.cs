namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;

    using Egret3DExportTools;
    using Newtonsoft.Json;

    public class GLTFTextureSerializer : GLTFSerializer
    {
        private UnityEngine.Texture2D texture;

        protected override void InitGLTFRoot()
        {
            base.InitGLTFRoot();

            this._root.ExtensionsRequired.Add(AssetVersionExtension.EXTENSION_NAME);
            this._root.ExtensionsUsed.Add(AssetVersionExtension.EXTENSION_NAME);
            
            this._root.Images = new List<Image>();
            this._root.Samplers = new List<Sampler>();
            this._root.Textures = new List<GLTF.Schema.Texture>();
        }

        private void ExportTexture()
        {
            var tex = this.texture;
            var path = PathHelper.GetTexturePath(tex);
            UnityEditor.TextureImporter importer = (UnityEditor.TextureImporter)UnityEditor.TextureImporter.GetAtPath(path);
            bool isNormal = importer && importer.textureType == UnityEditor.TextureImporterType.NormalMap;
            string ext = PathHelper.GetTextureExt(tex);
            byte[] bs;

            var isSupported = PathHelper.IsSupportedExt(tex);
            //只有jpg、png可以原始图片导出，其他类型不支持
            var filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), path);
            if (ExportToolsSetting.instance.exportOriginalImage && isSupported && System.IO.File.Exists(filename))
            {
                MyLog.Log("原始图片:" + filename);
                bs = System.IO.File.ReadAllBytes(filename);
            }
            else
            {
                bs = ExportImageTools.instance.EncodeToPNG(tex, ext);
            }

            if (!SerializeObject.assetsData.ContainsKey(path))
            {
                var assetData = AssetData.Create(path);
                assetData.buffer = bs;
                SerializeObject.assetsData.Add(path, assetData);
            }
        }

        protected override void _Serialize(UnityEngine.Object sourceAsset)
        {
            this.texture = sourceAsset as UnityEngine.Texture2D;
            //先把原始图片导出来
            this.ExportTexture();

            var path = PathHelper.GetTexturePath(this.texture);
            var mipmap = this.texture.mipmapCount > 1;
            //
            {
                this._root.Images.Add(new Image() { Uri = ExportConfig.instance.GetExportPath(path) });
            }
            //
            {
                var filterMode = this.texture.filterMode;
                var wrapMode = this.texture.wrapMode;

                var sampler = new Sampler();
                this._root.Samplers.Add(sampler);
                if (wrapMode == TextureWrapMode.Repeat)
                {
                    sampler.WrapS = GLTF.Schema.WrapMode.Repeat;
                    sampler.WrapT = GLTF.Schema.WrapMode.Repeat;
                }
                else
                {
                    sampler.WrapS = GLTF.Schema.WrapMode.ClampToEdge;
                    sampler.WrapT = GLTF.Schema.WrapMode.ClampToEdge;
                }
                sampler.MagFilter = filterMode == FilterMode.Point ? MagFilterMode.Nearest : MagFilterMode.Linear;
                if (!mipmap)
                {
                    sampler.MagFilter = filterMode == FilterMode.Point ? MagFilterMode.Nearest : MagFilterMode.Linear;
                }
                else if (filterMode == FilterMode.Point)
                {
                    sampler.MinFilter = MinFilterMode.NearestMipmapNearest;
                }
                else if (filterMode == FilterMode.Bilinear)
                {
                    sampler.MinFilter = MinFilterMode.LinearMipmapNearest;
                }
                else if (filterMode == FilterMode.Trilinear)
                {
                    sampler.MinFilter = MinFilterMode.LinearMipmapLinear;
                }
            }
            //
            {
                var gltfTexture = new GLTF.Schema.Texture();
                this._root.Textures.Add(gltfTexture);

                gltfTexture.Sampler = new SamplerId();
                gltfTexture.Source = new ImageId();
                gltfTexture.Extensions = new Dictionary<string, IExtension>(){
                    {
                        "egret",
                        new TextureExtension(){
                            anisotropy = this.texture.anisoLevel,
                            format = GetTextureFormat(),
                            levels = mipmap ? 0 : 1
                        }
                    }
                };
            }
        }

        public int GetTextureFormat()
        {
            var texExt = PathHelper.GetTextureExt(this.texture);
            var format = this.texture.format;
            if (format == TextureFormat.Alpha8)
            {
                return 6409;
            }
            else if (texExt == "jpg" ||
             format == TextureFormat.RGB24 ||
             format == TextureFormat.PVRTC_RGB2 ||
             format == TextureFormat.PVRTC_RGB4 ||
             format == TextureFormat.RGB565 ||
             format == TextureFormat.ETC_RGB4 ||
             format == TextureFormat.ATC_RGB4 ||
             format == TextureFormat.ETC2_RGB ||
             format == TextureFormat.ASTC_RGB_4x4 ||
             format == TextureFormat.ASTC_RGB_5x5 ||
             format == TextureFormat.ASTC_RGB_6x6 ||
             format == TextureFormat.ASTC_RGB_8x8 ||
             format == TextureFormat.ASTC_RGB_10x10 ||
             format == TextureFormat.ASTC_RGB_12x12
             )
            {
                return 6407;
            }


            return 6408;
        }
    }
}

namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using GLTF.Schema;
    using UnityEngine;

    public class GLTFTextureArraySerializer : AssetSerializer
    {
        private Texture2DArrayData textureArray;

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
            foreach (var tex in this.textureArray.textures)
            {
                var path = PathHelper.GetTexturePath(tex);
                byte[] bs = ExportImage.Export(tex);
                if (!SerializeObject.assetsData.ContainsKey(path))
                {
                    var assetData = AssetData.Create(path);
                    assetData.buffer = bs;
                    SerializeObject.assetsData.Add(path, assetData);
                }
            }
        }

        protected override void Serialize(UnityEngine.Object sourceAsset)
        {
            this.textureArray = sourceAsset as Texture2DArrayData;
            var firstTexture = this.textureArray.textures[0];
            //先把原始图片导出来
            this.ExportTexture();

            var mipmap = firstTexture.mipmapCount > 1;
            //
            {
                var image = new Image();
                image.Uris = new List<string>();
                foreach (var tex in this.textureArray.textures)
                {
                    image.Uris.Add(ExportSetting.instance.GetExportPath(PathHelper.GetTexturePath(tex)));
                }
                this._root.Images.Add(image);

            }
            //
            {
                var filterMode = firstTexture.filterMode;
                var wrapMode = firstTexture.wrapMode;

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
                        TextureExtension.EXTENSION_NAME,
                        new TextureExtension(){
                            width = firstTexture.width,
                            height = firstTexture.height,
                            format = GetTextureFormat(),
                            levels = mipmap ? 0 : 1,
                            encoding = 2,
                            faces = 6,
                            mapping = 1,
                            anisotropy = firstTexture.anisoLevel,
                        }
                    }
                };
            }
        }

        public int GetTextureFormat()
        {
            var firstTexture = this.textureArray.textures[0];
            var texExt = PathHelper.GetTextureExt(firstTexture);
            var format = firstTexture.format;
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

namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;

    using Egret3DExportTools;

    public class TextureWriter : GLTFExporter
    {
        private UnityEngine.Texture2D texture;

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

        private void ExportTexture()
        {
            var tex = this.texture;
            var path = ExportImageTools.GetTexturePath(tex);
            UnityEditor.TextureImporter importer = (UnityEditor.TextureImporter)UnityEditor.TextureImporter.GetAtPath(path);
            bool isNormal = importer && importer.textureType == UnityEditor.TextureImporterType.NormalMap;
            string ext = ExportImageTools.GetTextureExt(tex);
            byte[] bs;

            var isSupported = ExportImageTools.IsSupportedExt(tex);
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

            ResourceManager.instance.AddFileBuffer(path, bs);
        }

        public override string writePath
        {
            get
            {
                string name = PathHelper.CheckFileName(this.texture.name + ".image.json");
                var texPath = ExportImageTools.GetTexturePath(this.texture);
            // //相对路径
                var imgdescPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1) + name;
                return imgdescPath;
            }
        }

        public override byte[] WriteGLTF(UnityEngine.Object sourceAsset)
        {
            this.texture = sourceAsset as UnityEngine.Texture2D;
            return base.WriteGLTF(sourceAsset);
        }

        protected override void WriteJson(MyJson_Tree gltfJson)
        {   
            //先把原始图片导出来
            this.ExportTexture();

            base.WriteJson(gltfJson);

            var mipmap = this.texture.mipmapCount > 1;

            var images = new MyJson_Array();
            var samplers = new MyJson_Array();
            var textures = new MyJson_Array();

            gltfJson.Add("images", images);
            gltfJson.Add("samplers", samplers);
            gltfJson.Add("textures", textures);

            //
            {
                var path = ExportImageTools.GetTexturePath(this.texture);
                var image = new MyJson_Tree();
                image.SetUri("uri", path);
                images.Add(image);
            }

            {
                var sampler = new MyJson_Tree();
                var filterMode = this.texture.filterMode;
                var wrapMode = this.texture.wrapMode;

                if (wrapMode == TextureWrapMode.Repeat)
                {
                    sampler.SetInt("wrapS", 10497);
                    sampler.SetInt("wrapT", 10497);
                }
                else
                {
                    sampler.SetInt("wrapS", 33071);
                    sampler.SetInt("wrapT", 33071);
                }

                sampler.SetInt("magFilter", filterMode == FilterMode.Point ? 9728 : 9729);
                if (!mipmap)
                {
                    sampler.SetInt("minFilter", filterMode == FilterMode.Point ? 9728 : 9729);
                }
                else if (filterMode == FilterMode.Point)
                {
                    sampler.SetInt("minFilter", 9984);
                }
                else if (filterMode == FilterMode.Bilinear)
                {
                    sampler.SetInt("minFilter", 9985);
                }
                else if (filterMode == FilterMode.Trilinear)
                {
                    sampler.SetInt("minFilter", 9987);
                }

                samplers.Add(sampler);
            }

            {
                var texture = new MyJson_Tree();
                texture.SetInt("sampler", 0);
                texture.SetInt("source", 0);
                var extensions = new MyJson_Tree();
                var egret = new MyJson_Tree();
                if (this.texture.anisoLevel > 1)
                {
                    egret.SetInt("anisotropy", this.texture.anisoLevel);
                }

                var texExt = ExportImageTools.GetTextureExt(this.texture);
                if (this.texture.format == TextureFormat.Alpha8)
                {
                    egret.SetInt("format", 6409);
                }
                else if (texExt == "jpg" ||
                 this.texture.format == TextureFormat.RGB24 ||
                 this.texture.format == TextureFormat.PVRTC_RGB2 ||
                 this.texture.format == TextureFormat.PVRTC_RGB4 ||
                 this.texture.format == TextureFormat.RGB565 ||
                 this.texture.format == TextureFormat.ETC_RGB4 ||
                 this.texture.format == TextureFormat.ATC_RGB4 ||
                 this.texture.format == TextureFormat.ETC2_RGB ||
                 this.texture.format == TextureFormat.ASTC_RGB_4x4 ||
                 this.texture.format == TextureFormat.ASTC_RGB_5x5 ||
                 this.texture.format == TextureFormat.ASTC_RGB_6x6 ||
                 this.texture.format == TextureFormat.ASTC_RGB_8x8 ||
                 this.texture.format == TextureFormat.ASTC_RGB_10x10 ||
                 this.texture.format == TextureFormat.ASTC_RGB_12x12
                 )
                {
                    egret.SetInt("format", 6407);
                }
                else
                {
                    egret.SetInt("format", 6408);
                }

                egret.SetInt("levels", mipmap ? 0 : 1);


                extensions.Add("egret", egret);
                texture.Add("extensions", extensions);
                textures.Add(texture);
            }
        }
    }
}

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
        private readonly string texPath;
        private readonly string texExt;
        private readonly UnityEngine.Texture2D target;
        public TextureWriter(UnityEngine.Texture2D target, string texPath, string texExt) : base()
        {
            this.target = target;
            this.texPath = texPath;
            this.texExt = texExt;
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

            var mipmap = this.target.mipmapCount > 1;

            var images = new MyJson_Array();
            var samplers = new MyJson_Array();
            var textures = new MyJson_Array();

            gltfJson.Add("images", images);
            gltfJson.Add("samplers", samplers);
            gltfJson.Add("textures", textures);

            //
            {
                var image = new MyJson_Tree();
                image.SetUri("uri", this.texPath);
                images.Add(image);
            }

            {
                var sampler = new MyJson_Tree();
                var filterMode = this.target.filterMode;
                var wrapMode = this.target.wrapMode;

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
                if (this.target.anisoLevel > 1)
                {
                    egret.SetInt("anisotropy", this.target.anisoLevel);
                }


                if (this.target.format == TextureFormat.Alpha8)
                {
                    egret.SetInt("format", 6409);
                }
                else if (this.texExt == "jpg" ||
                 this.target.format == TextureFormat.RGB24 ||
                 this.target.format == TextureFormat.PVRTC_RGB2 ||
                 this.target.format == TextureFormat.PVRTC_RGB4 ||
                 this.target.format == TextureFormat.RGB565 ||
                 this.target.format == TextureFormat.ETC_RGB4 ||
                 this.target.format == TextureFormat.ATC_RGB4 ||
                 this.target.format == TextureFormat.ETC2_RGB ||
                 this.target.format == TextureFormat.ASTC_RGB_4x4 ||
                 this.target.format == TextureFormat.ASTC_RGB_5x5 ||
                 this.target.format == TextureFormat.ASTC_RGB_6x6 ||
                 this.target.format == TextureFormat.ASTC_RGB_8x8 ||
                 this.target.format == TextureFormat.ASTC_RGB_10x10 ||
                 this.target.format == TextureFormat.ASTC_RGB_12x12
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

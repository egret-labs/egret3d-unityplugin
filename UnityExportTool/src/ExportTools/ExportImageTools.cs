using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

namespace Egret3DExportTools
{
    public class ExportImageTools
    {
        private static ExportImageTools _instance;
        public static ExportImageTools instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExportImageTools();
                }
                return _instance;
            }
        }

        //TODO
        public static string GetTextureExt(Texture tex)
        {
            string ext = "png";
            string path = AssetDatabase.GetAssetPath(tex);
            int i = path.LastIndexOf(".");
            if (i >= 0)
            {
                ext = path.Substring(i + 1);
            }

            return ext;
        }

        public static bool IsSupportedExt(Texture tex)
        {
            var ext = GetTextureExt(tex);
            return (ext == "png" || ext == "jpg" || ext == "jpeg");
        }

        public static string GetTexturePath(Texture tex)
        {
            var path = AssetDatabase.GetAssetPath(tex);
            var ext = GetTextureExt(tex);
            if (path == "Resources/unity_builtin_extra" || string.IsNullOrEmpty(path))
            {
                path = "Library/" + tex.name + "." + ext;
            }

            if (ext != "png" && ext != "jpg" && ext != "jpeg")
            {
                //非png、jpg都导出为png
                path = path.Substring(0, path.LastIndexOf(".") + 1) + "png";
            }

            return PathHelper.CheckFileName(path);
        }

        private readonly List<string> taskString = new List<string>();
        public ExportImageTools()
        {
            this.Clear();
        }
        public void Clear()
        {
            this.taskString.Clear();
        }
        public byte[] EncodeToPNG(Texture2D source, string ext = "png")
        {
            var path = AssetDatabase.GetAssetPath(source);
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            MyLog.Log("---导出图片:" + source.name + " path:" + path);
            var saveTextureType = TextureImporterType.Default;
            var isRestore = false;
            if (importer)
            {
                saveTextureType = importer.textureType;
                if (saveTextureType == TextureImporterType.NormalMap && !ExportToolsSetting.instance.unityNormalTexture)
                {
                    //法线贴图类型贴图因为Unity特殊处理过，如果要正常导出，就要转换一下类型
                    isRestore = true;
                    importer.textureType = TextureImporterType.Default;
                    importer.SaveAndReimport();
                }
            }

            var renderTexture = RenderTexture.GetTemporary(source.width, source.height);
            Graphics.Blit(source, renderTexture);
            RenderTexture.active = renderTexture;
            var exportTexture = new Texture2D(source.width, source.height);
            exportTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            exportTexture.Apply();

            byte[] res = null;
            try
            {
                if (ext == "jpg" || ext == "jpeg")
                {
                    res = exportTexture.EncodeToJPG();
                }
                else if (ext == "exr")
                {
                    res = exportTexture.EncodeToEXR();
                }
                else
                {
                    res = exportTexture.EncodeToPNG();
                }
            }
            catch (System.Exception e)
            {
                MyLog.LogError(e.StackTrace);
                MyLog.LogError("图片导出出错:" + path + " 请保证原始资源是可读写，非压缩文件");
            }

            if (isRestore && importer)
            {
                importer.textureType = saveTextureType;
                importer.SaveAndReimport();
            }

            return res;
        }
    }
}
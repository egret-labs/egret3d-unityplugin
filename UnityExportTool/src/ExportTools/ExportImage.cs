using UnityEngine;
using UnityEditor;

namespace Egret3DExportTools
{
    public static class ExportImage
    {
        public static byte[] Export(Cubemap source)
        {
            byte[] bs = null;

            return bs;
        } 
        public static byte[] Export(Texture2D source)
        {
            var path = AssetDatabase.GetAssetPath(source);
            var textureSetting = ExportSetting.instance.texture;
            MyLog.Log("---导出图片:" + source.name + " path:" + path);

            //只有jpg、png可以原始图片导出，其他类型不支持
            var fileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), path);
            byte[] bs = null;
            if (textureSetting.useOriginalTexture && PathHelper.IsSupportedExt(source) && System.IO.File.Exists(fileName))
            {
                bs = System.IO.File.ReadAllBytes(fileName);
            }
            else
            {
                var saveTextureType = TextureImporterType.Default;
                var importer = UnityEditor.TextureImporter.GetAtPath(path) as TextureImporter;
                if (importer)
                {
                    saveTextureType = importer.textureType;
                    if (saveTextureType == TextureImporterType.NormalMap && !textureSetting.useNormalTexture)
                    {
                        //法线贴图类型贴图因为Unity特殊处理过，如果要正常导出，就要转换一下类型
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

                try
                {
                    string ext = PathHelper.GetTextureExt(source);
                    if (ext == "jpg" || ext == "jpeg")
                    {
                        bs = exportTexture.EncodeToJPG(textureSetting.jpgQuality);
                    }
                    else if (ext == "exr")
                    {
                        bs = exportTexture.EncodeToEXR();
                    }
                    else
                    {
                        bs = exportTexture.EncodeToPNG();
                    }
                }
                catch (System.Exception e)
                {
                    MyLog.LogError(e.StackTrace);
                    MyLog.LogError("图片导出出错:" + path + " 请保证原始资源是可读写，非压缩文件");
                }

                if (importer && importer.textureType != saveTextureType)
                {
                    importer.textureType = saveTextureType;
                    importer.SaveAndReimport();
                }
            }

            return bs;
        }
    }
}
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

namespace Egret3DExportTools
{
    public static class ExportImageTools
    {
        public static byte[] EncodeToPNG(Texture2D source, string ext = "png")
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
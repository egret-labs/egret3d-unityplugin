using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Egret3DExportTools
{
    public class PathHelper
    {
        public static string OutPath = ""; //资源导出的路径		
        public static string SceneOrPrefabPath = "";
        public static string ConfigPath = Application.dataPath + "/Egret3DExportTools/config.json";
        /**
		 * 导出资源的默认保存根目录
		 */
        private static string _saveRootDirectory = "";

        public static string CurSceneName
        {
            get
            {
                var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                if (string.IsNullOrEmpty(activeScene.name))
                {
                    return string.Empty;
                }
                string name = PathHelper.CheckFileName(activeScene.path).Replace(".unity", "");
                return name;
            }
        }

        public static string SaveRootDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_saveRootDirectory))
                {
                    string rp = Path.GetFullPath(Application.dataPath);
                    rp = Path.GetDirectoryName(rp);
                    _saveRootDirectory = Path.Combine(rp, "export");
                    if (!Directory.Exists(_saveRootDirectory))
                    {
                        Directory.CreateDirectory(_saveRootDirectory);
                    }
                }
                return _saveRootDirectory;
            }
        }

        public static string CheckFileName(string fileName)
        {
            //空格替换
            var validName = fileName.Replace(" ", "_");
            //竖杠替换
            validName = validName.Replace("|", "_");
            //
            validName = validName.Replace("-", "_");
            //
            validName = validName.Replace("[]", "_");
            //斜杠替换
            validName = validName.Replace("\\", "/");
            //冒号替换
            var i = validName.LastIndexOf("/");
            if (i >= 0)
            {
                var temp = validName.Substring(i);
                temp = temp.Replace(":", "_");
                validName = validName.Substring(0, i) + temp;
            }

            return validName;
        }

        public static void SetSceneOrPrefabPath(string path)
        {
            PathHelper.SceneOrPrefabPath = path;
        }

        /**
		 * 设置资源的保存路径,考虑到windows和mac的区别
		 */
        public static void SetOutPutPath(string exportPath, string name = "")
        {
            PathHelper.OutPath = exportPath + "\\";
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                PathHelper.OutPath = exportPath + "/";
            }
            if (!Directory.Exists(PathHelper.OutPath))
            {
                Directory.CreateDirectory(PathHelper.OutPath);
            }
        }
        /**
		 * 计算相对路径
		 * Assets/res/elong/1525_firedragon02_d.png
		 * Assets/res/elong/e/1525_firedragon02_d.imgdesc.json
		 */
        public static string GetRelativePath(string targetPath, string sourcePath)
        {
            string relPath = "";
            targetPath = targetPath.Replace("\\", "/");
            sourcePath = sourcePath.Replace("\\", "/");
            string[] targetPathArr = targetPath.Split(new char[] { '/' });
            string[] sourcePathArr = sourcePath.Split(new char[] { '/' });
            int targetPathLen = targetPathArr.Length;
            int sourcePathLen = sourcePathArr.Length;
            int i = 0;
            while (targetPathArr[i] == sourcePathArr[i] && i < targetPathLen && i < sourcePathLen)
            {
                i++;
            }
            for (int j = 0; j < sourcePathLen - i - 1; j++)
            {
                relPath += "../";
            }
            relPath = relPath + string.Join("/", targetPathArr.Skip(i).ToArray());
            relPath = PathHelper.CheckFileName(relPath);

            return relPath;
        }

        public static string GetFileDirectory(string filePath)
        {
            return filePath.Substring(0, filePath.LastIndexOf("/") + 1);
        }

        public static string GetAssetPath(UnityEngine.Object asset)
        {
            if(asset is UnityEngine.Mesh)
            {
                return GetMeshPath(asset as UnityEngine.Mesh);
            }
            else if(asset is UnityEngine.Material)
            {
                return GetMaterialPath(asset as UnityEngine.Material);
            }
            else if(asset is UnityEngine.AnimationClip)
            {
                return GetAnimationClipPath(asset as UnityEngine.AnimationClip);
            }
            else if(asset is UnityEngine.Texture)
            {
                return GetTextureDescPath(asset as UnityEngine.Texture);
            }

            return "";
        }

        public static string GetMeshPath(UnityEngine.Mesh mesh)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(mesh);
            //obj
            var extendName = "";
            var extend = path.Substring(path.LastIndexOf(".") + 1);
            if (extend == "obj")
            {
                extendName = path.Substring(path.LastIndexOf("/") + 1);
                extendName = extendName.Substring(0, extendName.LastIndexOf(".")) + "_";
            }
            path = path.Substring(0, path.LastIndexOf("/") + 1);

            path = PathHelper.CheckFileName(path + extendName + mesh.name + ".mesh.bin");
            return path;
        }

        public static string GetMaterialPath(UnityEngine.Material material)
        {
            var mat = material;
            string path = UnityEditor.AssetDatabase.GetAssetPath(mat);
            if (path == "Resources/unity_builtin_extra")
            {
                path = "Library/" + mat.name + ".mat";
            }
            if (!path.Contains(".mat"))
            {
                path += "." + mat.name + ".mat";//.obj文件此时应该导出不同的材质文件，GetAssetPath获取的确实同一个
            }
            path = PathHelper.CheckFileName(path + ".json");
            return path;
        }

        public static string GetAnimationClipPath(UnityEngine.AnimationClip clip)
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(clip);
            path = path.Substring(0, path.LastIndexOf(".")) + "_" + clip.name + ".ani.bin";
            path = PathHelper.CheckFileName(path);
            return path;
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

        public static string GetTextureDescPath(UnityEngine.Texture texture)
        {
            //TODO
            var path = GetTexturePath(texture);
            // //相对路径
            path = path.Substring(0, path.LastIndexOf("/") + 1) + texture.name + ".image.json";
            path = PathHelper.CheckFileName(path);
            return path;
        }

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
    }
}
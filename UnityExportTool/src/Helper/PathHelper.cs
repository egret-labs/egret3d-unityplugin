using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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

        /**
		 * 判断命名是否合法
		 */
        public static bool LegalName(string name, string objType)
        {
            Regex regex = new Regex("[^0-9a-zA-Z_+-.@() /]");
            Match match = regex.Match(name);
            if (match.Success)
            {
                MyLog.LogError(objType + "不合法的命名：" + name + "只支持字母、数字、_+-.@");
            }
            return !match.Success;
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
    }
}
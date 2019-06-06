using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Egret3DExportTools
{
    public class ResourceManager
    {
        // public const int VERSION = 4;//资源版本号
        private static ResourceManager _instance;
        public static ResourceManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ResourceManager();
                }
                return _instance;
            }
        }

        private int _hashIndex;
        private readonly Dictionary<int, int> _hashList = new Dictionary<int, int>();
        //
        // private readonly List<MyJson_Object> _objects = new List<MyJson_Object>();
        // private readonly List<MyJson_Object> _comps = new List<MyJson_Object>();
        // private readonly List<string> _assets = new List<string>();
        //
        // private readonly Dictionary<string, byte[]> _fileBuffers = new Dictionary<string, byte[]>(); //导出文件的名称和内容
        // private readonly Dictionary<int, string> _saveCache = new Dictionary<int, string>(); //资源缓存             

        private ResourceManager()
        {
            // this.Clean();
        }

        // private void ConvertJsonToString(System.Text.StringBuilder sb)
        // {
        //     // this._objects.Sort
        //     sb.Append("{\n\t\"assets\":[\n");
        //     for (int i = 0; i < this._assets.Count; i++)
        //     {
        //         var assetUrl = this._assets[i];
        //         MyJson_Object asset = new MyJson_Object();
        //         sb.Append("\t\t");
        //         // sb.Append(asset.ToString());
        //         var relativePath = assetUrl;
        //         relativePath = relativePath.Replace("Assets", ExportConfig.instance.rootDir);

        //         sb.Append('"' + relativePath + '"');//现在版本Asset只导出url
        //         if (i != this._assets.Count - 1)
        //         {
        //             sb.Append(',');
        //         }
        //         sb.Append("\n");
        //     }
        //     sb.Append("\t],\n\t\"entities\":[\n");
        //     for (int i = 0; i < this._objects.Count; i++)
        //     {
        //         MyJson_Object obj = this._objects[i];
        //         sb.Append("\t\t");
        //         sb.Append(obj.ToString());
        //         if (i != this._objects.Count - 1)
        //         {
        //             sb.Append(',');
        //         }
        //         sb.Append("\n");
        //     }
        //     sb.Append("\t],\n\t\"components\":[\n");
        //     for (int i = 0; i < this._comps.Count; i++)
        //     {
        //         MyJson_Object comp = this._comps[i];
        //         sb.Append("\t\t");
        //         sb.Append(comp.ToString());
        //         if (i != this._comps.Count - 1)
        //         {
        //             sb.Append(',');
        //         }
        //         sb.Append("\n");
        //     }
        //     //
        //     sb.Append("\t],\n");
        //     sb.Append("\n\t\"version\":" + VERSION + "\n}");
        // }

        // public void Clean()
        // {
        //     MyLog.Log("清除缓存");
        //     this._hashIndex = 1;
        //     this._hashList.Clear();
        //     this._objects.Clear();
        //     this._comps.Clear();
        //     this._assets.Clear();
        //     this._fileBuffers.Clear();
        //     this._saveCache.Clear();
        // }

        public int ResetHash(int unityHash)
        {
            int newHash;
            if (this._hashList.TryGetValue(unityHash, out newHash))
            {
                return newHash;
            }
            newHash = this._hashIndex++;
            this._hashList[unityHash] = newHash;
            return newHash;
        }

        // public void ExportFiles(string sceneOrPrefabPath, string exportPath = "")
        // {
        //     //得到.prefab.json和.scene.json的数据
        //     System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //     this.ConvertJsonToString(sb);
        //     //
        //     byte[] bs = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        //     this.AddFileBuffer(sceneOrPrefabPath, bs);
        //     //
        //     if (!System.IO.Directory.Exists(exportPath))
        //     {
        //         System.IO.Directory.CreateDirectory(exportPath);
        //     }
        //     foreach (var fileBuffer in ResourceManager.instance.fileBuffers)
        //     {
        //         if (string.IsNullOrEmpty(fileBuffer.Key))
        //         {
        //             continue;
        //         }
        //         //写入文件
        //         var relativePath = fileBuffer.Key;
        //         relativePath = relativePath.Replace("Assets", ExportConfig.instance.rootDir);
        //         var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(exportPath, relativePath));
        //         MyLog.Log("---导出文件:" + relativePath);
        //         //创建路径
        //         var fileDirectory = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
        //         if (!System.IO.Directory.Exists(fileDirectory))
        //         {
        //             System.IO.Directory.CreateDirectory(fileDirectory);
        //         }
        //         System.IO.File.WriteAllBytes(filePath, fileBuffer.Value);
        //     }
        // }

        // public int AddObjectJson(MyJson_Object item)
        // {
        //     for (var i = 0; i < this._objects.Count; i++)
        //     {
        //         var asset = this._objects[i];
        //         if (asset["uuid"] == item["uuid"])
        //         {
        //             return i;
        //         }
        //     }
        //     var index = this._objects.Count;
        //     this._objects.Add(item);
        //     return index;
        // }
        // public int AddCompJson(MyJson_Object item)
        // {
        //     for (var i = 0; i < this._comps.Count; i++)
        //     {
        //         var asset = this._comps[i];
        //         if (asset["uuid"] == item["uuid"])
        //         {
        //             return i;
        //         }
        //     }
        //     var index = this._comps.Count;
        //     this._comps.Add(item);
        //     return index;
        // }

        // public int AddAssetUrl(string assetUrl)
        // {
        //     var index = this._assets.IndexOf(assetUrl);
        //     if (index < 0)
        //     {
        //         index = this._assets.Count;
        //         this._assets.Add(assetUrl);
        //     }

        //     return index;
        // }

        // public bool AddFileBuffer(string key, byte[] buffer)
        // {
        //     if (string.IsNullOrEmpty(key))
        //     {
        //         return false;
        //     }
        //     MyLog.Log("添加导出文件:" + key);
        //     if (this._fileBuffers.ContainsKey(key))
        //     {
        //         this._fileBuffers[key] = buffer;
        //     }
        //     else
        //     {
        //         this._fileBuffers.Add(key, buffer);
        //     }
        //     return true;
        // }

        // public bool HaveCache(int hashCode)
        // {
        //     return this._saveCache.ContainsKey(hashCode);
        // }

        // public string GetCache(int hashCode)
        // {
        //     string res;
        //     this._saveCache.TryGetValue(hashCode, out res);
        //     return res;
        // }

        // public void SaveCache(int hashCode, string value)
        // {
        //     this._saveCache.Add(hashCode, value);
        // }

        // public List<MyJson_Object> objects { get { return this._objects; } }
        // public List<MyJson_Object> comps { get { return this._comps; } }
        // public List<string> assets { get { return this._assets; } }
        // public Dictionary<string, byte[]> fileBuffers { get { return this._fileBuffers; } }
    }
}
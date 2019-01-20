using System;
using System.Collections.Generic;
using FB.PosePlus;
using UnityEditor;
using UnityEngine;

namespace Egret3DExportTools
{
    public class ResourceManager
    {
        public const int VERSION = 4;//资源版本号
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
        private readonly List<MyJson_Object> _objects = new List<MyJson_Object>();
        private readonly List<MyJson_Object> _comps = new List<MyJson_Object>();
        private readonly List<string> _assets = new List<string>();
        //
        private readonly Dictionary<string, byte[]> _fileBuffers = new Dictionary<string, byte[]>(); //导出文件的名称和内容
        private readonly Dictionary<int, string> _saveCache = new Dictionary<int, string>(); //资源缓存             

        private ResourceManager()
        {
            this.Clean();
        }

        private void ConvertJsonToString(System.Text.StringBuilder sb)
        {
            sb.Append("{\n\t\"assets\":[\n");
            for (int i = 0; i < this._assets.Count; i++)
            {
                var assetUrl = this._assets[i];
                MyJson_Object asset = new MyJson_Object();
                sb.Append("\t\t");
                // sb.Append(asset.ToString());
                sb.Append('"' + assetUrl + '"');//现在版本Asset只导出url
                if (i != this._assets.Count - 1)
                {
                    sb.Append(',');
                }
                sb.Append("\n");
            }
            sb.Append("\t],\n\t\"objects\":[\n");
            for (int i = 0; i < this._objects.Count; i++)
            {
                MyJson_Object obj = this._objects[i];
                sb.Append("\t\t");
                sb.Append(obj.ToString());
                if (i != this._objects.Count - 1)
                {
                    sb.Append(',');
                }
                sb.Append("\n");
            }
            sb.Append("\t],\n\t\"components\":[\n");
            for (int i = 0; i < this._comps.Count; i++)
            {
                MyJson_Object comp = this._comps[i];
                sb.Append("\t\t");
                sb.Append(comp.ToString());
                if (i != this._comps.Count - 1)
                {
                    sb.Append(',');
                }
                sb.Append("\n");
            }
            //
            sb.Append("\t],\n");
            sb.Append("\n\t\"version\":" + VERSION + "\n}");
        }

        public void Clean()
        {
            MyLog.Log("清除缓存");
            this._hashIndex = 1;
            this._hashList.Clear();
            this._objects.Clear();
            this._comps.Clear();
            this._assets.Clear();
            this._fileBuffers.Clear();
            this._saveCache.Clear();
        }

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

        public void ExportFiles(string sceneOrPrefabPath, string exportPath = "")
        {
            //得到.prefab.json和.scene.json的数据
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            this.ConvertJsonToString(sb);
            //
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            this.AddFileBuffer(sceneOrPrefabPath, bs);
            //
            if (!System.IO.Directory.Exists(exportPath))
            {
                System.IO.Directory.CreateDirectory(exportPath);
            }
            foreach (var fileBuffer in ResourceManager.instance.fileBuffers)
            {
                if (string.IsNullOrEmpty(fileBuffer.Key))
                {
                    continue;
                }
                //写入文件
                var filePath = PathHelper.CheckFileName(System.IO.Path.Combine(exportPath, fileBuffer.Key));
                MyLog.Log("---导出文件:" + fileBuffer.Key);
                //创建路径
                var fileDirectory = filePath.Substring(0, filePath.LastIndexOf("/") + 1);
                if (!System.IO.Directory.Exists(fileDirectory))
                {
                    System.IO.Directory.CreateDirectory(fileDirectory);
                }
                System.IO.File.WriteAllBytes(filePath, fileBuffer.Value);
            }
        }

        public int AddObjectJson(MyJson_Object item)
        {
            for (var i = 0; i < this._objects.Count; i++)
            {
                var asset = this._objects[i];
                if (asset["uuid"] == item["uuid"])
                {
                    return i;
                }
            }
            var index = this._objects.Count;
            this._objects.Add(item);
            return index;
        }
        public int AddCompJson(MyJson_Object item)
        {
            for (var i = 0; i < this._comps.Count; i++)
            {
                var asset = this._comps[i];
                if (asset["uuid"] == item["uuid"])
                {
                    return i;
                }
            }
            var index = this._comps.Count;
            this._comps.Add(item);
            return index;
        }

        public int AddAssetUrl(string assetUrl)
        {
            var index = this._assets.IndexOf(assetUrl);
            if (index < 0)
            {
                index = this._assets.Count;
                this._assets.Add(assetUrl);
            }

            return index;
        }

        public bool AddFileBuffer(string key, byte[] buffer)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            MyLog.Log("添加导出文件:" + key);
            if (this._fileBuffers.ContainsKey(key))
            {
                this._fileBuffers[key] = buffer;
            }
            else
            {
                this._fileBuffers.Add(key, buffer);
            }
            return true;
        }

        public bool HaveCache(int hashCode)
        {
            return this._saveCache.ContainsKey(hashCode);
        }

        public string GetCache(int hashCode)
        {
            string res;
            this._saveCache.TryGetValue(hashCode, out res);
            return res;
        }

        public void SaveCache(int hashCode, string value)
        {
            this._saveCache.Add(hashCode, value);
        }

        public string SaveMesh(Transform target, Mesh mesh)
        {
            int id = mesh.GetInstanceID();
            if (this.HaveCache(id))
            {
                return this.GetCache(id);
            }

            var gltf = new MeshWriter(target);
            byte[] bs = gltf.WriteGLTF();
            var url = UnityEditor.AssetDatabase.GetAssetPath(mesh);
            //obj
            var extendName = "";
            var extend = url.Substring(url.LastIndexOf(".") + 1);
            if (extend == "obj")
            {
                extendName = url.Substring(url.LastIndexOf("/") + 1);
                extendName = extendName.Substring(0, extendName.LastIndexOf(".")) + "_";
            }
            url = url.Substring(0, url.LastIndexOf("/") + 1);

            var name = PathHelper.CheckFileName(url + extendName + mesh.name + ".mesh.bin");
            this.AddFileBuffer(name, bs);
            this.SaveCache(id, name);
            return name;
        }

        public string SaveMaterial(Material mat, bool isParticle = false, bool isAnimation = false)
        {
            int id = mat.GetInstanceID();
            if (this.HaveCache(id))
            {
                return this.GetCache(id);
            }

            string tempMatPath = UnityEditor.AssetDatabase.GetAssetPath(mat);
            if (tempMatPath == "Resources/unity_builtin_extra")
            {
                tempMatPath = "Library/" + mat.name + ".mat";
            }
            if (!tempMatPath.Contains(".mat"))
            {
                tempMatPath += "." + mat.name + ".mat";//.obj文件此时应该导出不同的材质文件，GetAssetPath获取的确实同一个
            }
            var name = PathHelper.CheckFileName(tempMatPath + ".json");

            var gltf = new MaterialWriter(mat, isParticle, isAnimation);
            byte[] bs = gltf.WriteGLTF();
            this.AddFileBuffer(name, bs);
            this.SaveCache(id, name);
            return name;
        }

        /**
         * 获取纹理信息
         */
        public string SaveTexture(Texture2D tex, string matPath)
        {
            int id = tex.GetInstanceID();
            if (this.HaveCache(id))
            {
                return this.GetCache(id);
            }
            var name = SaveTextureEditor(tex, matPath);
            this.SaveCache(id, name);
            return name;
        }

        /**
         * 保存纹理图片
         */
        public string SaveTextureEditor(Texture2D tex, string matPath)
        {
            string localp = System.IO.Path.GetDirectoryName(Application.dataPath);
            string path = AssetDatabase.GetAssetPath(tex);

            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            bool isNormal = importer && importer.textureType == TextureImporterType.NormalMap;
            string filename = localp + "/" + path;
            int i = filename.LastIndexOf(".");
            string ext = "png";
            if (i >= 0)
            {
                ext = filename.Substring(i + 1);
            }
            byte[] bs;
            string name = tex.name + "." + ext;
            name = PathHelper.CheckFileName(name);
            string rename;
            if (path == "Resources/unity_builtin_extra" || string.IsNullOrEmpty(path))
            {
                path = "Library/" + tex.name + "." + ext;
            }
            {
                var isSupported = true;
                if (ext != "png" && ext != "jpg" && ext != "jpeg")
                {
                    path = path.Substring(0, path.LastIndexOf(".") + 1) + "png";
                    //非png、jpg都导出为png
                    ext = "png";
                    isSupported = false;
                }

                //只有jpg、png可以原始图片导出，其他类型不支持
                if (ExportToolsSetting.instance.exportOriginalImage && isSupported && System.IO.File.Exists(filename))
                {
                    MyLog.Log("原始图片:" + filename);
                    bs = System.IO.File.ReadAllBytes(filename);
                }
                else
                {
                    bs = ExportImageTools.instance.EncodeToPNG(tex, ext);
                }

                path = PathHelper.CheckFileName(path);
                this.AddFileBuffer(path, bs);
                rename = SaveTextureFormat(tex, path, matPath, false, ext, isNormal);
            }

            return rename;
        }

        /**
         * 保存纹理图片相关信息
         */
        public string SaveTextureFormat(Texture2D tex, string texPath, string matPath, bool closemipmap = false, string ext = "png", bool normal = false)
        {
            string name = PathHelper.CheckFileName(tex.name + ".image.json");

            MyJson_Tree textureItem = new MyJson_Tree();
            var fileName = texPath.Substring(0, texPath.LastIndexOf(".") + 1) + ext;
            textureItem.SetString("name", PathHelper.CheckFileName(texPath));
            textureItem.SetEnum("filterMode", tex.filterMode, true);
            textureItem.SetEnum("wrap", tex.wrapMode, true);
            textureItem.SetBool("mipmap", !closemipmap && tex.mipmapCount > 1);

            if (tex.anisoLevel > 1)
            {
                textureItem.SetNumber("anisotropy", tex.anisoLevel);
            }

            if (tex.format == TextureFormat.Alpha8)
            {
                textureItem.SetString("format", "Gray");
            }
            else if (ext == "jpg" ||
             tex.format == TextureFormat.RGB24 ||
             tex.format == TextureFormat.PVRTC_RGB2 ||
             tex.format == TextureFormat.PVRTC_RGB4 ||
             tex.format == TextureFormat.RGB565 ||
             tex.format == TextureFormat.ETC_RGB4 ||
             tex.format == TextureFormat.ATC_RGB4 ||
             tex.format == TextureFormat.ETC2_RGB ||
             tex.format == TextureFormat.ASTC_RGB_4x4 ||
             tex.format == TextureFormat.ASTC_RGB_5x5 ||
             tex.format == TextureFormat.ASTC_RGB_6x6 ||
             tex.format == TextureFormat.ASTC_RGB_8x8 ||
             tex.format == TextureFormat.ASTC_RGB_10x10 ||
             tex.format == TextureFormat.ASTC_RGB_12x12
             )
            {
                textureItem.SetString("format", "RGB");
            }

            textureItem.SetInt("version", 2);

            //得到.imgdesc.json数据，并保存到bufs中
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            textureItem.CovertToStringWithFormat(sb, 4);
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            //相对路径
            var imgdescPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1) + name;
            this.AddFileBuffer(imgdescPath, bs);

            return imgdescPath;
        }

        public string SaveAniPlayer(AniPlayer player, Animator animator)
        {
            int id = player.GetInstanceID();
            if (this.HaveCache(id))
            {
                return this.GetCache(id);
            }
            string path = UnityEditor.AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
            var writer = new AnimationWriter(player.transform);
            byte[] bs = writer.WriteGLTF();
            var name = path.Substring(0, path.LastIndexOf("/") + 1) + player.gameObject.name + ".ani.bin";
            this.AddFileBuffer(name, bs);
            return name;
        }

        public List<MyJson_Object> objects { get { return this._objects; } }
        public List<MyJson_Object> comps { get { return this._comps; } }
        public List<string> assets { get { return this._assets; } }
        public Dictionary<string, byte[]> fileBuffers { get { return this._fileBuffers; } }
    }
}
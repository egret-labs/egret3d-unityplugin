using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace Egret3DExportTools
{
    public static class ExportExtendTools
    {
        [MenuItem("Egret3DExportTools/CheckEmptyTextures")]
        public static void CheckEmptyTexture()
        {
            Selection.activeGameObject = null;
            GameObject[] allObjs = GameObject.FindObjectsOfType<GameObject>();
            Debug.Log("--------------排查材质空贴图----------------");
            List<GameObject> selected = new List<GameObject>();
            foreach (var obj in allObjs)
            {
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                if (!meshRenderer)
                {
                    continue;
                }

                //
                var mats = meshRenderer.sharedMaterials;
                foreach (var mat in mats)
                {
                    UnityEngine.Texture texture = null;
                    UnityEditor.MaterialProperty[] orginmps = UnityEditor.MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { mat });
                    foreach (var mp in orginmps)
                    {
                        if (mp.type.ToString() == "Texture")
                        {
                            var tex = mat.GetTexture(mp.name);
                            if (tex != null)
                            {
                                texture = tex;
                                break;
                            }
                        }
                    }

                    if (texture == null)
                    {
                        selected.Add(obj);
                        Debug.LogWarning("缺少贴图:" + obj.name);
                    }

                }
            }
            // Selection.
            Selection.objects = selected.ToArray();
            Debug.Log("--------------排查完毕----------------");
        }

        [MenuItem("Egret3DExportTools/CheckDuplicateMeshs")]
        public static void CheckDuplicateMeshs()
        {
            var selectedObj = Selection.activeGameObject;
            if (selectedObj == null || selectedObj.GetComponent<MeshFilter>() == null || selectedObj.GetComponent<MeshFilter>().sharedMesh == null)
            {
                Debug.LogError("目标选择错误");
                return;
            }

            GameObject[] allObj = GameObject.FindObjectsOfType<GameObject>();
            Debug.Log("--------------排查重复网格----------------");
            var targetList = new List<GameObject>();
            //按照名字收集
            foreach (var obj in allObj)
            {
                var meshFilter = obj.GetComponent<MeshFilter>();
                if (!meshFilter || meshFilter.sharedMesh == null)
                {
                    continue;
                }

                if (obj.GetInstanceID() != selectedObj.GetInstanceID())
                {
                    if (targetList.IndexOf(obj) < 0)
                    {
                        targetList.Add(obj);
                    }
                }
            }
            //同名字查找相同网格的
            List<GameObject> selected = new List<GameObject>();
            var sourceMesh = selectedObj.GetComponent<MeshFilter>().sharedMesh;
            var sourceVertices = sourceMesh.vertices;
            for (int i = 0; i < targetList.Count; i++)
            {
                var target = targetList[i];
                var targetMesh = target.GetComponent<MeshFilter>().sharedMesh;
                var targetVertices = targetMesh.vertices;

                //长度相同
                var isDuplicate = false;
                if (sourceVertices.Length == targetVertices.Length && sourceMesh.GetInstanceID() != targetMesh.GetInstanceID())
                {
                    isDuplicate = true;
                    for (int k = 0; k < sourceVertices.Length; k++)
                    {
                        if (sourceVertices[k].Equals(targetVertices[k]))
                        {
                            isDuplicate = false;
                            break;
                        }
                    }
                }

                if (isDuplicate)
                {
                    if (selected.IndexOf(target) < 0)
                    {
                        selected.Add(target);
                    }
                }
            }
            Selection.objects = selected.ToArray();
            Debug.Log("--------------排查完毕----------------");
        }

        [MenuItem("Egret3DExportTools/OptimizingObjName")]
        public static void OptimizingObjName()
        {
            var selectedObj = Selection.activeGameObject;
            if (selectedObj == null)
            {
                Debug.LogWarning("没有选中任何对象");
                return;
            }
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(selectedObj);
            Debug.Log("选中的:" + selectedObj.name + " 类型:" + System.IO.Path.GetExtension(assetPath));

            if (string.IsNullOrEmpty(assetPath) || System.IO.Path.GetExtension(assetPath) != ".obj")
            {
                Debug.LogWarning("不是obj网格");
                return;
            }

            //加载obj文件
            string objText = File.ReadAllText(assetPath); ;
            Dictionary<string, int> renameDic = new Dictionary<string, int>();
            string[] allObjLines = objText.Split('\n');
            int nameIndex = 0;
            for (int i = 0, l = allObjLines.Length; i < l; i++)
            {
                string line = allObjLines[i];
                //将每一行按空格分割
                char[] charsToTrim = { ' ' };
                string[] chars = line.TrimEnd('\r').TrimStart(' ').Split(charsToTrim, StringSplitOptions.RemoveEmptyEntries);
                if (chars.Length <= 0)
                {
                    continue;
                }

                if (chars[0] == "g")
                {
                    //移除g
                    var key = line.Replace("g ", "");
                    key = key.Replace(" ", "_").Trim();
                    renameDic[key] = nameIndex;
                    allObjLines[i] = "g " + nameIndex;
                    nameIndex++;
                }
            }

            //加载obj文件的.meta文件,重置依赖关系
            string metaText = File.ReadAllText(assetPath + ".meta");
            string[] allMetaLines = metaText.Split('\n');
            //遍历renameDic中的名字替换
            for (int i = 0, l = allMetaLines.Length; i < l; i++)
            {
                string line = allMetaLines[i];
                int lastSpaceIndex = line.LastIndexOf(" ");
                if (lastSpaceIndex < 0)
                {
                    continue;
                }
                string name = line.Substring(lastSpaceIndex + 1).Trim();
                foreach (var rename in renameDic.Keys)
                {
                    if (name == rename)
                    {
                        allMetaLines[i] = line.Replace(name, renameDic[rename].ToString());
                    }
                }
            }

            File.WriteAllText(assetPath, string.Join("\n", allObjLines));
            File.WriteAllText(assetPath + ".meta", string.Join("\n", allMetaLines));
            //
            AssetDatabase.Refresh();
        }

        /**
         * 清除空组件
         */
        public static void CleanupMissingScripts()
        {
            GameObject[] allObjs = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < allObjs.Length; i++)
            {
                var gameObject = allObjs[i];

                // We must use the GetComponents array to actually detect missing components
                var components = gameObject.GetComponents<Component>();

                // Create a serialized object so that we can edit the component list
                var serializedObject = new SerializedObject(gameObject);
                // Find the component list property
                var prop = serializedObject.FindProperty("m_Component");

                // Track how many components we've removed
                int r = 0;

                // Iterate over all components
                for (int j = 0; j < components.Length; j++)
                {
                    // Check if the ref is null
                    if (components[j] == null)
                    {
                        // If so, remove from the serialized component array
                        prop.DeleteArrayElementAtIndex(j - r);
                        // Increment removed count
                        r++;
                    }
                }

                // Apply our changes to the game object
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

}


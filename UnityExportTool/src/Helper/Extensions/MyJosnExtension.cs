using UnityEngine;
using System;
using System.Collections.Generic;

namespace Egret3DExportTools
{
    public static class MyJosnExtension
    {
        //-----------------------------基础部分----------------------------------
        public static void SetBool(this Dictionary<string, IJsonNode> jsonNode, string key, bool value)
        {
            jsonNode[key] = new MyJson_Number(value);
        }
        public static void SetInt(this Dictionary<string, IJsonNode> jsonNode, string key, int value)
        {
            jsonNode[key] = new MyJson_Number(value);
        }
        public static void SetNumber(this Dictionary<string, IJsonNode> jsonNode, string key, float value, int? digits = null)
        {
            jsonNode[key] = new MyJson_Number(Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits));
        }
        public static void SetNumber(this Dictionary<string, IJsonNode> jsonNode, string key, double value, int? digits = null)
        {
            jsonNode[key] = new MyJson_Number(Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits));
        }
        public static void SetString(this Dictionary<string, IJsonNode> jsonNode, string key, string value)
        {
            jsonNode[key] = new MyJson_String(value);
        }
        public static void SetEnum(this Dictionary<string, IJsonNode> jsonNode, string key, Enum value, bool toString = false)
        {
            if (toString)
            {
                jsonNode[key] = new MyJson_String(value);
            }
            else
            {
                jsonNode[key] = new MyJson_Number(value);
            }
        }
        //-------------------------------扩展部分--------------------------------
        /**
         * 组件的公共部分，Vector2
         */
        public static void SetVector2(this Dictionary<string, IJsonNode> jsonNode, string desc, Vector2 data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            jsonNode[desc] = cItemArr;
        }
        /**
         * 组件的公共部分，Vector3
         */
        public static void SetVector3(this Dictionary<string, IJsonNode> jsonNode, string desc, Vector3 data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.z, digits);
            jsonNode[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Vector4
         */
        public static void SetVector4(this Dictionary<string, IJsonNode> jsonNode, string desc, Vector4 data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.z, digits);
            cItemArr.AddNumber(data.w, digits);

            jsonNode[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Quaternion
         */
        public static void SetQuaternion(this Dictionary<string, IJsonNode> jsonNode, string desc, Quaternion data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.z, digits);
            cItemArr.AddNumber(data.w, digits);

            jsonNode[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Color
         */
        public static void SetColor(this Dictionary<string, IJsonNode> jsonNode, string desc, Color data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.r, digits);
            cItemArr.AddNumber(data.g, digits);
            cItemArr.AddNumber(data.b, digits);
            cItemArr.AddNumber(data.a, digits);

            jsonNode[desc] = cItemArr;
        }
        public static void SetColor3(this Dictionary<string, IJsonNode> jsonNode, string desc, Color data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.r, digits);
            cItemArr.AddNumber(data.g, digits);
            cItemArr.AddNumber(data.b, digits);

            jsonNode[desc] = cItemArr;
        }
        /**
         * 组件的公共部分 Rect
         */
        public static void SetRect(this Dictionary<string, IJsonNode> jsonNode, string desc, Rect data, int? digits = null)
        {
            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(data.x, digits);
            cItemArr.AddNumber(data.y, digits);
            cItemArr.AddNumber(data.width, digits);
            cItemArr.AddNumber(data.height, digits);

            jsonNode[desc] = cItemArr;
        }
        public static void SetUVTransform(this Dictionary<string, IJsonNode> jsonNode, string desc, Vector4 data, int? digits = null)
        {
            var tx = data.z;
            var ty = data.w;
            var sx = data.x;
            var sy = data.y;
            var cx = 0.0f;
            var cy = 0.0f;
            var rotation = 0.0f;
            var c = Math.Cos(rotation);
            var s = Math.Sin(rotation);

            MyJson_Array cItemArr = new MyJson_Array();
            cItemArr.AddNumber(sx * c);
            cItemArr.AddNumber(sx * s);
            cItemArr.AddNumber(-sx * (c * cx + s * cy) + cx + tx);
            cItemArr.AddNumber(-sy * s);
            cItemArr.AddNumber(sy * c);
            cItemArr.AddNumber(-sy * (-s * cx + c * cy) + cy + ty);
            cItemArr.AddNumber(0.0);
            cItemArr.AddNumber(0.0);
            cItemArr.AddNumber(1.0);

            jsonNode[desc] = cItemArr;
        }

        //------------------------------------------------------复杂的数据-----------------------------------------------------------
        public static void SetUUID(this Dictionary<string, IJsonNode> jsonNode, string value)
        {
            jsonNode["uuid"] = new MyJson_String(value);
        }
        public static void SetUnityID(this Dictionary<string, IJsonNode> jsonNode, int value)
        {
            jsonNode["unityId"] = new MyJson_Number(value);
        }
        public static void SetHashCode(this Dictionary<string, IJsonNode> jsonNode, string key, UnityEngine.Object value)
        {
            jsonNode[key] = new MyJson_HashCode(value.GetInstanceID());
        }
        public static void SetClass(this Dictionary<string, IJsonNode> jsonNode, string className)
        {
            jsonNode.SetString("class", className);
        }
        public static void SetComponent(this Dictionary<string, IJsonNode> jsonNode, int compHash, string className)
        {
            jsonNode.SetUUID(compHash.ToString());
            jsonNode.SetUnityID(compHash);
            jsonNode.SetClass(className);
        }
        public static void SetAsset(this Dictionary<string, IJsonNode> jsonNode, string url)
        {
            jsonNode.SetString("url", url);
        }
        public static void SetMesh(this MyJson_Object jsonNode, GameObject obj, Mesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            int meshHash = mesh.GetInstanceID();
            string url = ResourceManager.instance.SaveMesh(obj.transform, mesh);
            var assetIndex = ResourceManager.instance.AddAssetUrl(url);

            //mesh
            var meshItem = new MyJson_Tree(false);
            meshItem.SetInt("asset", assetIndex);
            jsonNode["_mesh"] = meshItem;
        }
        public static void SetMaterials(this MyJson_Object jsonNode, GameObject obj, Material[] materials, bool isParticleMat = false, bool isAnimationMat = false)
        {
            var materialsItem = new MyJson_Array();
            jsonNode["_materials"] = materialsItem;
            //写材质
            foreach (var material in materials)
            {
                if (!material)
                {
                    Debug.LogWarning(obj.gameObject.name + " 材质缺失，请检查资源");
                    continue;
                }

                int hash = material.GetInstanceID();
                string url = ResourceManager.instance.SaveMaterial(material, isParticleMat, isAnimationMat);
                var assetIndex = ResourceManager.instance.AddAssetUrl(url);

                var matItem = new MyJson_Tree();
                matItem.SetInt("asset", assetIndex);
                materialsItem.Add(matItem);
            }
        }

        public static void SetAnimation(this MyJson_Object jsonNode, GameObject obj, UnityEngine.AnimationClip[] animationClips)
        {
            var exportAnimations = new MyJson_Array();
            jsonNode["_animations"] = exportAnimations;

            foreach (var animationClip in animationClips)
            {
                var gltfHash = animationClip.GetInstanceID();
                var url = UnityEditor.AssetDatabase.GetAssetPath(animationClip);
                url = url.Substring(0, url.LastIndexOf(".")) + "_" + animationClip.name + ".ani.bin";
                url = PathHelper.CheckFileName(url);
                //
                var assetIndex = ResourceManager.instance.AddAssetUrl(url);
                if (!ResourceManager.instance.HaveCache(gltfHash))
                {
                    var glTFWriter = new AnimationXWriter(obj.transform, animationClip);
                    ResourceManager.instance.AddFileBuffer(url, glTFWriter.WriteGLTF());
                    //
                    ResourceManager.instance.SaveCache(gltfHash, url);
                }
                var aniItem = new MyJson_Tree();
                aniItem.SetInt("asset", assetIndex);
                exportAnimations.Add(aniItem);
            }
        }
    }

    public static class MyJosnArrayExtension
    {
        public static void AddBool(this MyJson_Array array, bool value)
        {
            array.Add(new MyJson_Number(value));
        }
        public static void AddInt(this MyJson_Array array, int value)
        {
            array.Add(new MyJson_Number(value));
        }
        public static void AddNumber(this MyJson_Array array, float value, int? digits = null)
        {
            array.Add(new MyJson_Number(Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits)));
        }
        public static void AddNumber(this MyJson_Array array, double value, int? digits = null)
        {
            array.Add(new MyJson_Number(Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits)));
        }
        public static void AddHashCode(this MyJson_Array array, UnityEngine.Object value)
        {
            array.Add(new MyJson_HashCode(value.GetInstanceID()));
        }
        public static void AddString(this MyJson_Array array, string value)
        {
            array.Add(new MyJson_String(value));
        }
    }
}
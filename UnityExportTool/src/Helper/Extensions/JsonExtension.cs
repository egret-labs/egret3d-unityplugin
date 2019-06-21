using System;
using Egret3DExportTools;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace Egret3DExportTools
{
    public static class JsonExtension
    {
        public static void SetBool(this JContainer jsonNode, string key, bool value, bool? defalutValue = null)
        {
            jsonNode.Add(new JProperty(key, value));
        }
        public static void SetInt(this JContainer jsonNode, string key, int value, int? defalutValue = null)
        {
            if (value != defalutValue)
            {
                jsonNode.Add(new JProperty(key, value));
            }
        }
        public static void SetNumber(this JContainer jsonNode, string key, float value, float? defalutValue = null, int? digits = null)
        {
            if (value != defalutValue)
            {
                jsonNode.Add(new JProperty(key, Math.Round(value, digits ?? ExportSetting.instance.common.numberDigits)));
            }
        }
        public static void SetNumber(this JContainer jsonNode, string key, double value, float? defalutValue = null, int? digits = null)
        {
            if (value != defalutValue)
            {
                jsonNode.Add(new JProperty(key, Math.Round(value, digits ?? ExportSetting.instance.common.numberDigits)));
            }
        }
        public static void SetString(this JContainer jsonNode, string key, string value)
        {
            jsonNode.Add(new JProperty(key, value));
        }
        public static void SetEnum(this JContainer jsonNode, string key, Enum value, bool toString = false)
        {
            if (toString)
            {
                jsonNode.Add(new JProperty(key, value.ToString()));
            }
            else
            {
                jsonNode.Add(new JProperty(key, (int)System.Enum.Parse(value.GetType(), value.ToString())));
            }
        }

        //-------------------------------扩展部分--------------------------------

        public static void AddInt(this JArray array, int value)
        {
            array.Add(value);
        }
        public static void AddNumber(this JArray array, float value, int? digits = null)
        {
            array.Add(Math.Round(value, digits ?? ExportSetting.instance.common.numberDigits));
        }
        public static void AddNumber(this JArray array, double value, int? digits = null)
        {
            array.Add(Math.Round(value, digits ?? ExportSetting.instance.common.numberDigits));
        }

        public static void AddAsset(this JArray jsonNode, int assetIndex)
        {
            jsonNode.Add(new JObject(new JProperty(SerizileData.KEY_ASSET, assetIndex)));
        }

        //-------------------------------扩展部分--------------------------------
        /**
         * 组件的公共部分，Vector2
         */
        public static void SetVector2(this JContainer jsonNode, string desc, Vector2 data, Vector2? defalutValue = null, int? digits = null)
        {
            if (data != defalutValue)
            {
                JArray arr = new JArray();
                arr.AddNumber(data.x, digits);
                arr.AddNumber(data.y, digits);
                jsonNode.Add(new JProperty(desc, arr));
            }
        }
        /**
         * 组件的公共部分，Vector3
         */
        public static void SetVector3(this JContainer jsonNode, string desc, Vector3 data, Vector3? defalutValue = null, int? digits = null)
        {
            if (data != defalutValue)
            {
                JArray arr = new JArray();
                arr.AddNumber(data.x, digits);
                arr.AddNumber(data.y, digits);
                arr.AddNumber(data.z, digits);
                jsonNode.Add(new JProperty(desc, arr));
            }
        }
        /**
         * 组件的公共部分 Vector4
         */
        public static void SetVector4(this JContainer jsonNode, string desc, Vector4 data, Vector4? defalutValue = null, int? digits = null)
        {
            if (data != defalutValue)
            {
                JArray arr = new JArray();
                arr.AddNumber(data.x, digits);
                arr.AddNumber(data.y, digits);
                arr.AddNumber(data.z, digits);
                arr.AddNumber(data.w, digits);
                jsonNode.Add(new JProperty(desc, arr));
            }
        }
        /**
         * 组件的公共部分 Quaternion
         */
        public static void SetQuaternion(this JContainer jsonNode, string desc, Quaternion data, Quaternion? defalutValue = null, int? digits = null)
        {
            if (data != defalutValue)
            {
                JArray arr = new JArray();
                arr.AddNumber(data.x, digits);
                arr.AddNumber(data.y, digits);
                arr.AddNumber(data.z, digits);
                arr.AddNumber(data.w, digits);
                jsonNode.Add(new JProperty(desc, arr));
            }
        }
        /**
         * 组件的公共部分 Color
         */
        public static void SetColor(this JContainer jsonNode, string desc, Color value, Color? defalutValue = null, int? digits = null)
        {
            if (value != defalutValue)
            {
                JArray arr = new JArray();
                arr.AddNumber(value.r, digits);
                arr.AddNumber(value.g, digits);
                arr.AddNumber(value.b, digits);
                arr.AddNumber(value.a, digits);
                jsonNode.Add(new JProperty(desc, arr));
            }
        }
        public static void SetColor3(this JContainer jsonNode, string desc, Color value, Color? defalutValue = null, int? digits = null)
        {
            if (value != defalutValue)
            {
                JArray arr = new JArray();
                arr.AddNumber(value.r, digits);
                arr.AddNumber(value.g, digits);
                arr.AddNumber(value.b, digits);
                jsonNode.Add(new JProperty(desc, arr));
            }
        }
        /**
         * 组件的公共部分 Rect
         */
        public static void SetRect(this JContainer jsonNode, string desc, Rect data, int? digits = null)
        {
            JArray arr = new JArray();
            arr.AddNumber(data.x, digits);
            arr.AddNumber(data.y, digits);
            arr.AddNumber(data.width, digits);
            arr.AddNumber(data.height, digits);
            jsonNode.Add(new JProperty(desc, arr));
        }
        public static void SetUVTransform(this JContainer jsonNode, string desc, Vector4 data, int? digits = null)
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

            JArray arr = new JArray();
            arr.AddNumber(sx * c);
            arr.AddNumber(sx * s);
            arr.AddNumber(-sx * (c * cx + s * cy) + cx + tx);
            arr.AddNumber(-sy * s);
            arr.AddNumber(sy * c);
            arr.AddNumber(-sy * (-s * cx + c * cy) + cy + ty);
            arr.AddNumber(0.0);
            arr.AddNumber(0.0);
            arr.AddNumber(1.0);
            jsonNode.Add(new JProperty(desc, arr));
        }

        public static void SetReference(this JContainer jsonNode, string key, string uuid)
        {
            jsonNode.Add(new JProperty(key, new JObject(new JProperty(SerizileData.KEY_UUID, uuid))));
        }

        public static void SetAsset(this JContainer jsonNode, string key, int assetIndex)
        {
            jsonNode.Add(new JProperty(key, new JObject(new JProperty(SerizileData.KEY_ASSET, assetIndex))));
        }

        public static void SetTexture(this JContainer jsonNode, string key, Texture value, Texture defalutValue = null)
        {
            if (value != defalutValue)
            {
                var assetData = SerializeObject.SerializeAsset(value);
                var uri = ExportSetting.instance.GetExportPath(assetData.uri);
                jsonNode.Add(new JProperty(key, uri));
            }
        }

        public static void SetCubemap(this JContainer jsonNode, string key, Cubemap value, Cubemap defalutValue = null)
        {
            if (value != defalutValue)
            {
                var assetData = SerializeObject.SerializeAsset(value);
                var uri = ExportSetting.instance.GetExportPath(assetData.uri);
                jsonNode.Add(new JProperty(key, uri));
            }
        }

        public static void SetTextureArray(this JContainer jsonNode, string key, Texture2DArrayData value)
        {
            var assetData = SerializeObject.SerializeAsset(value);
            var uri = ExportSetting.instance.GetExportPath(assetData.uri);
            jsonNode.Add(new JProperty(key, uri));
        }

        public static void SetMesh(this JContainer jsonNode, UnityEngine.GameObject obj, UnityEngine.Mesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            var asset = SerializeObject.SerializeAsset(mesh);
            var assetIndex = SerializeObject.currentData.AddAsset(asset);
            jsonNode.SetAsset("mesh", assetIndex);
        }

        public static void SetMaterials(this JObject jsonNode, UnityEngine.GameObject obj, UnityEngine.Material[] materials, bool isParticleMat = false, bool isAnimationMat = false)
        {
            var materialsItem = new JArray();
            jsonNode.Add("materials", materialsItem);
            //写材质
            foreach (var material in materials)
            {
                if (material == null)
                {
                    UnityEngine.Debug.LogWarning(obj.gameObject.name + " 材质缺失，请检查资源");
                    continue;
                }

                var asset = SerializeObject.SerializeAsset(material);
                var assetIndex = SerializeObject.currentData.AddAsset(asset);
                materialsItem.AddAsset(assetIndex);
            }
        }

        public static void SetAnimation(this JObject jsonNode, UnityEngine.GameObject obj, UnityEngine.AnimationClip[] animationClips)
        {
            var exportAnimations = new JArray();
            jsonNode.Add("_animations", exportAnimations);
            foreach (var animationClip in animationClips)
            {
                var asset = SerializeObject.SerializeAsset(animationClip);
                var assetIndex = SerializeObject.currentData.AddAsset(asset);
                exportAnimations.AddAsset(assetIndex);
            }
        }

        public static void SetLightmaps(this JObject jsonNode, string exportPath)
        {
            var lightmapsJson = new JArray();
            jsonNode.Add("lightmaps", lightmapsJson);
            foreach (var lightmapData in LightmapSettings.lightmaps)
            {
                Texture2D lightmap = lightmapData.lightmapColor;
                var asset = SerializeObject.SerializeAsset(lightmap);
                var assetIndex = SerializeObject.currentData.AddAsset(asset);
                lightmapsJson.AddAsset(assetIndex);
            }
        }
    }
}
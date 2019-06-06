using System;
using Egret3DExportTools;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace Egret3DExportTools
{
    public static class JsonExtension
    {
        public static void SetBool(this JContainer jsonNode, string key, bool value)
        {
            jsonNode.Add(new JProperty(key, value));
        }
        public static void SetInt(this JContainer jsonNode, string key, int value)
        {
            jsonNode.Add(new JProperty(key, value));
        }
        public static void SetNumber(this JContainer jsonNode, string key, float value, float? defalutValue = null, int? digits = null)
        {
            if (value != defalutValue)
            {
                jsonNode.Add(new JProperty(key, Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits)));
            }
        }
        public static void SetNumber(this JContainer jsonNode, string key, double value, float? defalutValue = null, int? digits = null)
        {
            if (value != defalutValue)
            {
                jsonNode.Add(new JProperty(key, Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits)));
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
            array.Add(Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits));
        }
        public static void AddNumber(this JArray array, double value, int? digits = null)
        {
            array.Add(Math.Round(value, digits ?? ExportToolsSetting.instance.floatRoundDigits));
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

        public static void SetTexture(this JContainer jsonNode, string key, Texture value, Texture defalutValue = null)
        {
            if (value != defalutValue)
            {
                var path = PathHelper.GetTextureDescPath(value);
                var assetData = SerializeObject.SerializeAsset(value, path, AssetType.Texture);

                var uri = ExportConfig.instance.GetExportPath(assetData.uri);
                jsonNode.Add(new JProperty(key, uri));

            }
        }

        public static void SetAsset(this JContainer jsonNode, string key, int assetIndex)
        {
            jsonNode.Add(new JProperty(key, new JObject(new JProperty(SerizileData.KEY_ASSET, assetIndex))));
        }
    }
}
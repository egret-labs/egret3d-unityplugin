namespace Egret3DExportTools
{
    public static class MaterialExtension
    {
        public static float GetFloat(this UnityEngine.Material source, string key, float defalutValue)
        {
            if (source.HasProperty(key))
            {
                return source.GetFloat(key);
            }

            return defalutValue;
        }

        public static UnityEngine.Color GetColor(this UnityEngine.Material source, string key, UnityEngine.Color defalutValue)
        {
            if (source.HasProperty(key))
            {
                return source.GetColor(key);
            }

            return defalutValue;
        }

        public static UnityEngine.Vector4 GetVector4(this UnityEngine.Material source, string key, UnityEngine.Vector4 defalutValue)
        {
            if (source.HasProperty(key))
            {
                return source.GetVector(key);
            }

            return defalutValue;
        }

        public static UnityEngine.Texture GetTexture(this UnityEngine.Material source, string key, UnityEngine.Texture defalutValue)
        {
            if (source.HasProperty(key))
            {
                return source.GetTexture(key);
            }

            return defalutValue;
        }
    }
}
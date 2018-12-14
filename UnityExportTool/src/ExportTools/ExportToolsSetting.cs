using UnityEngine;
namespace Egret3DExportTools
{
    [System.Serializable]
    public class ExportConfig
    {
        public string exportPath;
        public string[] customShaders;

        private static ExportConfig _instance;
        public static ExportConfig instance
        {
            get { return _instance; }
        }
        public static void Reload(string configPath, string defaultExportPath)
        {
            if (System.IO.File.Exists(configPath))
            {
                var jsonStr = System.IO.File.ReadAllText(configPath, System.Text.Encoding.UTF8);
                _instance = JsonUtility.FromJson<ExportConfig>(jsonStr);
            }
            else
            {
                _instance = new ExportConfig();
            }

            if (!System.IO.Directory.Exists(_instance.exportPath))
            {
                _instance.exportPath = defaultExportPath;
            }

            MyLog.Log("exportPath:" + _instance.exportPath);

            if (_instance.customShaders != null)
            {
                foreach (var customShader in _instance.customShaders)
                {
                    MyLog.Log("customShader:" + customShader);
                }
            }
        }
        public void Save(string configPath)
        {
            System.IO.File.WriteAllText(configPath, JsonUtility.ToJson(this));
        }

        public bool IsCustomShader(string shaderName)
        {
            if (this.customShaders != null)
            {
                foreach (var customShader in this.customShaders)
                {
                    if (shaderName == customShader)
                    {
                        return true;
                    }
                }
            }


            return false;
        }
    }

    public enum ExportLightType
    {
        None,
        Lambert,
        Phong,
    }

    //TODO 放到config中
    public static class ExportToolsSetting
    {
        /**
        *是否打印Log信息
         */
        public static bool debugLog = false;
        /**
        *是否可以导出未激活的对象
        */
        public static bool exportUnactivatedObject = false;
        /**
        *是否可以导出未激活的组件
        */
        public static bool exportUnactivatedComp = false;
        /**
        *预制体坐标是否归零
        */
        public static bool prefabResetPos = false;
        /**
        *gltf导出格式是否缩进
        */
        public static bool reduceGltfJsonSize = true;
        /**
        *估算一个合理的最大粒子数
        */
        public static bool estimateMaxParticles = true;
        public static bool unityNormalTexture = false;
        /**
        *小数点保留位数
        */
        public static int floatRoundDigits = 6;

        public static bool exportOriginalImage = false;

        public static float jpegQuality = 75.0f;

        public static ExportLightType lightType = ExportLightType.None;

        public static bool enableUV2s = true;
        public static bool enableNormals = true;
        public static bool enableColors = true;
        public static bool enableBones = true;
        public static bool enableTangents = false;
    }
}
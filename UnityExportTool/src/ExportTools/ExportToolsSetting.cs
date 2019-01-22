using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace Egret3DExportTools
{
    [JsonObject(MemberSerialization.OptOut)]
    public class CustomShaderConfig {
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string[] include; // TODO
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public string technique;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int[] enable;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int[] frontFace;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int[] cullFace;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int[] blendEquationSeparate;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int[] blendFuncSeparate;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int[] depthFunc;
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public int[] depthMask;
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class ExportConfig
    {
        public string exportPath = "";
        public Dictionary<string, CustomShaderConfig> customShaders = new Dictionary<string, CustomShaderConfig>();

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
                _instance = JsonConvert.DeserializeObject<ExportConfig>(jsonStr);
            }
            else
            {
                _instance = new ExportConfig();
            }

            if (!System.IO.Directory.Exists(_instance.exportPath))
            {
                _instance.exportPath = defaultExportPath;
            }

            if (_instance.customShaders != null)
            {
                foreach (var customShader in _instance.customShaders)
                {
                    MyLog.Log("customShader:" + customShader.Key);
                }
            }
        }
        public void Save(string configPath)
        {
            System.IO.File.WriteAllText(configPath, JsonConvert.SerializeObject(this));
        }

        public CustomShaderConfig IsCustomShader(string shaderName)
        {
            if (this.customShaders != null)
            {
                foreach (var customShader in this.customShaders)
                {
                    if (shaderName == customShader.Key)
                    {
                        return customShader.Value;
                    }
                }
            }

            return null;
        }
    }

    public enum ExportLightType
    {
        None,
        Lambert,
        Phong,
        Standard,
    }

    //TODO 放到config中
    public class ExportToolsSetting : UnityEngine.ScriptableObject
    {
        private static ExportToolsSetting _instance;
        public static ExportToolsSetting instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExportToolsSetting();
                }
                return _instance;
            }
        }
        /**
        *是否打印Log信息
         */
        public bool debugLog = false;
        /**
        *是否可以导出未激活的对象
        */
        public bool exportUnactivatedObject = false;
        /**
        *是否可以导出未激活的组件
        */
        public bool exportUnactivatedComp = false;
        /**
        *预制体坐标是否归零
        */
        public bool prefabResetPos = false;
        /**
        *gltf导出格式是否缩进
        */
        public bool reduceGltfJsonSize = true;
        /**
        *估算一个合理的最大粒子数
        */
        public bool estimateMaxParticles = true;
        public bool unityNormalTexture = false;
        /**
        *小数点保留位数
        */
        public int floatRoundDigits = 6;

        public bool exportOriginalImage = false;

        public float jpegQuality = 75.0f;

        public ExportLightType lightType = ExportLightType.None;

        public bool enableUV2s = true;
        public bool enableNormals = true;
        public bool enableColors = true;
        public bool enableBones = true;
        public bool enableTangents = false;

        [SerializeField]
        public List<UnityEngine.GameObject> meshIgnores = new List<UnityEngine.GameObject>();


        public bool IsInMeshIgnores(UnityEngine.GameObject target)
        {
            return this.meshIgnores.Contains(target);
        }
    }
}
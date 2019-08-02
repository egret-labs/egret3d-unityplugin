using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace Egret3DExportTools
{
    public enum ExportLightType
    {
        None,
        Lambert,
        Phong,
        Standard,
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class CommonSetting
    {
        /**
        *控制台输出Log信息
        */
        public bool debugLog = false;
        /**
        *格式化json
        */
        public bool jsonFormatting = true;
        /**
        *预制体坐标是否归零
        */
        public bool posToZero = false;
        /**
        *是否可以导出未激活的对象
        */
        public bool exportUnactivatedObject = false;
        /**
        *是否可以导出未激活的组件
        */
        public bool exportUnactivatedComp = false;
        /**
        *小数点保留位数
        */
        public int numberDigits = 6;
    }
    [JsonObject(MemberSerialization.OptOut)]
    public class LightSetting
    {   
        /**
        *光照类型
        */
        public ExportLightType type = ExportLightType.None;
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class SceneSetting
    {
        /**
        *导出光照贴图
        */
        public bool lightmap = true;
        /**
        *开启场景静态合并
        */
        public bool staticBatching = true;
        /**
        *场景雾
        */
        public bool fog = true;
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class TextureSetting
    {
        public bool useOriginalTexture = false;
        public bool useNormalTexture = false;
        public int jpgQuality = 75;
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class MeshSetting
    {
        public bool uv2 = true;
        public bool normal = true;
        public bool color = true;
        public bool bone = true;
        public bool tangent = false;
    }


    [JsonObject(MemberSerialization.OptOut)]
    public class ShaderSetting
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] include; // TODO
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string technique;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] enable;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] frontFace;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] cullFace;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] blendEquationSeparate;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] blendFuncSeparate;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] depthFunc;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int[] depthMask;
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class ExportSetting
    {
        private static ExportSetting _instance;
        public static ExportSetting instance
        {
            get { return _instance; }
        }
        public static void Reload(string configPath, string defaultExportPath)
        {
            if (System.IO.File.Exists(configPath))
            {
                var jsonStr = System.IO.File.ReadAllText(configPath, System.Text.Encoding.UTF8);
                _instance = JsonConvert.DeserializeObject<ExportSetting>(jsonStr);
            }
            else
            {
                _instance = new ExportSetting();
            }

            if (!System.IO.Directory.Exists(_instance.exportDir))
            {
                _instance.exportDir = defaultExportPath;
            }

            if (_instance.shader != null)
            {
                foreach (var customShader in _instance.shader)
                {
                    MyLog.Log("customShader:" + customShader.Key);
                }
            }
        }

        public string exportDir = "";
        public string rootName = "Assets";
        public CommonSetting common = new CommonSetting();
        public LightSetting light = new LightSetting();

        public SceneSetting scene = new SceneSetting();
        public TextureSetting texture = new TextureSetting();
        public MeshSetting mesh = new MeshSetting();
        public Dictionary<string, ShaderSetting> shader = new Dictionary<string, ShaderSetting>();

        public void Save(string configPath)
        {
            System.IO.File.WriteAllText(configPath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public ShaderSetting GetCustomShader(string shaderName)
        {
            if (this.shader != null)
            {
                foreach (var customShader in this.shader)
                {
                    if (shaderName == customShader.Key)
                    {
                        return customShader.Value;
                    }
                }
            }

            return null;
        }

        public string GetExportPath(string path)
        {
            return path.Replace("Assets", ExportSetting.instance.rootName);
        }
    }

}
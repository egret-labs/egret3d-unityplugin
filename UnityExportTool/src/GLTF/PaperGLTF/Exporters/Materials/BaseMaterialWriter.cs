namespace PaperGLTF
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Egret3DExportTools;

    public enum MaterialType
    {
        Diffuse,
        Lambert,
        Phong,
        Standard,
        StandardSpecular,
        StandardRoughness,
        Particle,
        Custom
    }

    public enum BlendMode
    {
        None,
        Blend,
        Blend_PreMultiply,
        Add,
        Add_PreMultiply,
        Subtractive,
        Subtractive_PreMultiply,
        Multiply,
        Multiply_PreMultiply,
    }

    public enum EnableState
    {
        BLEND = 3042,
        CULL_FACE = 2884,
        DEPTH_TEST = 2929,
        POLYGON_OFFSET_FILL = 32823,
        SAMPLE_ALPHA_TO_COVERAGE = 32926,
    }

    public enum BlendEquation
    {
        FUNC_ADD = 32774,
        FUNC_SUBTRACT = 32778,
        FUNC_REVERSE_SUBTRACT = 32779,
    }

    public enum BlendFactor
    {
        ZERO = 0,
        ONE = 1,
        SRC_COLOR = 768,
        ONE_MINUS_SRC_COLOR = 769,
        DST_COLOR = 774,
        ONE_MINUS_DST_COLOR = 775,
        SRC_ALPHA = 770,
        ONE_MINUS_SRC_ALPHA = 771,
        DST_ALPHA = 772,
        ONE_MINUS_DST_ALPHA = 773,
        CONSTANT_COLOR = 32769,
        ONE_MINUS_CONSTANT_COLOR = 32770,
        CONSTANT_ALPHA = 32771,
        ONE_MINUS_CONSTANT_ALPHA = 32772,
        SRC_ALPHA_SATURATE = 776,
    }

    public enum CullFace
    {
        FRONT = 1028,
        BACK = 1029,
        FRONT_AND_BACK = 1032,
    }

    public enum FrontFace
    {
        CW = 2304,
        CCW = 2305,
    }

    public enum DepthFunc
    {
        NEVER = 512,
        LESS = 513,
        LEQUAL = 515,
        EQUAL = 514,
        GREATER = 516,
        NOTEQUAL = 517,
        GEQUAL = 518,
        ALWAYS = 519,
    }

    public enum RenderQueue
    {
        Background = 1000,
        Geometry = 2000,
        AlphaTest = 2450,
        Transparent = 3000,
        Overlay = 4000,
    }
    public abstract class BaseMaterialWriter
    {
        public Material source;

        protected Dictionary<string, IJsonNode> values = new Dictionary<string, IJsonNode>();
        protected List<string> defines = new List<string>();

        public virtual void Clean()
        {
            this.source = null;

            this.values.Clear();
            this.defines.Clear();
        }

        public MyJson_Tree Write()
        {
            this.Update();
            var materialItemJson = new MyJson_Tree();
            var extensions = new MyJson_Tree();
            materialItemJson.Add("extensions", extensions);

            var KHR_techniques_webglJson = new MyJson_Tree();
            extensions.Add("KHR_techniques_webgl", KHR_techniques_webglJson);

            var paperJson = new MyJson_Tree();
            extensions.Add("paper", paperJson);

            var valuesJson = new MyJson_Tree();
            KHR_techniques_webglJson.Add("values", valuesJson);

            //
            var source = this.source;
            var shaderName = source.shader.name.ToLower();
            MyLog.Log("Shader:" + shaderName);
            foreach (var value in this.values)
            {
                valuesJson.Add(value.Key, value.Value);
            }
            KHR_techniques_webglJson.SetString("technique", this.technique);
            //paper
            paperJson.SetInt("renderQueue", this.source.renderQueue);

            if (this.defines.Count > 0)
            {
                var definesJson = new MyJson_Array();
                foreach (var define in this.defines)
                {
                    definesJson.AddString(define);
                }
                paperJson.Add("defines", definesJson);
            }

            //
            //standard
            var isDoubleSide = this.isDoubleSide;
            var isTransparent = this.isTransparent;
            var blend = this.blendMode;
            var statesJson = new MyJson_Tree();
            var enable = new MyJson_Array();

            if (isDoubleSide || blend != BlendMode.None || isTransparent)
            {
                //states
                paperJson.Add("states", statesJson);
                var functionsJson = new MyJson_Tree();
                statesJson.Add("enable", enable);
                statesJson.Add("functions", functionsJson);
                this.SetBlend(enable, functionsJson, blend);
                this.SetCullFace(enable, functionsJson, !isDoubleSide);
                this.SetDepth(enable, functionsJson, true, !isTransparent);
            }

            Debug.Log("===============================================" + this.shaderName);
            //
            var customConfig = ExportConfig.instance.IsCustomShader(this.shaderName);
            if (customConfig != null)
            {
                if (customConfig.enable != null)
                {
                    if (customConfig.enable.Length == 0)
                    {
                        statesJson.Remove("enable");
                    }
                    else
                    {
                        enable.Clear();
                        foreach (var value in customConfig.enable)
                        {
                            enable.AddInt(value);
                        }

                        statesJson.Add("enable", enable);
                    }
                }

                if (!String.IsNullOrEmpty(customConfig.technique))
                {
                    KHR_techniques_webglJson.SetString("technique", customConfig.technique);
                }
            }

            return materialItemJson;
        }

        protected virtual void Update()
        {

        }

        protected void SetBlend(MyJson_Array enalbesJson, MyJson_Tree functionsJson, BlendMode blend)
        {
            if (blend == BlendMode.None)
            {
                return;
            }
            enalbesJson.AddInt((int)EnableState.BLEND);

            var blendEquationSeparate = new MyJson_Array();
            blendEquationSeparate.AddInt((int)BlendEquation.FUNC_ADD);
            blendEquationSeparate.AddInt((int)BlendEquation.FUNC_ADD);
            var blendFuncSeparate = new MyJson_Array();
            switch (blend)
            {
                case BlendMode.Add:
                    blendFuncSeparate.AddInt((int)BlendFactor.SRC_ALPHA);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    blendFuncSeparate.AddInt((int)BlendFactor.SRC_ALPHA);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    break;
                case BlendMode.Blend:
                    blendFuncSeparate.AddInt((int)BlendFactor.SRC_ALPHA);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE_MINUS_SRC_ALPHA);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE_MINUS_SRC_ALPHA);
                    break;
                case BlendMode.Add_PreMultiply:
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    break;
                case BlendMode.Blend_PreMultiply:
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE_MINUS_CONSTANT_ALPHA);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE);
                    blendFuncSeparate.AddInt((int)BlendFactor.ONE_MINUS_CONSTANT_ALPHA);
                    break;
                case BlendMode.Multiply:
                    blendFuncSeparate.AddInt((int)BlendFactor.ZERO);
                    blendFuncSeparate.AddInt((int)BlendFactor.SRC_COLOR);
                    blendFuncSeparate.AddInt((int)BlendFactor.ZERO);
                    blendFuncSeparate.AddInt((int)BlendFactor.SRC_COLOR);
                    break;
                case BlendMode.Multiply_PreMultiply:
                    blendFuncSeparate.AddInt((int)BlendFactor.ZERO);
                    blendFuncSeparate.AddInt((int)BlendFactor.SRC_COLOR);
                    blendFuncSeparate.AddInt((int)BlendFactor.ZERO);
                    blendFuncSeparate.AddInt((int)BlendFactor.SRC_ALPHA);
                    break;
            }

            functionsJson.Add("blendEquationSeparate", blendEquationSeparate);
            functionsJson.Add("blendFuncSeparate", blendFuncSeparate);

            //
            var customConfig = ExportConfig.instance.IsCustomShader(this.shaderName);
            if (customConfig != null)
            {
                if (customConfig.blendFuncSeparate != null)
                {
                    if (customConfig.blendFuncSeparate.Length == 0)
                    {
                        functionsJson.Remove("blendEquationSeparate");
                    }
                    else
                    {
                        blendFuncSeparate.Clear();
                        foreach (var value in customConfig.blendFuncSeparate)
                        {
                            blendFuncSeparate.AddInt(value);
                        }
                        functionsJson.Add("blendEquationSeparate", blendEquationSeparate);
                    }
                }

                if (customConfig.blendFuncSeparate != null)
                {
                    if (customConfig.blendFuncSeparate.Length == 0)
                    {
                        functionsJson.Remove("blendFuncSeparate");
                    }
                    else
                    {
                        blendFuncSeparate.Clear();
                        foreach (var value in customConfig.blendFuncSeparate)
                        {
                            blendFuncSeparate.AddInt(value);
                        }
                        functionsJson.Add("blendFuncSeparate", blendFuncSeparate);
                    }
                }
            }
        }

        protected void SetCullFace(MyJson_Array enalbesJson, MyJson_Tree functionsJson, bool cull)
        {
            var frontFace = new MyJson_Array();
            var cullFace = new MyJson_Array();

            if (cull)
            {
                frontFace.AddInt((int)FrontFace.CCW);
                functionsJson.Add("frontFace", frontFace);

                cullFace.AddInt((int)CullFace.BACK);
                functionsJson.Add("cullFace", cullFace);

                enalbesJson.AddInt((int)EnableState.CULL_FACE);
            }

            //
            var customConfig = ExportConfig.instance.IsCustomShader(this.shaderName);
            if (customConfig != null)
            {
                if (customConfig.frontFace != null)
                {
                    if (customConfig.frontFace.Length == 0)
                    {
                        functionsJson.Remove("frontFace");
                    }
                    else
                    {
                        frontFace.Clear();
                        foreach (var value in customConfig.frontFace)
                        {
                            frontFace.AddInt(value);
                        }

                        functionsJson.Add("frontFace", frontFace);
                    }
                }

                if (customConfig.cullFace != null)
                {
                    if (customConfig.cullFace.Length == 0)
                    {
                        functionsJson.Remove("cullFace");
                    }
                    else
                    {
                        cullFace.Clear();
                        foreach (var value in customConfig.cullFace)
                        {
                            cullFace.AddInt(value);
                        }

                        functionsJson.Add("cullFace", cullFace);
                    }
                }
            }
        }

        protected void SetDepth(MyJson_Array enalbesJson, MyJson_Tree functionsJson, bool zTest, bool zWrite)
        {
            var depthFunc = new MyJson_Array();
            var depthMask = new MyJson_Array();

            if (zTest)
            {
                depthFunc.AddInt((int)DepthFunc.LEQUAL);
                functionsJson.Add("depthFunc", depthFunc);
                enalbesJson.AddInt((int)EnableState.DEPTH_TEST);
            }

            if (zWrite)
            {
                depthMask.AddBool(true);
                functionsJson.Add("depthMask", depthMask);
            }

            //
            var customConfig = ExportConfig.instance.IsCustomShader(this.shaderName);
            if (customConfig != null)
            {
                if (customConfig.depthFunc != null)
                {
                    if (customConfig.depthFunc.Length == 0)
                    {
                        functionsJson.Remove("depthFunc");
                    }
                    else
                    {
                        depthFunc.Clear();
                        foreach (var value in customConfig.depthFunc)
                        {
                            depthFunc.AddInt(value);
                        }

                        functionsJson.Add("depthFunc", depthFunc);
                    }
                }

                if (customConfig.depthMask != null)
                {
                    if (customConfig.depthMask.Length == 0)
                    {
                        functionsJson.Remove("depthMask");
                    }
                    else
                    {
                        depthMask.Clear();
                        foreach (var value in customConfig.depthMask)
                        {
                            depthMask.AddInt(value);
                        }

                        functionsJson.Add("depthMask", depthMask);
                    }
                }
            }
        }

        protected void SetFloat(string key, float value, float defalutValue = 0.0f)
        {
            if (value != defalutValue)
            {
                this.values.SetNumber(key, value);
            }
        }

        protected void SetColor3(string key, Color value, Color defalutValue)
        {
            if (value != defalutValue)
            {
                this.values.SetColor3(key, value);
            }
        }

        protected void SetColor3AndOpacity(Color value, Color defalutValue)
        {
            if (value != defalutValue)
            {
                this.values.SetColor3("diffuse", value);
                this.values.SetNumber("opacity", value.a);
            }
        }

        protected void SetVector2(string key, Vector2 value, Vector2 defalutValue)
        {
            if (value != defalutValue)
            {
                this.values.SetVector2(key, value);
            }
        }

        protected void SetVector4(string key, Vector4 value, Vector4 defalutValue)
        {
            if (value != defalutValue)
            {
                this.values.SetVector4(key, value);
            }
        }

        protected void SetTexture(string key, Texture value, Texture defalutValue = null)
        {
            if (value != defalutValue)
            {
                var texPath = ResourceManager.instance.SaveTexture(value as Texture2D, "");
                this.values.SetString(key, texPath);
            }
        }


        protected float GetFloat(string key, float defalutValue)
        {
            if (this.source.HasProperty(key))
            {
                return this.source.GetFloat(key);
            }

            return defalutValue;
        }

        protected Color GetColor(string key, Color defalutValue)
        {
            if (this.source.HasProperty(key))
            {
                return this.source.GetColor(key);
            }

            return defalutValue;
        }

        protected Vector4 GetVector4(string key, Vector4 defalutValue)
        {
            if (this.source.HasProperty(key))
            {
                return this.source.GetVector(key);
            }

            return defalutValue;
        }

        protected Texture GetTexture(string key, Texture defalutValue)
        {
            if (this.source.HasProperty(key))
            {
                return this.source.GetTexture(key);
            }

            return defalutValue;
        }

        protected bool isDoubleSide
        {
            get
            {
                var isDoubleSide = source.HasProperty("_Cull") && source.GetInt("_Cull") == (float)UnityEngine.Rendering.CullMode.Off;
                if (!isDoubleSide)
                {
                    //others
                    var shaderName = this.shaderName;
                    isDoubleSide = shaderName.Contains("both") || shaderName.Contains("side");
                }
                return isDoubleSide;
            }
        }

        protected bool isTransparent
        {
            get
            {
                if (source.GetTag("RenderType", false, "") == "Transparent")
                {
                    return true;
                }
                return false;
            }
        }

        protected BlendMode blendMode
        {
            get
            {
                var blend = BlendMode.None;
                var shaderName = this.shaderName;
                if (source.GetTag("RenderType", false, "") == "Transparent")
                {
                    var additive = shaderName.Contains("additive");
                    var multiply = shaderName.Contains("multiply");
                    var premultiply = shaderName.Contains("premultiply");
                    if (additive)
                    {
                        blend = premultiply ? BlendMode.Add_PreMultiply : BlendMode.Add;
                    }
                    else if (multiply)
                    {
                        blend = premultiply ? BlendMode.Multiply_PreMultiply : BlendMode.Multiply;
                    }
                    else
                    {
                        blend = premultiply ? BlendMode.Blend_PreMultiply : BlendMode.Blend;
                    }
                }
                return blend;
            }
        }

        protected string shaderName
        {
            get
            {
                return this.source.shader.name;
            }
        }

        protected virtual string technique
        {
            get
            {
                return "builtin/meshbasic.shader.json";
            }
        }
    }
}
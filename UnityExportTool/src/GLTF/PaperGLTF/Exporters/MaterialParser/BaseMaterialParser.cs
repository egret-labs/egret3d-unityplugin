namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

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

    public enum FunctionState
    {
        DepthFunc,
        DepthMask,
        FrontFace,
        CullFace,
        BlendEquationSeparate,
        BlendFuncSeparate,
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
    public abstract class BaseMaterialParser
    {
        public static readonly List<string> UNITY_RENDER_TYPE = new List<string> { "Opaque", "Transparent", "TransparentCutout", "Background", "Overlay" };
        protected string shaderAsset;
        protected Material source;
        protected MaterialData data;

        public void Init(string shaderAsset)
        {
            this.shaderAsset = shaderAsset;
        }
        public void Parse(Material source, MaterialData data)
        {
            this.source = source;
            this.data = data;

            //Unifrom
            this.CollectUniformValues();
            //Defines
            this.CollectDefines();
            //States
            this.CollectStates();
            //Egret
            this.data.shaderAsset = this.shaderAsset;
        }



        public virtual void CollectUniformValues()
        {
        }

        public virtual void CollectDefines()
        {
            var defines = this.data.defines;
            var cutoff = 0.0f;
            var tag = source.GetTag("RenderType", false, "");
            if (UNITY_RENDER_TYPE.Contains(tag))
            {
                if (tag == "TransparentCutout")
                {
                    cutoff = this.GetFloat("_Cutoff", 0.0f);
                }
            }
            else if (source.HasProperty("_Cutoff"))
            {
                cutoff = this.GetFloat("_Cutoff", 0.0f);
            }
            //
            if (cutoff > 0.0f)
            {
                defines.Add(new Define { name = "ALPHATEST " + cutoff.ToString("0.0####") });
                // defines.Add("ALPHATEST " + cutoff.ToString("0.0####"));
            }
        }

        public virtual void CollectStates()
        {
            var customConfig = ExportConfig.instance.getCustomShader(this.shaderName);
            var isDoubleSide = this.isDoubleSide;
            var isTransparent = this.isTransparent;

            var blend = this.blendMode;
            if (isDoubleSide || blend != BlendMode.None || isTransparent || (customConfig != null && customConfig.enable != null))
            {
                if (customConfig != null && customConfig.enable != null)
                {
                    foreach (var value in customConfig.enable)
                    {
                        this.data.enables.Add((EnableState)value);
                    }
                    //
                    if (customConfig.blendEquationSeparate != null)
                    {
                        this.SetBlendEquationSeparate(this.data.functions, customConfig.blendEquationSeparate);
                    }
                    if (customConfig.blendFuncSeparate != null)
                    {
                        this.SetBlendFuncSeparate(this.data.functions, customConfig.blendFuncSeparate);
                    }
                    if (customConfig.frontFace != null)
                    {
                        this.SetFrontFace(this.data.functions, customConfig.frontFace);
                    }
                    if (customConfig.cullFace != null)
                    {
                        this.SetCullFace(this.data.functions, customConfig.cullFace);
                    }
                    if (customConfig.depthFunc != null)
                    {
                        this.SetDepthFunc(this.data.functions, customConfig.depthFunc);
                    }
                    if (customConfig.depthMask != null)
                    {
                        this.SetDepthMask(this.data.functions, customConfig.depthMask);
                    }
                }
                else
                {
                    this.SetBlend(this.data.enables, this.data.functions, blend);
                    this.SetCull(this.data.enables, this.data.functions, !isDoubleSide);
                    this.SetDepth(this.data.enables, this.data.functions, true, !isTransparent);
                }
            }
        }

        protected void SetBlendEquationSeparate(Dictionary<string, IJsonNode> functions, int[] blendEquationSeparateValue)
        {
            var blendEquationSeparate = new MyJson_Array();
            foreach (var v in blendEquationSeparateValue)
            {
                blendEquationSeparate.AddInt((int)v);
            }
            functions.Add("blendEquationSeparate", blendEquationSeparate);
        }

        protected void SetBlendFuncSeparate(Dictionary<string, IJsonNode> functions, int[] blendFuncSeparateValue)
        {
            var blendFuncSeparate = new MyJson_Array();
            foreach (var v in blendFuncSeparateValue)
            {
                blendFuncSeparate.AddInt((int)v);
            }
            functions.Add("blendFuncSeparate", blendFuncSeparate);
        }

        protected void SetFrontFace(Dictionary<string, IJsonNode> functions, int[] frontFaceValue)
        {
            var frontFace = new MyJson_Array();
            foreach (var v in frontFaceValue)
            {
                frontFace.AddInt(v);
            }
            functions.Add("frontFace", frontFace);
        }

        protected void SetCullFace(Dictionary<string, IJsonNode> functions, int[] cullFaceValue)
        {
            var cullFace = new MyJson_Array();
            foreach (var v in cullFaceValue)
            {
                cullFace.AddInt(v);
            }
            functions.Add("cullFace", cullFace);
        }

        protected void SetDepthFunc(Dictionary<string, IJsonNode> functions, int[] depthFuncValue)
        {
            var depthFunc = new MyJson_Array();
            foreach (var v in depthFuncValue)
            {
                depthFunc.AddInt(v);
            }
            functions.Add("depthFunc", depthFunc);
        }

        protected void SetDepthMask(Dictionary<string, IJsonNode> functions, int[] depthMaskValue)
        {
            var depthMask = new MyJson_Array();
            foreach (var v in depthMaskValue)
            {
                depthMask.AddBool(v != 0);
            }
            functions.Add("depthMask", depthMask);
        }

        protected void SetBlend(List<EnableState> enables, Dictionary<string, IJsonNode> functions, BlendMode blend)
        {
            if (blend == BlendMode.None)
            {
                return;
            }
            enables.Add(EnableState.BLEND);

            var blendFuncSeparate = new int[4];
            switch (blend)
            {
                case BlendMode.Add:
                    blendFuncSeparate[0] = (int)BlendFactor.SRC_ALPHA;
                    blendFuncSeparate[1] = (int)BlendFactor.ONE;
                    blendFuncSeparate[2] = (int)BlendFactor.SRC_ALPHA;
                    blendFuncSeparate[3] = (int)BlendFactor.ONE;
                    break;
                case BlendMode.Blend:
                    blendFuncSeparate[0] = (int)BlendFactor.SRC_ALPHA;
                    blendFuncSeparate[1] = (int)BlendFactor.ONE_MINUS_SRC_ALPHA;
                    blendFuncSeparate[2] = (int)BlendFactor.ONE;
                    blendFuncSeparate[3] = (int)BlendFactor.ONE_MINUS_SRC_ALPHA;
                    break;
                case BlendMode.Add_PreMultiply:
                    blendFuncSeparate[0] = (int)BlendFactor.ONE;
                    blendFuncSeparate[1] = (int)BlendFactor.ONE;
                    blendFuncSeparate[2] = (int)BlendFactor.ONE;
                    blendFuncSeparate[3] = (int)BlendFactor.ONE;
                    break;
                case BlendMode.Blend_PreMultiply:
                    blendFuncSeparate[0] = (int)BlendFactor.ONE;
                    blendFuncSeparate[1] = (int)BlendFactor.ONE_MINUS_CONSTANT_ALPHA;
                    blendFuncSeparate[2] = (int)BlendFactor.ONE;
                    blendFuncSeparate[3] = (int)BlendFactor.ONE_MINUS_CONSTANT_ALPHA;
                    break;
                case BlendMode.Multiply:
                    blendFuncSeparate[0] = (int)BlendFactor.ZERO;
                    blendFuncSeparate[1] = (int)BlendFactor.SRC_COLOR;
                    blendFuncSeparate[2] = (int)BlendFactor.ZERO;
                    blendFuncSeparate[3] = (int)BlendFactor.SRC_COLOR;
                    break;
                case BlendMode.Multiply_PreMultiply:
                    blendFuncSeparate[0] = (int)BlendFactor.ZERO;
                    blendFuncSeparate[1] = (int)BlendFactor.SRC_COLOR;
                    blendFuncSeparate[2] = (int)BlendFactor.ZERO;
                    blendFuncSeparate[3] = (int)BlendFactor.SRC_ALPHA;
                    break;
            }

            int[] blendEquationSeparate = { (int)BlendEquation.FUNC_ADD, (int)BlendEquation.FUNC_ADD };
            this.SetBlendEquationSeparate(functions, blendEquationSeparate);
            this.SetBlendFuncSeparate(functions, blendFuncSeparate);
        }

        protected void SetCull(List<EnableState> enables, Dictionary<string, IJsonNode> functions, bool cull)
        {
            if (cull)
            {
                int[] frontFace = { (int)FrontFace.CCW };
                this.SetFrontFace(functions, frontFace);
                int[] cullFace = { (int)CullFace.BACK };
                this.SetCullFace(functions, cullFace);

                enables.Add(EnableState.CULL_FACE);
            }
        }

        protected void SetDepth(List<EnableState> enables, Dictionary<string, IJsonNode> functions, bool zTest, bool zWrite)
        {
            if (zTest && zWrite)
            {
                return;
            }

            if (zTest)
            {
                int[] depthFunc = { (int)DepthFunc.LEQUAL };
                this.SetDepthFunc(functions, depthFunc);
                enables.Add(EnableState.DEPTH_TEST);
            }

            int[] depthMask = { zWrite ? 1 : 0 };
            this.SetDepthMask(functions, depthMask);
        }

        protected void SetFloat(string key, float value, float defalutValue = 0.0f)
        {
            if (value != defalutValue)
            {
                this.data.values.SetNumber(key, value);
            }
        }

        protected void SetColor3(string key, Color value, Color defalutValue)
        {
            if (value != defalutValue)
            {
                this.data.values.SetColor3(key, value);
            }
        }

        protected void SetColor3AndOpacity(Color value, Color defalutValue)
        {
            if (value != defalutValue)
            {
                this.data.values.SetColor3("diffuse", value);
                this.data.values.SetNumber("opacity", value.a);
            }
        }

        protected void SetVector2(string key, Vector2 value, Vector2 defalutValue)
        {
            if (value != defalutValue)
            {
                this.data.values.SetVector2(key, value);
            }
        }

        protected void SetVector4(string key, Vector4 value, Vector4 defalutValue)
        {
            if (value != defalutValue)
            {
                this.data.values.SetVector4(key, value);
            }
        }

        protected void SetTexture(string key, Texture value, Texture defalutValue = null)
        {
            if (value != defalutValue)
            {
                var texPath = SerializeObject.SerializeAsset(value, AssetType.Texture);
                this.data.values.SetUri(key, texPath);
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

        protected virtual bool isDoubleSide
        {
            get
            {
                var isDoubleSide = source.HasProperty("_Cull") && source.GetInt("_Cull") == (float)UnityEngine.Rendering.CullMode.Off;
                if (!isDoubleSide)
                {
                    //others
                    var shaderName = this.shaderName.ToLower();
                    isDoubleSide = shaderName.Contains("both") || shaderName.Contains("side");
                }
                return isDoubleSide;
            }
        }

        protected virtual bool isTransparent
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

        protected virtual BlendMode blendMode
        {
            get
            {
                var blend = BlendMode.None;
                var shaderName = this.shaderName.ToLower();
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

        protected virtual string shaderName
        {
            get
            {
                return this.source.shader.name;
            }
        }
    }
}
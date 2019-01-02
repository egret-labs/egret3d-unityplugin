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
    public abstract class BaseMaterialWriter
    {
        public Material source;

        protected int renderQueue = 0;
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
            var definesJson = new MyJson_Array();
            paperJson.SetInt("renderQueue", this.source.renderQueue);
            paperJson.Add("defines", definesJson);
            extensions.Add("paper", paperJson);
            //states
            var statesJson = new MyJson_Tree();
            paperJson.Add("states", statesJson);
            var enalbesJson = new MyJson_Array();
            var functionsJson = new MyJson_Tree();
            statesJson.Add("enable", enalbesJson);
            statesJson.Add("functions", functionsJson);

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
            foreach (var define in this.defines)
            {
                definesJson.AddString(define);
            }

            //
            //standard
            var isDoubleSide = source.HasProperty("_Cull") && source.GetInt("_Cull") == (float)UnityEngine.Rendering.CullMode.Off;
            if (!isDoubleSide)
            {
                //others
                isDoubleSide = shaderName.Contains("both") || shaderName.Contains("side");
            }

            var isTransparent = false;
            var blend = BlendMode.None;
            if (source.GetTag("RenderType", false, "") == "Transparent")
            {
                //TODO
                isTransparent = true;
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

            this.SetBlend(enalbesJson, functionsJson, blend);
            this.SetCullFace(enalbesJson, functionsJson, !isDoubleSide);
            this.SetDepth(enalbesJson, functionsJson, true, !isTransparent);

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
        }

        protected void SetCullFace(MyJson_Array enalbesJson, MyJson_Tree functionsJson, bool cull)
        {
            if (cull)
            {
                var frontFaceJson = new MyJson_Array();
                frontFaceJson.AddInt((int)FrontFace.CCW);
                functionsJson.Add("frontFace", frontFaceJson);

                var cullFaceJson = new MyJson_Array();
                cullFaceJson.AddInt((int)CullFace.BACK);
                functionsJson.Add("cullFace", cullFaceJson);

                enalbesJson.AddInt((int)EnableState.CULL_FACE);
            }
        }

        protected void SetDepth(MyJson_Array enalbesJson, MyJson_Tree functionsJson, bool zTest, bool zWrite)
        {
            if (zTest)
            {
                var depthFunc = new MyJson_Array();
                depthFunc.AddInt((int)DepthFunc.LEQUAL);
                functionsJson.Add("depthFunc", depthFunc);
                enalbesJson.AddInt((int)EnableState.DEPTH_TEST);
            }

            if (zWrite)
            {
                var depthMask = new MyJson_Array();
                depthMask.AddBool(true);
                functionsJson.Add("depthMask", depthMask);
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

        protected virtual string technique
        {
            get
            {
                return "builtin/meshbasic.shader.json";
            }
        }
    }
}
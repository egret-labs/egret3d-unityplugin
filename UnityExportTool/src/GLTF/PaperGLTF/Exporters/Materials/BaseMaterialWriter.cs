namespace PaperGLTF
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Egret3DExportTools;
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

            var valuesJson = new MyJson_Tree();
            KHR_techniques_webglJson.Add("values", valuesJson);

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
            }
        }

        protected void SetCullFace(MyJson_Array enalbesJson, MyJson_Tree functionsJson, bool cull, FrontFace frontFace = FrontFace.CCW, CullFace cullFace = CullFace.BACK)
        {
            if (cull)
            {
                var frontFaceJson = new MyJson_Array();
                frontFaceJson.AddInt((int)frontFace);
                functionsJson.Add("frontFace", frontFaceJson);

                var cullFaceJson = new MyJson_Array();
                cullFaceJson.AddInt((int)cullFace);
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
                return "meshbasic";
            }
        }
    }
}
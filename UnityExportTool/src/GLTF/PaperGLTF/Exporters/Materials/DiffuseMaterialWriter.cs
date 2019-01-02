namespace PaperGLTF
{
    using UnityEngine;
    using UnityEditor;
    using Egret3DExportTools;
    public class DiffuseMaterialWriter : BaseMaterialWriter
    {
        protected override void Update()
        {
            var source = this.source;
            var mainTex = this.MainText;
            var mainColor = this.MainColor;
            if(mainTex != null)
            {
                var texPath = ResourceManager.instance.SaveTexture(mainTex as Texture2D, "");
                this.values.SetString("map", texPath);
                if (source.HasProperty("_MainTex_ST"))
                {
                    var mainST = this.GetVector4("_MainTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    this.values.SetUVTransform("uvTransform", mainST);
                }
            }

            this.values.SetColor3("diffuse", mainColor);
            this.values.SetNumber("opacity", mainColor.a);
        }

        protected UnityEngine.Texture MainText
        {
            get
            {
                var orginmps = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { source });
                foreach (var mp in orginmps)
                {
                    if (mp.type.ToString() == "Texture")
                    {
                        var tex = source.GetTexture(mp.name);
                        if (tex != null)
                        {
                            return tex;
                        }
                    }
                }

                return null;
            }
        }

        protected Color MainColor
        {
            get
            {
                var color = Color.white;
                var source = this.source;
                if (source.HasProperty("_Color"))
                {
                    color = source.GetColor("_Color");
                }
                else if (source.HasProperty("_MainColor"))
                {
                    color = source.GetColor("_MainColor");
                }
                else if (source.HasProperty("_TintColor"))
                {
                    color = source.GetColor("_TintColor");
                }
                return color;
            }
        }

        protected override string technique
        {
            get
            {
                return "builtin/meshbasic.shader.json";
            }
        }
    }
}
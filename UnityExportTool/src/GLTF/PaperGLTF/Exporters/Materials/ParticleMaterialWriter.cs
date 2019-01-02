namespace PaperGLTF
{
    using UnityEngine;
    using Egret3DExportTools;
    public class ParticleMaterialWriter : BaseMaterialWriter
    {
        protected override void Update()
        {
            var source = this.source;

            var tex = this.GetTexture("_MainTex", null);
            if (tex != null)
            {
                var texPath = ResourceManager.instance.SaveTexture(tex as Texture2D, "");
                this.values.SetString("map", texPath);

                if (source.HasProperty("_MainTex_ST"))
                {
                    var mainST = this.GetVector4("_MainTex_ST", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
                    this.values.SetUVTransform("uvTransform", mainST);
                }
            }

            Color color = Color.white;
            if (source.HasProperty("_TintColor"))
            {
                color = this.GetColor("_TintColor", Color.white);
            }
            else if (source.HasProperty("_Color"))
            {
                color = this.GetColor("_Color", Color.white);
            }
            this.values.SetColor3("diffuse", color);
            this.values.SetNumber("opacity", color.a);
        }

        protected override string technique
        {
            get
            {               
                return "builtin/particle.shader.json";
            }
        }
    }
}
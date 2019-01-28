namespace Egret3DExportTools
{
    using UnityEngine;
    public class ParticleMaterialWriter : BaseMaterialWriter
    {
        protected override void Update()
        {
            var source = this.source;

            var tex = this.GetTexture("_MainTex", null);
            if (tex != null)
            {
                this.SetTexture("map", tex);

                var defaultValue = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
                var mainST = this.GetVector4("_MainTex_ST", defaultValue);
                if (!mainST.Equals(defaultValue))
                {
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
            this.SetColor3AndOpacity(color, Color.white);
        }

        protected  override bool isDoubleSide
        {
            get
            {
                return true;
            }
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
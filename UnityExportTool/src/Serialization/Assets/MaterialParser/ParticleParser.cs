namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using UnityEngine;
    public class ParticleParser : BaseMaterialParser
    {
        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var source = this.source;
            var values = this.data.values;

            var tex = this.source.GetTexture("_MainTex", null);
            if (tex != null)
            {
                this.data.values.SetTexture("map", tex);

                var defaultValue = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
                var mainST = this.source.GetVector4("_MainTex_ST", defaultValue);
                if (!mainST.Equals(defaultValue))
                {
                    // values.SetUVTransform("uvTransform", mainST);
                    this.data.values.SetUVTransform("uvTransform", mainST);
                }
            }

            Color color = Color.white;
            if (source.HasProperty("_TintColor"))
            {
                color = this.source.GetColor("_TintColor", Color.white);
            }
            else if (source.HasProperty("_Color"))
            {
                color = this.source.GetColor("_Color", Color.white);
            }
            this.data.values.SetColor3("diffuse", color, Color.white);
            this.data.values.SetNumber("opacity", color.a, Color.white.a);
        }

        protected override bool isDoubleSide
        {
            get
            {
                return true;
            }
        }
    }
}
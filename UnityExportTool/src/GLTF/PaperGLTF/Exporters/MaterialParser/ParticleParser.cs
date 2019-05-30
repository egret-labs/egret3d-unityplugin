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

            var tex = this.GetTexture("_MainTex", null);
            if (tex != null)
            {
                this.SetTexture("map", tex);

                var defaultValue = new Vector4(1.0f, 1.0f, 0.0f, 0.0f);
                var mainST = this.GetVector4("_MainTex_ST", defaultValue);
                if (!mainST.Equals(defaultValue))
                {
                    values.SetUVTransform("uvTransform", mainST);
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
    }
}
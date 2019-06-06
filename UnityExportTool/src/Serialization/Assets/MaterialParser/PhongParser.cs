namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using UnityEngine;
    public class PhongParser : DiffuseParser
    {
        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var source = this.source;
            var shininess = 30.0f;
            if (this.source.HasProperty("_Shininess"))
            {
                shininess = this.source.GetFloat("_Shininess", 30.0f);
                if (shininess > 0.0f)
                {
                    shininess = 1 / shininess;
                }
            }
            this.data.values.SetNumber("shininess", shininess, 30.0f);

            var specularMap = this.source.GetTexture("_SpecGlossMap", null);
            if (specularMap != null)
            {
                this.data.values.SetTexture("specularMap", specularMap);
            }
            var aoMap = this.source.GetTexture("_OcclusionMap", null);
            if (aoMap != null)
            {
                this.data.values.SetTexture("aoMap", aoMap);
                this.data.values.SetNumber("aoMapIntensity", this.source.GetFloat("_OcclusionStrength", 1.0f), 1.0f);
            }
            var emissiveMap = this.source.GetTexture("_EmissionMap", null);
            if (emissiveMap != null)
            {
                this.data.values.SetTexture("emissiveMap", emissiveMap);
            }
            var normalMap = this.source.GetTexture("_BumpMap", null);
            if (normalMap != null)
            {
                this.data.values.SetTexture("normalMap", normalMap);
            }
            var displacementMap = this.source.GetTexture("_ParallaxMap", null);
            if (displacementMap != null)
            {
                this.data.values.SetTexture("displacementMap", displacementMap);
                this.data.values.SetNumber("displacementScale", this.source.GetFloat("_Parallax", 1.0f), 1.0f);
                this.data.values.SetNumber("displacementBias", 0.0f, 0.0f);
            }
        }
    }
}
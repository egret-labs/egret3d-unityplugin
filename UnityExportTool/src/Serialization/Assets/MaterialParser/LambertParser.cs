namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using UnityEngine;
    public class LambertParser : DiffuseParser
    {
        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var aoMap = this.source.GetTexture("_OcclusionMap", null);
            if (aoMap != null)
            {
                this.data.values.SetTexture("aoMap", aoMap);
                // this.SetFloat("aoMapIntensity", this.GetFloat("_OcclusionStrength", 1.0f), 1.0f);
                this.data.values.SetNumber("aoMapIntensity", this.source.GetFloat("_OcclusionStrength", 1.0f), 1.0f);
            }
            var emissiveMap = this.source.GetTexture("_EmissionMap", null);
            if (emissiveMap != null)
            {
                this.data.values.SetTexture("emissiveMap", emissiveMap);
            }
            var specGlossMap = this.source.GetTexture("_SpecGlossMap", null);
            if (specGlossMap != null)
            {
                this.data.values.SetTexture("specularMap", specGlossMap);
            }
        }
    }
}
namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using UnityEngine;
    public class LambertParser : DiffuseParser
    {
        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var aoMap = this.GetTexture("_OcclusionMap", null);
            if (aoMap != null)
            {
                this.SetTexture("aoMap", aoMap);
                this.SetFloat("aoMapIntensity", this.GetFloat("_OcclusionStrength", 1.0f), 1.0f);
            }
            var emissiveMap = this.GetTexture("_EmissionMap", null);
            if (emissiveMap != null)
            {
                this.SetTexture("emissiveMap", emissiveMap);
            }
            var specGlossMap = this.GetTexture("_SpecGlossMap", null);
            if (specGlossMap != null)
            {
                this.SetTexture("specularMap", specGlossMap);
            }
        }
    }
}
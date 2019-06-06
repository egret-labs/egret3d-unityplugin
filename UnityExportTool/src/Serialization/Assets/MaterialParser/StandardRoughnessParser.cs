namespace Egret3DExportTools
{
    using UnityEngine;
    public class StandardRoughnessParser : StandardParser
    {
       protected override void StandardBegin()
        {
            var roughness = this.source.GetFloat("_Glossiness", 0.0f);
            var metalness = this.source.GetFloat("_Metallic", 0.0f);
            var emissive = this.source.GetColor("_EmissionColor", Color.black);

            this.data.values.SetColor3("emissive", emissive, Color.black);
            this.data.values.SetNumber("roughness", roughness, 0.5f);
            this.data.values.SetNumber("metalness", metalness, 0.5f);

            var metalnessMap = this.source.GetTexture("_MetallicGlossMap", null);
            if (metalnessMap != null)
            {
                this.data.values.SetTexture("metalnessMap", metalnessMap);
            }

            var roughnessMap = this.source.GetTexture("_SpecGlossMap", null);
            if (roughnessMap != null)
            {
                this.data.values.SetTexture("roughnessMap", roughnessMap);
            }
        }
    }
}
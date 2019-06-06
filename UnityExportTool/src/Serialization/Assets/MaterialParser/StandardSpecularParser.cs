namespace Egret3DExportTools
{
    using UnityEngine;
    public class StandardSpecularParser : StandardParser
    {
        protected override void StandardBegin()
        {
            // var roughness = this.GetFloat("_Glossiness", 0.0f);
            var roughness = 1.0f;
            var metalness = this.source.GetFloat("_Metallic", 0.0f);
            var emissive = this.source.GetColor("_EmissionColor", Color.black);

            this.data.values.SetColor3("emissive", emissive, Color.black);
            this.data.values.SetNumber("roughness", roughness, 0.5f);
            this.data.values.SetNumber("metalness", metalness, 0.5f);

            var specularMap = this.source.GetTexture("_SpecGlossMap", null);
            if (specularMap != null)
            {
                this.data.values.SetTexture("specularMap", specularMap);
            }
        }
    }
}
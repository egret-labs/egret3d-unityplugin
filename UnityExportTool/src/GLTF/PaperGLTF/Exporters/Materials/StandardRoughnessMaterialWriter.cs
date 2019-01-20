namespace Egret3DExportTools
{
    using UnityEngine;
    public class StandardRoughnessMaterialWriter : StandardMaterialWriter
    {
       protected override void StandardBegin()
        {
            var roughness = this.GetFloat("_Glossiness", 0.0f);
            var metalness = this.GetFloat("_Metallic", 0.0f);
            var emissive = this.GetColor("_EmissionColor", Color.black);

            this.SetColor3("emissive", emissive, Color.black);
            this.SetFloat("roughness", roughness, 0.5f);
            this.SetFloat("metalness", metalness, 0.5f);

            var metalnessMap = this.GetTexture("_MetallicGlossMap", null);
            if (metalnessMap != null)
            {
                this.SetTexture("metalnessMap", metalnessMap);
            }

            var roughnessMap = this.GetTexture("_SpecGlossMap", null);
            if (roughnessMap != null)
            {
                this.SetTexture("roughnessMap", roughnessMap);
            }
        }
    }
}
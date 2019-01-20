namespace Egret3DExportTools
{
    using UnityEngine;
    public class StandardSpecularMaterialWriter : StandardMaterialWriter
    {
        protected override void StandardBegin()
        {
            // var roughness = this.GetFloat("_Glossiness", 0.0f);
            var roughness = 1.0f;
            var metalness = this.GetFloat("_Metallic", 0.0f);
            var emissive = this.GetColor("_EmissionColor", Color.black);

            this.SetColor3("emissive", emissive, Color.black);
            this.SetFloat("roughness", roughness, 0.5f);
            this.SetFloat("metalness", metalness, 0.5f);

            var specularMap = this.GetTexture("_SpecGlossMap", null);
            if (specularMap != null)
            {
                this.SetTexture("specularMap", specularMap);
            }
        }
    }
}
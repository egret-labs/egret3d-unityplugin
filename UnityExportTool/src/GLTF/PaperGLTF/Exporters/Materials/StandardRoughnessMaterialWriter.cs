namespace PaperGLTF
{
    using UnityEngine;
    using Egret3DExportTools;
    public class StandardRoughnessMaterialWriter : StandardMaterialWriter
    {
       protected override void StandardBegin()
        {
            var roughness = this.GetFloat("_Glossiness", 0.0f);
            var metalness = this.GetFloat("_Metallic", 0.0f);
            var emissive = this.GetColor("_EmissionColor", Color.black);

            this.values.SetColor3("emissive", emissive);
            this.values.SetNumber("roughness", roughness);
            this.values.SetNumber("metalness", metalness);

            var metalnessMap = this.GetTexture("_MetallicGlossMap", null);
            if (metalnessMap != null)
            {
                var texPath = ResourceManager.instance.SaveTexture(metalnessMap as Texture2D, "");
                this.values.SetString("metalnessMap", texPath);
            }

            var roughnessMap = this.GetTexture("_SpecGlossMap", null);
            if (roughnessMap != null)
            {
                var texPath = ResourceManager.instance.SaveTexture(roughnessMap as Texture2D, "");
                this.values.SetString("roughnessMap", texPath);
            }
        }
        // protected override void StandardEnd()
        // {
            
        // }
    }
}
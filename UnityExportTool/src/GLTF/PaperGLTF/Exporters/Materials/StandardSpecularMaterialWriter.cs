namespace PaperGLTF
{
    using UnityEngine;
    using Egret3DExportTools;
    public class StandardSpecularMaterialWriter : StandardMaterialWriter
    {
        protected override void StandardBegin()
        {
            // var roughness = this.GetFloat("_Glossiness", 0.0f);
            var roughness = 1.0f;
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
            Debug.Log("StandardSpecularMaterialWriter:" + roughness);
        }
        // protected override void StandardEnd()
        // {
            
        // }
    }
}
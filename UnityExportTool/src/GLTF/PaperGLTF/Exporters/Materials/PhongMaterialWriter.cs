namespace PaperGLTF
{
    using UnityEngine;
    using Egret3DExportTools;
    public class PhongMaterialWriter : DiffuseMaterialWriter
    {
        protected override void Update()
        {
            var source = this.source;
            if (source.HasProperty("_SpecGlossMap"))
            {
                var specularMap = source.GetTexture("_SpecGlossMap");
                if (specularMap != null)
                {
                    var texPath = ResourceManager.instance.SaveTexture(specularMap as Texture2D, "");
                    this.values.SetString("specularMap", texPath);
                    this.defines.Add("USE_SPECULARMAP");
                }
            }

            if (source.HasProperty("_BumpMap"))
            {
                var normalMap = source.GetTexture("_BumpMap");
                if (normalMap != null)
                {
                    var texPath = ResourceManager.instance.SaveTexture(normalMap as Texture2D, "");
                    this.values.SetString("normalMap", texPath);
                    this.defines.Add("USE_NORMALMAP");
                }
            }
            
            base.Update();
        }

        protected override string technique
        {
            get
            {
                var shaderName = this.source.shader.name.ToLower();
                var isDoubleSide = this.source.HasProperty("_Cull") && this.source.GetInt("_Cull") == (int)UnityEngine.Rendering.CullMode.Off;
                if (!isDoubleSide)
                {
                    isDoubleSide = shaderName.Contains("both") || shaderName.Contains("side");
                }
                if (isDoubleSide)
                {
                    return "meshphong_doubleside";
                }

                return "meshphong";
            }
        }
    }
}
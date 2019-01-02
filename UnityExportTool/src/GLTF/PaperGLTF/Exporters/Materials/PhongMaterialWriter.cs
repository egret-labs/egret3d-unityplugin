namespace PaperGLTF
{
    using UnityEngine;
    using Egret3DExportTools;
    public class PhongMaterialWriter : DiffuseMaterialWriter
    {
        protected override void Update()
        {
            base.Update();
            var source = this.source;
            if (source.HasProperty("_Shininess"))
            {
                var shininess = this.GetFloat("_Shininess", 0.0f);
                if (shininess > 0.0f)
                {
                    shininess = 1 / shininess;
                }
                if (shininess != 30.0f)
                {
                    this.values.SetNumber("shininess", shininess);
                }
            }

            if (source.HasProperty("_SpecGlossMap"))
            {
                var specularMap = source.GetTexture("_SpecGlossMap");
                if (specularMap != null)
                {
                    var texPath = ResourceManager.instance.SaveTexture(specularMap as Texture2D, "");
                    this.values.SetString("specularMap", texPath);
                }
            }

            if (source.HasProperty("_BumpMap"))
            {
                var normalMap = source.GetTexture("_BumpMap");
                if (normalMap != null)
                {
                    var texPath = ResourceManager.instance.SaveTexture(normalMap as Texture2D, "");
                    this.values.SetString("normalMap", texPath);
                }
            }

            base.Update();
        }

        protected override string technique
        {
            get
            {
                return "builtin/meshphong.shader.json";
            }
        }
    }
}
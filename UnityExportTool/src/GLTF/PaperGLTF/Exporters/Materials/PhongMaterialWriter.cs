namespace Egret3DExportTools
{
    using UnityEngine;
    public class PhongMaterialWriter : DiffuseMaterialWriter
    {
        protected override void Update()
        {
            base.Update();
            var source = this.source;
            var shininess = 30.0f;
            if (this.source.HasProperty("_Shininess"))
            {
                shininess = this.GetFloat("_Shininess", 30.0f);
                if (shininess > 0.0f)
                {
                    shininess = 1 / shininess;
                }
            }
            this.SetFloat("shininess", shininess, 30.0f);

            var specularMap = this.GetTexture("_SpecGlossMap", null);
            if (specularMap != null)
            {
                this.SetTexture("specularMap", specularMap);
            }
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
            var normalMap = this.GetTexture("_BumpMap", null);
            if (normalMap != null)
            {
                this.SetTexture("normalMap", normalMap);
            }
            var displacementMap = this.GetTexture("_ParallaxMap", null);
            if (displacementMap != null)
            {
                this.SetTexture("displacementMap", displacementMap);
                this.SetFloat("displacementScale", this.GetFloat("_Parallax", 1.0f), 1.0f);
                this.SetFloat("displacementBias", 0.0f, 0.0f);
            }
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
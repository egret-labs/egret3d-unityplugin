namespace Egret3DExportTools
{
    using UnityEngine;
    public class StandardMaterialWriter : BaseMaterialWriter
    {
        protected virtual void StandardBegin()
        {
            var roughness = 1.0f - this.GetFloat("_Glossiness", 0.0f);
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
        }
        protected virtual void StandardEnd()
        {
            this.defines.Add("STANDARD");
        }
        protected override void Update()
        {
            var source = this.source;
            this.StandardBegin();
            var map = this.GetTexture("_MainTex", null);
            if (map != null)
            {
                this.SetTexture("map", map);
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

            var bumpMap = this.GetTexture("_BumpMap", null);
            if (bumpMap != null)
            {
                var bumpScale = this.GetFloat("_BumpScale", 1.0f);
                this.SetTexture("normalMap", bumpMap);
                this.SetVector2("normalScale", new UnityEngine.Vector2(bumpScale, bumpScale), Vector2.one);
            }

            var normalMap = this.GetTexture("_DetailNormalMap", null);
            if (normalMap != null)
            {
                var normalScale = this.GetFloat("_DetailNormalMapScale", 1.0f);
                this.SetTexture("normalMap", normalMap);
                this.SetVector2("normalScale", new UnityEngine.Vector2(normalScale, normalScale), Vector2.one);
            }

            var displacementMap = this.GetTexture("_ParallaxMap", null);
            if (displacementMap != null)
            {
                this.SetTexture("displacementMap", displacementMap);
                this.SetFloat("displacementScale", this.GetFloat("_Parallax", 1.0f), 1.0f);
                this.SetFloat("displacementBias", 0.0f, 0.0f);
            }

            this.StandardEnd();
        }

        protected override string technique
        {
            get
            {
                return "builtin/meshphysical.shader.json";
            }
        }
    }
}
namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using UnityEngine;
    public class StandardParser : BaseMaterialParser
    {
        protected virtual void StandardBegin()
        {
            var roughness = 1.0f - this.source.GetFloat("_Glossiness", 0.0f);
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
        }

        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var source = this.source;
            this.StandardBegin();
            var map = this.source.GetTexture("_MainTex", null);
            if (map != null)
            {
                this.data.values.SetTexture("map", map);
            }
            var aoMap = this.source.GetTexture("_OcclusionMap", null);
            if (aoMap != null)
            {
                this.data.values.SetTexture("aoMap", aoMap);
                this.data.values.SetNumber("aoMapIntensity", this.source.GetFloat("_OcclusionStrength", 1.0f), 1.0f);
            }

            var emissiveMap = this.source.GetTexture("_EmissionMap", null);
            if (emissiveMap != null)
            {
                this.data.values.SetTexture("emissiveMap", emissiveMap);
            }

            var bumpMap = this.source.GetTexture("_BumpMap", null);
            if (bumpMap != null)
            {
                var bumpScale = this.source.GetFloat("_BumpScale", 1.0f);
                this.data.values.SetTexture("normalMap", bumpMap);
                this.data.values.SetVector2("normalScale", new UnityEngine.Vector2(bumpScale, bumpScale), Vector2.one);
            }

            var normalMap = this.source.GetTexture("_DetailNormalMap", null);
            if (normalMap != null)
            {
                var normalScale = this.source.GetFloat("_DetailNormalMapScale", 1.0f);
                this.data.values.SetTexture("normalMap", normalMap);
                this.data.values.SetVector2("normalScale", new UnityEngine.Vector2(normalScale, normalScale), Vector2.one);
            }

            var displacementMap = this.source.GetTexture("_ParallaxMap", null);
            if (displacementMap != null)
            {
                this.data.values.SetTexture("displacementMap", displacementMap);
                this.data.values.SetNumber("displacementScale", this.source.GetFloat("_Parallax", 1.0f), 1.0f);
                this.data.values.SetNumber("displacementBias", 0.0f, 0.0f);
            }            
        }

        public override void CollectDefines()
        {
            base.CollectDefines();
            this.data.asset.defines.Add(new Define() { name = "STANDARD" });
        }
    }
}
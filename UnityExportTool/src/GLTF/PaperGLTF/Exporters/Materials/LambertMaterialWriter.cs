namespace PaperGLTF
{
    using Egret3DExportTools;
    public class LambertMaterialWriter : DiffuseMaterialWriter
    {
        protected override void Update()
        {
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
                    return "meshlambert_doubleside";
                }

                return "meshlambert";
            }
        }
    }
}
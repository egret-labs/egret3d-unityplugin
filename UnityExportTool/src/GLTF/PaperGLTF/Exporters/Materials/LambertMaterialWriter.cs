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
                return "builtin/meshlambert.shader.json";
            }
        }
    }
}
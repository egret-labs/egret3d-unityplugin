namespace Egret3DExportTools
{
    public static class MaterialWriterFactory
    {
        public static BaseMaterialWriter Create(MaterialType type, UnityEngine.Material material)
        {
            BaseMaterialWriter writer;
            switch(type)
            {
                case MaterialType.Custom:
                {
                    writer = new CustomMaterialWriter();
                    break;
                }
                case MaterialType.Particle:
                {
                    writer = new ParticleMaterialWriter();
                    break;
                }
                case MaterialType.Lambert:
                {
                    writer = new LambertMaterialWriter();
                    break;
                }
                case MaterialType.Phong:
                {
                    writer = new PhongMaterialWriter();
                    break;
                }
                case MaterialType.Standard:
                {
                    writer = new StandardMaterialWriter();
                    break;
                }
                case MaterialType.StandardSpecular:
                {
                    writer = new StandardSpecularMaterialWriter();
                    break;
                }
                case MaterialType.StandardRoughness:
                {
                    writer = new StandardRoughnessMaterialWriter();
                    break;
                }
                default:
                {
                    writer = new DiffuseMaterialWriter();
                    break;
                }
            }

            writer.source = material;

            return writer;
        }
    }
}
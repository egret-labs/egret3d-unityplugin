namespace Egret3DExportTools
{
    using GLTF.Schema;

    public static class GLTFInitialize
    {
        public static void Initialize()
        {
            GLTFProperty.RegisterExtension(new CoordinateSystemExtensionFactory());
            GLTFProperty.RegisterExtension(new AnimationExtensionFactory());
        }
    }
}

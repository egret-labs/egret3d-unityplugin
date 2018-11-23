namespace PaperGLTF
{
    using GLTF.Schema;
    using PaperGLTF.Schema;

    public static class GLTFInitialize
    {
        public static void Initialize()
        {
            GLTFProperty.RegisterExtension(new CoordinateSystemExtensionFactory());
            GLTFProperty.RegisterExtension(new AnimationExtensionFactory());
        }
    }
}

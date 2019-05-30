namespace Egret3DExportTools
{
    using Newtonsoft.Json.Linq;
    using GLTF.Extensions;
    using GLTF.Schema;

    public class CoordinateSystemExtensionFactory : ExtensionFactory
    {
        public const string EXTENSION_NAME = "coordinateSystem";
        public const string COORDINATE_DIR = "dir";
        public const string COORDINATE_Unit = "unit";

        public CoordinateSystemExtensionFactory()
        {
            ExtensionName = EXTENSION_NAME;
        }

        public override IExtension Deserialize(GLTFRoot root, JProperty extensionToken)
        {
            var dir = CoordinateSystem.leftHand.ToString();
            var unit = 1.0f;
            if (extensionToken != null)
            {
                JToken dirToken = extensionToken.Value[COORDINATE_DIR];
                dir = dirToken != null ? dirToken.ToString() : dir;

                JToken scaleToken = extensionToken.Value[COORDINATE_Unit];
                unit = scaleToken != null ? scaleToken.DeserializeAsInt() : unit;
            };

            return new CoordinateSystemExtension(dir, unit);
        }
    }
}

using GLTF.Schema;
using Newtonsoft.Json.Linq;

namespace Egret3DExportTools
{
    public class AssetVersionExtension : IExtension
    {
        public const string EXTENSION_NAME = "egret";
        public string version = "5.0";
        public string minVersion = "5.0";

        public IExtension Clone(GLTFRoot root)
        {
            return new AssetVersionExtension
            {
                version = version,
                minVersion = minVersion,
            };
        }

        public virtual JProperty Serialize()
        {
            JObject ext = new JObject();
            
            ext.SetString("version", version);
            ext.SetString("minVersion", minVersion);

            return new JProperty("egret", ext);
        }
    }
}
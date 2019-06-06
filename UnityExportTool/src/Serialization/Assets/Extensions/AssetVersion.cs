using GLTF.Schema;
using Newtonsoft.Json.Linq;

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

        ext.Add(new JProperty(
            "version",
            version
        ));

        ext.Add(new JProperty(
            "minVersion",
            minVersion
        ));

        return new JProperty("egret", ext);
    }
}
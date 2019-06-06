using GLTF.Schema;
using Newtonsoft.Json.Linq;

namespace Egret3DExportTools
{
    public class TextureExtension : IExtension
    {
        public int anisotropy = 1;
        public int format = 6408;
        public int levels = 0;

        public IExtension Clone(GLTFRoot root)
        {
            return new TextureExtension
            {
                anisotropy = anisotropy,
                format = format,
                levels = levels
            };
        }

        public JProperty Serialize()
        {
            JObject ext = new JObject();

            if (this.anisotropy > 1)
            {
                ext.Add(new JProperty(
                "anisotropy",
                this.anisotropy
            ));
            }

            ext.Add(new JProperty(
                "format",
                format
            ));

            ext.Add(new JProperty(
                "levels",
                levels
            ));

            return new JProperty("egret", ext);
        }
    }
}
using GLTF.Schema;
using Newtonsoft.Json.Linq;

namespace Egret3DExportTools
{
    public class TextureExtension : IExtension
    {
        public const string EXTENSION_NAME = "egret";
        public int width = 0;
        public int height = 0;
        public int format = 6408;
        public int levels = 0;
        public int encoding = 0;
        public int faces = 1;
        public int mapping = 0;
        public int anisotropy = 1;

        public IExtension Clone(GLTFRoot root)
        {
            return new TextureExtension
            {
                width = width,
                height = height,
                format = format,
                levels = levels,
                encoding = encoding,
                faces = faces,
                mapping = mapping,
                anisotropy = anisotropy,
            };
        }

        public JProperty Serialize()
        {
            JObject ext = new JObject();

            ext.SetInt("width", this.width, 0);
            ext.SetInt("height", this.height, 0);
            ext.SetInt("format", this.format, 6408);
            ext.SetInt("levels", this.levels, 1);
            ext.SetInt("encoding", this.encoding, 0);
            ext.SetInt("faces", this.faces, 1);
            ext.SetInt("mapping", this.mapping, 0);
            ext.SetInt("anisotropy", this.anisotropy, 1);

            return new JProperty("egret", ext);
        }
    }
}
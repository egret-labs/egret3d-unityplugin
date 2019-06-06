namespace Egret3DExportTools
{
    using GLTF.Schema;
    using Newtonsoft.Json.Linq;
    public enum CoordinateSystem
    {
        leftHand,
        rightHand
    }

    public class CoordinateSystemExtension : IExtension
    {
        public const string EXTENSION_NAME = "coordinateSystem";
        public string coordinateDir = CoordinateSystem.leftHand.ToString();
        public float coordinateUnit;
        public CoordinateSystemExtension(string d, float u)
        {
            this.coordinateDir = d;
            this.coordinateUnit = u;
        }

        public IExtension Clone(GLTFRoot root)
        {
            return new CoordinateSystemExtension(this.coordinateDir, this.coordinateUnit);
        }

        public JProperty Serialize()
        {
            JObject ext = new JObject();

            ext.Add(new JProperty(
                    "dir",
                    coordinateDir.ToString()
                ));

            ext.Add(new JProperty(
                    "unit",
                    coordinateUnit
                ));

            return new JProperty(CoordinateSystemExtension.EXTENSION_NAME, ext);
        }
    }
}

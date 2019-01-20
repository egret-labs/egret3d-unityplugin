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
                    CoordinateSystemExtensionFactory.COORDINATE_DIR,
                    coordinateDir.ToString()
                ));

            ext.Add(new JProperty(
                    CoordinateSystemExtensionFactory.COORDINATE_Unit,
                    coordinateUnit
                ));

            return new JProperty(CoordinateSystemExtensionFactory.EXTENSION_NAME, ext);
        }
    }
}

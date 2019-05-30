namespace Egret3DExportTools
{
    using Newtonsoft.Json.Linq;
    using GLTF.Schema;
    using System.Collections.Generic;

    public class AnimationExtensionFactory : ExtensionFactory
    {
        // Animation.
        public const string EXTENSION_NAME = "paper";
        public const string FRAME_RATE = "frameRate";
        public const string CLIPS = "clips";
        public const string EVENTS = "events";

        // AnimationChannel.
        public const string TYPE = "type";
        public const string PROPERTY = "property";

        public AnimationExtensionFactory()
        {
            ExtensionName = EXTENSION_NAME;
        }

        public override IExtension Deserialize(GLTFRoot root, JProperty extensionToken)
        {           
            return new AnimationExtension();
        }
    }
}

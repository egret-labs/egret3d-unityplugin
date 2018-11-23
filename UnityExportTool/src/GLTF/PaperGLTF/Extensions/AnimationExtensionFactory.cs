namespace PaperGLTF.Schema
{
    using Newtonsoft.Json.Linq;
    using GLTF.Schema;
    using System.Collections.Generic;

    public class AnimationExtensionFactory : ExtensionFactory
    {
        // Animation.
        public const string EXTENSION_NAME = "paper";
        public const string FRAME_RATE = "frameRate";
        public const string FRAME_COUNT = "frameCount";
        public const string DATA = "data";
        public const string FRAMES = "frames";
        public const string JOINTS = "joints";
        public const string CLIPS = "clips";

        // AnimationChannel.
        public const string TYPE = "type";
        public const string PROPERTY = "property";

        public AnimationExtensionFactory()
        {
            ExtensionName = EXTENSION_NAME;
        }

        public override IExtension Deserialize(GLTFRoot root, JProperty extensionToken)
        {           
            var frameRate = 25.0f;
            var frameCount = 0;
            var data = 0;
            var frames = new List<int>();
            var joints = new List<string>();
            var clips = new List<AnimationClip>();
            /*if (extensionToken != null)
            {
                JToken token = extensionToken.Value[FRAME_RATE];
                frameRate = token != null ? token.DeserializeAsInt() : frameRate;

                token = extensionToken.Value[FRAME_COUNT];
                frameCount = token != null ? token.DeserializeAsInt() : frameCount;

                token = extensionToken.Value[DATA];
                data = token != null ? token.DeserializeAsInt() : data;

                token = extensionToken.Value[FRAMES]; 
            }*/

            return new AnimationExtension {
                frameRate = frameRate,
                frameCount = frameCount,
                data = data,
                frames = frames,
                joints = joints,
                clips = clips
            };
        }
    }
}

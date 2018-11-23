namespace PaperGLTF.Schema
{
    using GLTF.Schema;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public class FrameEvent
    {
        public string name;
        public float position;
        public int intVariable;
        public float floatVariable;
        public string stringVariable;
    }

    public class AnimationClip
    {
        public string name;
        public int playTimes;
        public float position;
        public float duration;
        public List<FrameEvent> events = new List<FrameEvent>();
    }

    public class AnimationChannelExtension : IExtension {
        public string type;
        public string property;

        public IExtension Clone(GLTFRoot root)
        {
            return new AnimationChannelExtension
            {
                type = type,
                property = property,
            };
        }

        public JProperty Serialize()
        {
            JObject ext = new JObject();

            ext.Add(new JProperty(
                AnimationExtensionFactory.TYPE,
                type
            ));

            ext.Add(new JProperty(
                AnimationExtensionFactory.PROPERTY,
                property
            ));

            return new JProperty(AnimationExtensionFactory.EXTENSION_NAME, ext);
        }
    }

    public class AnimationExtension : IExtension
    {
        public float frameRate;
        public int frameCount;
        public int data;
        public List<int> frames = new List<int>();
        public List<string> joints = new List<string>();
        public List<AnimationClip> clips = new List<AnimationClip>();

        public IExtension Clone(GLTFRoot root)
        {
            return new AnimationExtension {
                frameRate = frameRate,
                frameCount = frameCount,
                data = data,
                frames = frames,
                joints = joints,
                clips = clips
            };
        }

        public JProperty Serialize()
        {
            JObject ext = new JObject();

            ext.Add(new JProperty(
                    AnimationExtensionFactory.FRAME_RATE,
                    frameRate
                ));

            ext.Add(new JProperty(
                    AnimationExtensionFactory.FRAME_COUNT,
                    frameCount
                ));

            ext.Add(new JProperty(
                    AnimationExtensionFactory.DATA,
                    data
                ));

            ext.Add(new JProperty(
                    AnimationExtensionFactory.FRAMES,
                    new JArray(frames)
                ));

            ext.Add(new JProperty(
                    AnimationExtensionFactory.JOINTS,
                    new JArray(joints)
                ));

            if (this.clips.Count > 0)
            {
                var obj = JsonConvert.SerializeObject(clips);
                JsonConvert.DeserializeObject(obj);

                ext.Add(new JProperty(
                        AnimationExtensionFactory.CLIPS,
                        JsonConvert.DeserializeObject(obj)
                    ));
            }
            
            return new JProperty(AnimationExtensionFactory.EXTENSION_NAME, ext);
        }
    }
}

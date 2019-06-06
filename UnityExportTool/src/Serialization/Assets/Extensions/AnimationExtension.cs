namespace Egret3DExportTools
{
    using GLTF.Schema;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;

    public class AnimationClip
    {
        public string name;
        public int playTimes;
        public float position;
        public float duration;
    }

    public class AnimationFrameEvent
    {
        public string name;
        public float position;
        public int intVariable;
        public float floatVariable;
        public string stringVariable;
    }

    public class AnimationChannelExtension : IExtension {
        public string type;
        public string property;
        public string uri;
        public int needUpdate;

        public IExtension Clone(GLTFRoot root)
        {
            return new AnimationChannelExtension
            {
                type = type,
                property = property,
                uri = uri,
                needUpdate = needUpdate,
            };
        }

        public JProperty Serialize()
        {
            JObject ext = new JObject();

            ext.Add(new JProperty(
                "type",
                type
            ));

            ext.Add(new JProperty(
                "property",
                property
            ));

            ext.Add(new JProperty(
                "uri",
                uri
            ));

            ext.Add(new JProperty(
                "needUpdate",
                needUpdate
            ));

            return new JProperty(AnimationExtension.EXTENSION_NAME, ext);
        }
    }

    public class AnimationExtension : IExtension
    {
        public const string EXTENSION_NAME = "egret";
        public float frameRate;
        public List<AnimationClip> clips = new List<AnimationClip>();
        public List<AnimationFrameEvent> events = new List<AnimationFrameEvent>();

        public IExtension Clone(GLTFRoot root)
        {
            return new AnimationExtension {
                frameRate = frameRate,
                clips = clips,
                events = events
            };
        }

        public JProperty Serialize()
        {
            JObject ext = new JObject();

            ext.Add(new JProperty(
                    "frameRate",
                    frameRate
                ));

            if (clips.Count > 0)
            {
                var obj = JsonConvert.SerializeObject(clips);
                JsonConvert.DeserializeObject(obj);

                ext.Add(new JProperty(
                        "clips",
                        JsonConvert.DeserializeObject(obj)
                    ));
            }

            if (events.Count > 0)
            {
                var obj = JsonConvert.SerializeObject(events);
                JsonConvert.DeserializeObject(obj);

                ext.Add(new JProperty(
                        "events",
                        JsonConvert.DeserializeObject(obj)
                    ));
            }

            return new JProperty(AnimationExtension.EXTENSION_NAME, ext);
        }
    }
}

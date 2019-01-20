namespace Egret3DExportTools
{
    using GLTF.Schema;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using UnityGLTF.Extensions;

    public class AnimationWriter : GLTFExporter
    {
        private Transform _target;
        public AnimationWriter(Transform target):base()
        {
            this._target = target;
        }

        public override byte[] WriteGLTF()
        {
            this.BeginWrite();
            ExportAnimation();
            var res = this.EndWrite();
            return res;
        }

        private void ExportAnimation()
        {
            var player = this._target.GetComponent<FB.PosePlus.AniPlayer>();
            var totalBones = player.bones.Count;
            var totalFrame = player.totalFrame();

            var bufSize = UnityEngine.Quaternion.identity.GetBytes().Length + UnityEngine.Vector3.zero.GetBytes().Length;
            var binarySingleFrameSize = totalBones * bufSize;

            var animation = new GLTF.Schema.Animation()
            {
                Name = player.name,
                Extensions = new Dictionary<string, IExtension>()
            };          

            var ext = new AnimationExtension();
            ext.frameRate = player.clips[0].fps;
            ext.clips = new List<AnimationClip>();
            float position = 0;
            for (var aniIndex = 0; aniIndex < player.clips.Count; aniIndex++)
            {
                var rawClip = player.clips[aniIndex];
                var ani = new AnimationClip();
                ani.name = rawClip.clipName;
                ani.position = position;
                ani.duration = (rawClip.frames.Count - 1) / ext.frameRate;
                position += rawClip.frames.Count / ext.frameRate;
                /*for (var evtIndex = 0; evtIndex < rawClip.events.Count; evtIndex++)
                {
                    var rawEvent = rawClip.events[evtIndex];
                    var evt = new Schema.FrameEvent();
                    evt.name = rawEvent.name;
                    evt.position = rawEvent.position;
                    evt.intVariable = rawEvent.intVariable;
                    evt.floatVariable = rawEvent.floatVariable;
                    evt.stringVariable = rawEvent.stringVariable;
                    ani.events.Add(evt);
                }*/
                ext.clips.Add(ani);
            }

            var bufferOffset = this._bufferWriter.BaseStream.Position;

            WriteBinary(player, totalFrame, binarySingleFrameSize);

            var bufferLength = this._bufferWriter.BaseStream.Position;

            var accessor = new Accessor();
            accessor.Count = (int)(bufferLength / 4);
            accessor.Type = GLTFAccessorAttributeType.SCALAR;
            accessor.ComponentType = GLTFComponentType.Float;
            accessor.BufferView = ExportBufferView((int)0, (int)bufferLength);

            this._root.Accessors.Add(accessor);

            animation.Extensions.Add(AnimationExtensionFactory.EXTENSION_NAME, ext);

            this._root.Animations.Add(animation);
        }


        public void WriteBinary(FB.PosePlus.AniPlayer player, int totalFrame, int binarySingleFrameSize)
        {
            var binaryTotalFrameSize = totalFrame * binarySingleFrameSize;
            var clips = player.clips;
            int totalBones = player.bones.Count;
            //ChunkData
            for (int i = 0, l = clips.Count; i < l; i++)
            {
                var clip = clips[i];
                if (clip == null)
                {
                    continue;
                }
                var frames = clip.frames;
                for (int j = 0, ll = frames.Count; j < ll; j++)
                {
                    var frame = frames[j];
                    if (frame == null)
                    {
                        continue;
                    }
                    var bones = frame.bonesinfo;
                    for (int k = 0, lll = bones.Count; k < lll; k++)
                    {
                        var boneName = clip.boneinfo[k];
                        var bonePos = bones[k];
                        if (bonePos == null || player.getbone(boneName) == null)
                        {
                            continue;
                        }
                        //
                        this._bufferWriter.Write(bonePos.r.x);
                        this._bufferWriter.Write(bonePos.r.y);
                        this._bufferWriter.Write(bonePos.r.z);
                        this._bufferWriter.Write(bonePos.r.w);

                        this._bufferWriter.Write(bonePos.t.x);
                        this._bufferWriter.Write(bonePos.t.y);
                        this._bufferWriter.Write(bonePos.t.z);
                    }
                }
            }
        }

        protected override void Init()
        {
            base.Init();

            this._root = new GLTFRoot
            {
                Accessors = new List<Accessor>(),
                Asset = new Asset
                {
                    Version = "2.0",
                    Generator = "paper",
                },
                Buffers = new List<GLTF.Schema.Buffer>(),
                BufferViews = new List<BufferView>(),
                Animations = new List<GLTF.Schema.Animation>(),
            };

            _buffer = new GLTF.Schema.Buffer();
            _bufferId = new BufferId
            {
                Id = _root.Buffers.Count,
                Root = _root
            };
            _root.Buffers.Add(_buffer);
        }
    }
}

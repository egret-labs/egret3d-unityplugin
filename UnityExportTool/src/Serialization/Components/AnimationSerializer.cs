using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Egret3DExportTools
{
    public class AnimationSerializer : ComponentSerializer
    {
        private List<UnityEngine.AnimationClip> animationClips = new List<UnityEngine.AnimationClip>();
        protected UnityEngine.AnimationClip[] _getAnimationClips(Component component)
        {
            return AnimationUtility.GetAnimationClips(component.gameObject);
        }
        public override bool Match(Component component)
        {
            var animation = component as Animation;
            this.animationClips.Clear();
            if (animation.clip != null)
            {
                this.animationClips.Add(animation.clip);
            }

            var clips = _getAnimationClips(animation);
            if (clips != null && clips.Length > 0)
            {
                foreach (var clip in clips)
                {
                    if (clip == animation.clip)
                    {
                        continue;
                    }

                    this.animationClips.Add(clip);
                }
            }

            if (this.animationClips.Count == 0)
            {
                return false;
            }

            return true;
        }

        public override void Serialize(Component component, ComponentData compData)
        {
            var animation = component as Animation;            
            compData.properties.SetBool("autoPlay", animation.playAutomatically);
            compData.SetAnimation(component.gameObject, this.animationClips.ToArray());
        }
    }
}
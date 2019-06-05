using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Egret3DExportTools
{
    public class AnimationSerializer : ComponentSerializer
    {
        protected UnityEngine.AnimationClip[] _getAnimationClips(Component component)
        {
            return AnimationUtility.GetAnimationClips(component.gameObject);
        }

        public override bool Serialize(Component component, ComponentData compData)
        {
            var animation = component as Animation;
            var animationClips = new List<UnityEngine.AnimationClip>();
            if (animation.clip)
            {
                animationClips.Add(animation.clip);
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

                    animationClips.Add(clip);
                }
            }

            if (animationClips.Count == 0)
            {
                return false;
            }

            compData.SetBool("autoPlay", animation.playAutomatically);
            compData.SetAnimation(component.gameObject, animationClips.ToArray());
            return true;
        }
    }
}
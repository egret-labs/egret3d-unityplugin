using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Egret3DExportTools
{
    public class AnimationParser : ComponentParser
    {
        protected UnityEngine.AnimationClip[] _getAnimationClips(Component component)
        {
            return AnimationUtility.GetAnimationClips(component.gameObject);
        }

        public override bool WriteToJson(GameObject gameObject, Component component, MyJson_Object compJson)
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

            compJson.SetBool("autoPlay", animation.playAutomatically);
            compJson.SetAnimation(gameObject, animationClips.ToArray());
            return true;
        }
    }
}
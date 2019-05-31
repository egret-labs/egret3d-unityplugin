using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class AnimatorSerializer : AnimationSerializer
    {
        public override bool WriteToJson(GameObject gameObject, Component component, MyJson_Object compJson, MyJson_Object entityJson)
        {
            var aniamtior = component as Animator;
            if (aniamtior.runtimeAnimatorController == null)
            {
                MyLog.Log("缺少runtimeAnimatorController");
                return false;
            }
            var clips = aniamtior.runtimeAnimatorController.animationClips;
            if (clips == null || clips.Length == 0)
            {
                MyLog.Log("clips为空");
                return false;
            }

            compJson.SetBool("autoPlay", true); // TODO
            compJson.SetAnimation(gameObject, clips);

            return true;
        }
    }
}
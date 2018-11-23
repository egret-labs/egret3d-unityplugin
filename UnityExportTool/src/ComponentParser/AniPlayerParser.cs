using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class AniPlayerParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            FB.PosePlus.AniPlayer comp = component as FB.PosePlus.AniPlayer;

            var animationsItem = new MyJson_Array();
            compJson["_animations"] = animationsItem;

            var animator = obj.GetComponent<Animator>();
            if (comp.clips.Count > 0 && animator != null)
            {
                int gltfHash = animator.runtimeAnimatorController.GetInstanceID();
                string url = ResourceManager.instance.SaveAniPlayer(comp, animator);
                var assetIndex = ResourceManager.instance.AddAssetUrl(url);

                var aniItem = new MyJson_Tree();
                aniItem.SetInt("asset", assetIndex);
                animationsItem.Add(aniItem);
            }

            return true;
        }
    }
}


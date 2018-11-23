using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class TransformParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            Transform comp = component as Transform;

            Vector3 localPosition = comp.localPosition;
            Quaternion localRotation = comp.localRotation;
            Vector3 localScale = comp.localScale;
            //这里特殊处理一下，拥有SkinnedMeshRenderer组件的Transform属性清零，因为Egret3D实现不同，如果不清零，会影响动画
            /*if (obj.GetComponent<SkinnedMeshRenderer>() != null)
            {
                localPosition = Vector3.zero;
                localRotation = Quaternion.identity;
                localScale = Vector3.one;
            }*/
            //localPosition
            compJson.SetVector3("localPosition", localPosition);
            //localRotation
            compJson.SetQuaternion("localRotation", localRotation);
            //localScale
            compJson.SetVector3("localScale", localScale);
            if ((component as Transform).parent)
            {
                compJson.SetHashCode("_parent", comp.parent);
            }
            var childrenItem = new MyJson_Array();
            compJson["children"] = childrenItem;
            for (int i = 0; i < comp.childCount; i++)
            {
                var child = comp.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    childrenItem.AddHashCode(child);
                }
            }

            return true;
        }
    }
}

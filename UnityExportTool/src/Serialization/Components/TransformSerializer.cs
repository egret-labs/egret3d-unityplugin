using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class TransformSerializer : ComponentSerializer
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson, MyJson_Object entityJson)
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
            compJson.SetString("name", obj.name);
            compJson.SetString("tag", obj.tag);
            compJson.SetInt("layer", 1 << obj.layer);
            compJson.SetBool("isStatic", obj.isStatic);
            //localPosition
            compJson.SetVector3("_localPosition", localPosition);
            //localRotation
            compJson.SetQuaternion("_localRotation", localRotation);
            //localScale
            compJson.SetVector3("_localScale", localScale);
            // if ((component as Transform).parent)
            // {
            //     compJson.SetHashCode("_parent", comp.parent);
            // }
            var childrenItem = new MyJson_Array();
            compJson["children"] = childrenItem;
            // for (int i = 0; i < comp.childCount; i++)
            // {
            //     var child = comp.GetChild(i);
            //     if (child.gameObject.activeInHierarchy)
            //     {
            //         childrenItem.AddHashCode(child);
            //     }
            // }

            //遍历子对象
            if (comp.childCount > 0)
            {
                for (int i = 0; i < comp.childCount; i++)
                {
                    var child = comp.GetChild(i).gameObject;
                    if (child.gameObject.activeInHierarchy)
                    {
                        var childJson = SerializeObject.Serialize(child);
                        childrenItem.AddHashCode(childJson);
                    }
                }
            }


            return true;
        }
    }
}

using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class TransformSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            var obj = component.gameObject;
            Transform comp = component as Transform;

            Vector3 localPosition = comp.localPosition;
            Quaternion localRotation = comp.localRotation;
            Vector3 localScale = comp.localScale;

            compData.SetString("name", obj.name);
            compData.SetString("tag", obj.tag);
            compData.SetInt("layer", 1 << obj.layer);
            compData.SetBool("isStatic", obj.isStatic);
            //localPosition
            compData.SetVector3("_localPosition", localPosition);
            //localRotation
            compData.SetQuaternion("_localRotation", localRotation);
            //localScale
            compData.SetVector3("_localScale", localScale);

            if (comp.childCount > 0)
            {
                for (int i = 0; i < comp.childCount; i++)
                {
                    var child = comp.GetChild(i).gameObject;
                    if (child.gameObject.activeInHierarchy)
                    {
                        var childEntity = SerializeObject.SerializeEntity(child);
                        compData.AddChild(childEntity.transform);
                    }
                }
            }
            return true;
        }
    }
}

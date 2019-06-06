using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class TransformSerializer : ComponentSerializer
    {
        public override void Serialize(Component component, ComponentData compData)
        {
            var obj = component.gameObject;
            Transform comp = component as Transform;

            Vector3 localPosition = comp.localPosition;
            Quaternion localRotation = comp.localRotation;
            Vector3 localScale = comp.localScale;

            compData.properties.SetString("name", obj.name);
            compData.properties.SetString("tag", obj.tag);
            compData.properties.SetInt("layer", 1 << obj.layer);
            compData.properties.SetBool("isStatic", obj.isStatic);
            //localPosition
            compData.properties.SetVector3("_localPosition", localPosition);
            //localRotation
            compData.properties.SetQuaternion("_localRotation", localRotation);
            //localScale
            compData.properties.SetVector3("_localScale", localScale);

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
        }
    }
}

using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class SphereColliderSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            SphereCollider comp = component as SphereCollider;
            var sphereItem = new MyJson_Array();
            sphereItem.AddNumber(comp.center.x);
            sphereItem.AddNumber(comp.center.y);
            sphereItem.AddNumber(comp.center.z);
            sphereItem.AddNumber(comp.radius);

            compData.props.Add("sphere", sphereItem);

            return true;
        }
    }
}























using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class SphereColliderParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            SphereCollider comp = component as SphereCollider;
            var sphereItem = new MyJson_Array();
            sphereItem.AddNumber(comp.center.x);
            sphereItem.AddNumber(comp.center.y);
            sphereItem.AddNumber(comp.center.z);
            sphereItem.AddNumber(comp.radius);

            compJson.Add("sphere", sphereItem);

            return true;
        }
    }
}























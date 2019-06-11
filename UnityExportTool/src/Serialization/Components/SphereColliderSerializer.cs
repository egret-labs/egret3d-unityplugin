using UnityEngine;
using System;

namespace Egret3DExportTools
{
    using Newtonsoft.Json.Linq;
    public class SphereColliderSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            SphereCollider comp = component as SphereCollider;
            var arr = new JArray();
            arr.AddNumber(comp.center.x);
            arr.AddNumber(comp.center.y);
            arr.AddNumber(comp.center.z);
            arr.AddNumber(comp.radius);

            compData.properties.Add("sphere", arr);
        }
    }
}























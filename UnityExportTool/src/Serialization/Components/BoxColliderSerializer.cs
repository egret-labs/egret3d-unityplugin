using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
namespace Egret3DExportTools
{
    public class BoxColliderSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            BoxCollider comp = component as BoxCollider;

            var halfSize = comp.size * 0.5f;
            var minimum = comp.center - halfSize;
            var maximum = comp.center + halfSize;
            JArray aabbItem = new JArray();
            aabbItem.AddNumber(minimum.x);
            aabbItem.AddNumber(minimum.y);
            aabbItem.AddNumber(minimum.z);
            aabbItem.AddNumber(maximum.x);
            aabbItem.AddNumber(maximum.y);
            aabbItem.AddNumber(maximum.z);
            
            compData.properties.Add("aabb", aabbItem);
        }
    }
}





















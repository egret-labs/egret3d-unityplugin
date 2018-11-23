using UnityEngine;
using System;
namespace Egret3DExportTools
{
    public class BoxColliderParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            BoxCollider comp = component as BoxCollider;

            var halfSize = comp.size * 0.5f;
            var minimum = comp.center - halfSize;
            var maximum = comp.center + halfSize;
            MyJson_Array aabbItem = new MyJson_Array();
            aabbItem.AddNumber(minimum.x);
            aabbItem.AddNumber(minimum.y);
            aabbItem.AddNumber(minimum.z);
            aabbItem.AddNumber(maximum.x);
            aabbItem.AddNumber(maximum.y);
            aabbItem.AddNumber(maximum.z);
            
            compJson.Add("aabb", aabbItem);
            return true;
        }
    }
}





















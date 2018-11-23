using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class ComponentParser_MeshCollider : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            return false;
        }
    }
}

using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class MeshFilterSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            MeshFilter comp = component as MeshFilter;
            compData.SetMesh(component.gameObject, comp.sharedMesh);
            return true;
        }
    }
}

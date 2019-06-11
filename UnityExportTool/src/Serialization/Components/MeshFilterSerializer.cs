using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class MeshFilterSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            MeshFilter comp = component as MeshFilter;
            compData.properties.SetMesh(component.gameObject, comp.sharedMesh);
        }
    }
}

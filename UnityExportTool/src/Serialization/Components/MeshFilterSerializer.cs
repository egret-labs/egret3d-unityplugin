using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class MeshFilterSerializer : ComponentSerializer
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson, MyJson_Object entityJson)
        {
            MeshFilter comp = component as MeshFilter;
            compJson.SetMesh(obj, comp.sharedMesh);

            return true;
        }
    }
}

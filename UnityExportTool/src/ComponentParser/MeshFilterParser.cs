using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class MeshFilterParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            MeshFilter comp = component as MeshFilter;
            compJson.SetMesh(obj, comp.sharedMesh);

            return true;
        }
    }
}

using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class SkinnedMeshRendererSerializer : ComponentSerializer
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson, MyJson_Object entityJson)
        {
            SkinnedMeshRenderer comp = component as SkinnedMeshRenderer;

            var meshFilter = new MyJson_Object();
            meshFilter.SetSerializeClass(meshFilter.GetHashCode(), SerializeClass.MeshFilter);
            meshFilter.SetMesh(obj, comp.sharedMesh);
            (entityJson["components"] as MyJson_Array).AddHashCode(meshFilter);
            ResourceManager.instance.AddCompJson(meshFilter);
            
            compJson.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compJson.SetBool("_receiveShadows", comp.receiveShadows);
            compJson.SetMaterials(obj, comp.sharedMaterials, false, true);            

            return true;
        }
    }
}
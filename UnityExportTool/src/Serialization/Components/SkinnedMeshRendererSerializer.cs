using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class SkinnedMeshRendererSerializer : ComponentSerializer
    {
        public override void Serialize(Component component, ComponentData compData)
        {
            SkinnedMeshRenderer comp = component as SkinnedMeshRenderer;

            var meshFilter = SerializeObject.currentData.CreateComponent(SerializeClass.MeshFilter);
            meshFilter.SetMesh(component.gameObject, comp.sharedMesh);
            compData.entity.AddComponent(meshFilter);

            compData.properties.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compData.properties.SetBool("_receiveShadows", comp.receiveShadows);
            compData.SetMaterials(component.gameObject, comp.sharedMaterials, false, true);  
        }
    }
}
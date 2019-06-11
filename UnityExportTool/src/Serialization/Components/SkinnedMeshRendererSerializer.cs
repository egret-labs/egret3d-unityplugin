using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class SkinnedMeshRendererSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            SkinnedMeshRenderer comp = component as SkinnedMeshRenderer;

            var meshFilter = SerializeObject.currentData.CreateComponent(SerializeClass.MeshFilter);
            meshFilter.properties.SetMesh(component.gameObject, comp.sharedMesh);
            (compData as ComponentData).entity.AddComponent(meshFilter);

            compData.properties.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compData.properties.SetBool("_receiveShadows", comp.receiveShadows);
            compData.properties.SetMaterials(component.gameObject, comp.sharedMaterials, false, true);  
        }
    }
}
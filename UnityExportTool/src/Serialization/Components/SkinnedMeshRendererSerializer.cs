using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class SkinnedMeshRendererSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            SkinnedMeshRenderer comp = component as SkinnedMeshRenderer;

            var meshFilter = ComponentData.Create(SerializeClass.MeshFilter);
            meshFilter.SetMesh(component.gameObject, comp.sharedMesh);
            compData.entity.AddComponent(meshFilter);
            
            compData.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compData.SetBool("_receiveShadows", comp.receiveShadows);
            compData.SetMaterials(component.gameObject, comp.sharedMaterials, false, true);  

            return true;
        }
    }
}
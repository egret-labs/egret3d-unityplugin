using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class MeshRendererSerializer : ComponentSerializer
    {
        public override void Serialize(Component component, ComponentData compData)
        {
            MeshRenderer comp = component as MeshRenderer;
            compData.properties.SetBool("isStatic", component.gameObject.isStatic);
            compData.properties.SetInt("lightmapIndex", comp.lightmapIndex);
            compData.properties.SetVector4("lightmapScaleOffset", comp.lightmapScaleOffset, null, 8);
            compData.properties.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compData.properties.SetBool("_receiveShadows", comp.receiveShadows);
            compData.SetMaterials(component.gameObject, comp.sharedMaterials);
        }
    }
}
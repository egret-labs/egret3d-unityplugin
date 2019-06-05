using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class MeshRendererSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            MeshRenderer comp = component as MeshRenderer;
            compData.SetBool("isStatic", component.gameObject.isStatic);
            compData.SetInt("lightmapIndex", comp.lightmapIndex);
            compData.SetVector4("lightmapScaleOffset", comp.lightmapScaleOffset, 8);
            compData.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compData.SetBool("_receiveShadows", comp.receiveShadows);
            compData.SetMaterials(component.gameObject, comp.sharedMaterials);

            return true;
        }
    }
}
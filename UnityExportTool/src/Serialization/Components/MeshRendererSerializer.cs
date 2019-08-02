using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class MeshRendererSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            MeshRenderer comp = component as MeshRenderer;
            compData.properties.SetBool("isStatic", component.gameObject.isStatic);
            
            var sceneSetting = ExportSetting.instance.scene;
            if(sceneSetting.lightmap)
            {
                compData.properties.SetInt("lightmapIndex", comp.lightmapIndex);
                compData.properties.SetVector4("lightmapScaleOffset", comp.lightmapScaleOffset, null, 8);
            }            
            compData.properties.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compData.properties.SetBool("_receiveShadows", comp.receiveShadows);
            compData.properties.SetMaterials(component.gameObject, comp.sharedMaterials);
        }
    }
}
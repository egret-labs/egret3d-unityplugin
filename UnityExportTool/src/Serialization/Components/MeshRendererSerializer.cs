using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class MeshRendererSerializer : ComponentSerializer
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson, MyJson_Object entityJson)
        {
            MeshRenderer comp = component as MeshRenderer;
            compJson.SetBool("isStatic", obj.isStatic);
            compJson.SetInt("lightmapIndex", comp.lightmapIndex);
            compJson.SetVector4("lightmapScaleOffset", comp.lightmapScaleOffset, 8);
            compJson.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compJson.SetBool("_receiveShadows", comp.receiveShadows);
            compJson.SetMaterials(obj, comp.sharedMaterials);
            return true;
        }
    }
}
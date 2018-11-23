using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class MeshRendererParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            MeshRenderer comp = component as MeshRenderer;
            compJson.SetInt("_lightmapIndex", comp.lightmapIndex);
            compJson.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compJson.SetBool("_receiveShadows", comp.receiveShadows);
            compJson.SetVector4("_lightmapScaleOffset", comp.lightmapScaleOffset, 8);
            compJson.SetMaterials(obj, comp.sharedMaterials);

            return true;
        }
    }
}
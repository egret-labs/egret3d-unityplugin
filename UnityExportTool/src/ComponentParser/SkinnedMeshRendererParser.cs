using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class SkinnedMeshRendererParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            SkinnedMeshRenderer comp = component as SkinnedMeshRenderer;
            compJson.SetBool("_castShadows", comp.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off);
            compJson.SetBool("_receiveShadows", comp.receiveShadows);
            compJson.SetMesh(obj, comp.sharedMesh);
            compJson.SetMaterials(obj, comp.sharedMaterials, false, true);

            return true;
        }
    }
}
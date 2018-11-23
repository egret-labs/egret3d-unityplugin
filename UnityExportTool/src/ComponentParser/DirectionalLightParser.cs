using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class DirectionalLightParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            Light comp = component as Light;
            if(comp.type != LightType.Directional)
            {
                return false;
            }

            compJson.SetBool("castShadows", comp.shadows != LightShadows.None);
            compJson.SetColor("color", comp.color);
            compJson.SetNumber("intensity", comp.intensity);
            // compJson.SetNumber("shadowBias", comp.shadowBias);

            return true;
        }
    }
}

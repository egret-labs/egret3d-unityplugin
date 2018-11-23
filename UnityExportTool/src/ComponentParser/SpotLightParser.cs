using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class SpotLightParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            Light comp = component as Light;
            if (comp.type != LightType.Spot)
            {
                return false;
            }
            
            compJson.SetBool("castShadows", comp.shadows != LightShadows.None);
            compJson.SetColor("color", comp.color);
            compJson.SetNumber("intensity", comp.intensity);
            // compJson.SetNumber("shadowBias", comp.shadowBias);
            compJson.SetNumber("distance", comp.range);
            compJson.SetNumber("angle", comp.spotAngle * Math.PI / 180.0f);

            return true;
        }
    }
}

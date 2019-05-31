using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class SpotLightSerializer : DirectionalLightSerializer
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson, MyJson_Object entityJson)
        {
            Light comp = component as Light;
            if (comp.type != LightType.Spot)
            {
                return false;
            }

            base.WriteToJson(obj, component, compJson, entityJson);
            
            compJson.SetNumber("distance", comp.range);
            compJson.SetNumber("angle", comp.spotAngle * Math.PI / 180.0f);

            return true;
        }
    }
}

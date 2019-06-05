using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class SpotLightSerializer : DirectionalLightSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            Light comp = component as Light;
            if (comp.type != LightType.Spot)
            {
                return false;
            }

            base.Serialize(component, compData);
            compData.SetNumber("distance", comp.range);
            compData.SetNumber("angle", comp.spotAngle * Math.PI / 180.0f);

            return true;
        }
    }
}

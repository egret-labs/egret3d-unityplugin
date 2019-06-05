using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class DirectionalLightSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            Light comp = component as Light;
            if (comp.type != LightType.Directional)
            {
                return false;
            }

            compData.SetBool("castShadows", comp.shadows != LightShadows.None);
            compData.SetNumber("intensity", comp.intensity);
            compData.SetColor("color", comp.color);

            if (comp.shadows != LightShadows.None)
            {
                //
                var shadow = ComponentData.Create(SerializeClass.LightShadow);
                shadow.SetNumber("radius", comp.shadowNormalBias); // TODO
                shadow.SetNumber("bias", comp.shadowBias);
                shadow.SetNumber("near", comp.shadowNearPlane);
                shadow.SetNumber("far", 500.0f);
                shadow.SetNumber("size", comp.cookieSize);
                // shadow.SetNumber("mapSize", comp.shadowResolution);
                compData.entity.AddComponent(shadow);
            }
            return true;
        }
    }
}

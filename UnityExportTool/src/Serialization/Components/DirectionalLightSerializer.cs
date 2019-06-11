using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class DirectionalLightSerializer : ComponentSerializer
    {
        protected override bool Match(Component component)
        {
            Light comp = component as Light;
            if (comp.type != LightType.Directional)
            {
                return false;
            }

            return true;
        }
        protected override void Serialize(Component component, ComponentData compData)
        {
            Light comp = component as Light;
            compData.properties.SetBool("castShadows", comp.shadows != LightShadows.None);
            compData.properties.SetNumber("intensity", comp.intensity);
            compData.properties.SetColor("color", comp.color);

            if (comp.shadows != LightShadows.None)
            {
                //
                var shadow = SerializeObject.currentData.CreateComponent(SerializeClass.LightShadow);
                shadow.properties.SetNumber("radius", comp.shadowNormalBias); // TODO
                shadow.properties.SetNumber("bias", comp.shadowBias);
                shadow.properties.SetNumber("near", comp.shadowNearPlane);
                shadow.properties.SetNumber("far", 500.0f);
                shadow.properties.SetNumber("size", comp.cookieSize);
                // shadow.SetNumber("mapSize", comp.shadowResolution);
                (compData as ComponentData).entity.AddComponent(shadow);
            }
        }
    }
}

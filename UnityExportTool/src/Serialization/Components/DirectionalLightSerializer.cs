using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Egret3DExportTools
{
    public class DirectionalLightSerializer : ComponentSerializer
    {
        protected override bool Match(Component component)
        {
            Light comp = component as Light;            
            return comp.type == LightType.Directional;
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
                shadow.properties.SetNumber("radius", 1.0f); // TODO
                shadow.properties.SetNumber("bias", -0.0001f);
                shadow.properties.SetNumber("near", comp.shadowNearPlane);
                shadow.properties.SetNumber("far", 500.0f);
                shadow.properties.SetNumber("size", comp.cookieSize);
                shadow.properties.SetEnum("quality", comp.shadowResolution);

                (compData as ComponentData).entity.AddComponent(shadow);
            }
        }
    }
}

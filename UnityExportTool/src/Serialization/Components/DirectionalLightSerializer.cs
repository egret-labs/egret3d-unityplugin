using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class DirectionalLightSerializer : ComponentSerializer
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson, MyJson_Object entityJson)
        {
            Light comp = component as Light;
            if (comp.type != LightType.Directional)
            {
                return false;
            }

            compJson.SetBool("castShadows", comp.shadows != LightShadows.None);
            compJson.SetNumber("intensity", comp.intensity);
            compJson.SetColor("color", comp.color);

            if (comp.shadows != LightShadows.None)
            {
                var shadow = new MyJson_Object();
                shadow.SetSerializeClass(shadow.GetHashCode(), SerializeClass.LightShadow);
                shadow.SetNumber("radius", comp.shadowNormalBias); // TODO
                shadow.SetNumber("bias", comp.shadowBias);
                shadow.SetNumber("near", comp.shadowNearPlane);
                shadow.SetNumber("far", 500.0f);
                shadow.SetNumber("size", comp.cookieSize);
                // shadow.SetNumber("mapSize", comp.shadowResolution);

                //
                (entityJson["components"] as MyJson_Array).AddHashCode(shadow);
                ResourceManager.instance.AddCompJson(shadow);
            }

            return true;
        }
    }
}

using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class CameraSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            Camera comp = component as Camera;
            if (comp.orthographic)
            {
                compData.properties.SetNumber("size", 2 * comp.orthographicSize);//half-size?
                compData.properties.SetInt("opvalue", 0);
            }
            else
            {
                compData.properties.SetNumber("fov", comp.fieldOfView / 57.3, 4);
                compData.properties.SetInt("opvalue", 1);
            }
            compData.properties.SetNumber("near", comp.nearClipPlane);
            compData.properties.SetNumber("far", comp.farClipPlane);
            compData.properties.SetInt("cullingMask", comp.cullingMask);

            //clearFlags
            var clearFlags = comp.clearFlags;
            if(clearFlags == CameraClearFlags.SolidColor || clearFlags == CameraClearFlags.Skybox)
            {
                compData.properties.SetInt("bufferMask", 16640);
            }
            else if(clearFlags == CameraClearFlags.Depth)
            {
                compData.properties.SetInt("bufferMask", 256);
            }
            else
            {
                compData.properties.SetInt("bufferMask", 0);
            }

            //backgroundColor
            compData.properties.SetColor("backgroundColor", comp.backgroundColor);
            //viewport
            compData.properties.SetRect("viewport", comp.rect);
            //order
            compData.properties.SetNumber("order", comp.depth);
        }
    }
}


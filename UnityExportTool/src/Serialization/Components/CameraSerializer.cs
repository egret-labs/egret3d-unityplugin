using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class CameraSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            Camera comp = component as Camera;
            if (comp.orthographic)
            {
                compData.SetNumber("size", 2 * comp.orthographicSize);//half-size?
                compData.SetInt("opvalue", 0);
            }
            else
            {
                compData.SetNumber("fov", comp.fieldOfView / 57.3, 4);
                compData.SetInt("opvalue", 1);
            }
            compData.SetNumber("near", comp.nearClipPlane);
            compData.SetNumber("far", comp.farClipPlane);
            compData.SetInt("cullingMask", comp.cullingMask);

            //clearFlags
            var clearFlags = comp.clearFlags;
            if(clearFlags == CameraClearFlags.SolidColor || clearFlags == CameraClearFlags.Skybox)
            {
                compData.SetInt("bufferMask", 16640);
            }
            else if(clearFlags == CameraClearFlags.Depth)
            {
                compData.SetInt("bufferMask", 256);
            }
            else
            {
                compData.SetInt("bufferMask", 0);
            }

            //backgroundColor
            compData.SetColor("backgroundColor", comp.backgroundColor);
            //viewport
            compData.SetRect("viewport", comp.rect);
            //order
            compData.SetNumber("order", comp.depth);
            return true;
        }
    }
}


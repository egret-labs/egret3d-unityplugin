using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class CameraSerializer : ComponentSerializer
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson, MyJson_Object entityJson)
        {
            Camera comp = component as Camera;

            if (comp.orthographic)
            {
                compJson.SetNumber("size", 2 * comp.orthographicSize);//half-size?
                compJson.SetInt("opvalue", 0);
            }
            else
            {
                compJson.SetNumber("fov", comp.fieldOfView / 57.3, 4);
                compJson.SetInt("opvalue", 1);
            }
            compJson.SetNumber("near", comp.nearClipPlane);
            compJson.SetNumber("far", comp.farClipPlane);
            compJson.SetInt("cullingMask", comp.cullingMask);

            //clearFlags
            var clearFlags = comp.clearFlags;
            if(clearFlags == CameraClearFlags.SolidColor || clearFlags == CameraClearFlags.Skybox)
            {
                compJson.SetInt("bufferMask", 16640);
            }
            else if(clearFlags == CameraClearFlags.Depth)
            {
                compJson.SetInt("bufferMask", 256);
            }
            else
            {
                compJson.SetInt("bufferMask", 0);
            }

            //backgroundColor
            compJson.SetColor("backgroundColor", comp.backgroundColor);
            //viewport
            compJson.SetRect("viewport", comp.rect);
            //order
            compJson.SetNumber("order", comp.depth);

            return true;
        }
    }
}


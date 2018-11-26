using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class CameraParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
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
            compJson.SetNumber("_near", comp.nearClipPlane);
            compJson.SetNumber("_far", comp.farClipPlane);
            // compJson.SetInt("cullingMask", 0xffffff);
            compJson.SetInt("cullingMask", comp.cullingMask);
            //clearFlag
            switch (comp.clearFlags)
            {
                case CameraClearFlags.Skybox:
                case CameraClearFlags.SolidColor:
                    compJson.SetBool("clearOption_Color", true);
                    compJson.SetBool("clearOption_Depth", true);
                    break;
                case CameraClearFlags.Depth:
                    compJson.SetBool("clearOption_Color", false);
                    compJson.SetBool("clearOption_Depth", true);
                    break;
                case CameraClearFlags.Nothing:
                    compJson.SetBool("clearOption_Color", false);
                    compJson.SetBool("clearOption_Depth", false);
                    break;
                default:
                    break;
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


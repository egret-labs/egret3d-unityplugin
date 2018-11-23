using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class ParticleSystemRendererParser : ComponentParser
    {
        public override bool WriteToJson(GameObject _object, Component component, MyJson_Object compJson)
        {
            ParticleSystemRenderer comp = component as ParticleSystemRenderer;
            ParticleSystem c = _object.GetComponent<ParticleSystem>();
            if (c == null || !c.emission.enabled)
            {
                MyLog.LogWarning("无效的粒子组件:" + _object.name);
                return false;
            }
            compJson.SetNumber("velocityScale", comp.velocityScale);
            compJson.SetNumber("lengthScale", comp.lengthScale);
            compJson.SetEnum("_renderMode", comp.renderMode);
            if (comp.renderMode == ParticleSystemRenderMode.Mesh && comp.mesh == null)
            {
                throw new Exception(_object.name + ": mesh 丢失");
            }
            //Mesh
            compJson.SetMesh(_object, comp.mesh);
            //Material粒子系统不支持多材质
            compJson.SetMaterials(_object, new Material[] { comp.sharedMaterial }, true);
            return true;
        }
    }
}
using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class ParticleRendererSerializer : ComponentSerializer
    {
        public override bool Serialize(Component component, ComponentData compData)
        {
            var obj = component.gameObject;
            ParticleSystemRenderer comp = component as ParticleSystemRenderer;
            ParticleSystem c = obj.GetComponent<ParticleSystem>();
            if (c == null || !c.emission.enabled)
            {
                MyLog.LogWarning("无效的粒子组件:" + obj.name);
                return false;
            }
            compData.SetNumber("velocityScale", comp.velocityScale);
            compData.SetNumber("lengthScale", comp.lengthScale);
            compData.SetEnum("_renderMode", comp.renderMode);
            if (comp.renderMode == ParticleSystemRenderMode.Mesh && comp.mesh == null)
            {
                throw new Exception(obj.name + ": mesh 丢失");
            }
            //Mesh
            compData.SetMesh(obj, comp.mesh);
            //Material粒子系统不支持多材质
            compData.SetMaterials(obj, new Material[] { comp.sharedMaterial }, true);

            return true;
        }
    }
}
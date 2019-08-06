using System;
using UnityEngine;

namespace Egret3DExportTools
{
    public class ParticleRendererSerializer : ComponentSerializer
    {
        protected override bool Match(Component component)
        {
            var obj = component.gameObject;
            ParticleSystemRenderer comp = component as ParticleSystemRenderer;
            ParticleSystem c = obj.GetComponent<ParticleSystem>();
            if (c == null || !c.emission.enabled)
            {
                MyLog.LogWarning("无效的粒子组件:" + obj.name);
                return false;
            }
            if (comp.renderMode == ParticleSystemRenderMode.Mesh && comp.mesh == null)
            {
                MyLog.LogWarning(obj.name + ": mesh 丢失");
                return false;
            }

            return true;
        }
        protected override void Serialize(Component component, ComponentData compData)
        {
            var obj = component.gameObject;
            ParticleSystemRenderer comp = component as ParticleSystemRenderer;
            ParticleSystem c = obj.GetComponent<ParticleSystem>();
            compData.properties.SetNumber("velocityScale", comp.velocityScale);
            compData.properties.SetNumber("lengthScale", comp.lengthScale);
            compData.properties.SetEnum("renderMode", comp.renderMode);
            //Mesh
            compData.properties.SetMesh(obj, comp.mesh);
            //Material粒子系统不支持多材质
            compData.properties.SetMaterials(obj, new Material[] { comp.sharedMaterial }, true);
        }
    }
}
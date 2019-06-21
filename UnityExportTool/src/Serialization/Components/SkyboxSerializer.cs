using UnityEngine;

namespace Egret3DExportTools
{
    public class SkyboxSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            Skybox comp = component as Skybox;
            base.Serialize(component, compData);

            var mats = new Material[1] { comp.material };
            compData.properties.SetMaterials(component.gameObject, mats);
        }
    }
}

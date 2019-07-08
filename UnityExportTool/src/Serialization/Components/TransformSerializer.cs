using UnityEngine;

namespace Egret3DExportTools
{
    public class TransformSerializer : ComponentSerializer
    {
        protected override void Serialize(Component component, ComponentData compData)
        {
            var obj = component.gameObject;
            Transform comp = component as Transform;

            Vector3 localPosition = comp.localPosition;
            Quaternion localRotation = comp.localRotation;
            Vector3 localScale = comp.localScale;

            compData.properties.SetBool("isStatic", obj.isStatic);
            //localPosition
            compData.properties.SetVector3("_localPosition", localPosition);
            //localRotation
            compData.properties.SetQuaternion("_localRotation", localRotation);
            //localScale
            compData.properties.SetVector3("_localScale", localScale);            

            //
            var treeNode = SerializeObject.currentData.CreateComponent(SerializeClass.TreeNode);
            treeNode.properties.SetString("name", obj.name);
            treeNode.properties.SetString("tag", obj.tag);
            treeNode.properties.SetInt("layer", 1 << obj.layer);
            if (comp.childCount > 0)
            {
                for (int i = 0; i < comp.childCount; i++)
                {
                    var child = comp.GetChild(i).gameObject;
                    if (child.gameObject.activeInHierarchy)
                    {
                        var childEntity = SerializeObject.SerializeEntity(child);
                        treeNode.AddChild(childEntity.treeNode);
                    }
                }
            }

            compData.entity.AddComponent(treeNode);

        }
    }
}

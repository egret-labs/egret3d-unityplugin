namespace Egret3DExportTools
{
    using System;
    using UnityEngine;
    using UnityEditor;
    public class CustomMaterialWriter : BaseMaterialWriter
    {
        protected override void Update()
        {
            //自定义的value全部导出，不和默认值做过滤
            var target = this.source;
            var materialProperties = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { target });
            foreach (var materialProperty in materialProperties)
            {
                if (materialProperty.flags == MaterialProperty.PropFlags.HideInInspector)
                {
                    continue;
                }

                var uniform = new MyJson_Tree();
                string type = materialProperty.type.ToString();
                if (type == "Float" || type == "Range")
                {
                    this.values.SetNumber(materialProperty.name, this.GetFloat(materialProperty.name, 0.0f));
                }
                else if (type == "Vector")
                {
                    this.values.SetVector4(materialProperty.name, this.GetVector4(materialProperty.name, Vector4.zero));
                }
                else if (type == "Color")
                {
                    this.values.SetColor(materialProperty.name, this.GetColor(materialProperty.name, Color.white));
                }
                else if (type == "Texture")
                {
                    var tex = this.GetTexture(materialProperty.name, null);
                    if (tex != null)
                    {
                        string texdim = materialProperty.textureDimension.ToString();
                        if (texdim == "Tex2D")
                        {
                            this.SetTexture(materialProperty.name, tex);

                            string propertyName = materialProperty.name + "_ST";
                            if (target.HasProperty(propertyName))
                            {
                                this.values.SetVector4(propertyName, this.GetVector4(propertyName, Vector4.zero));
                            }
                        }
                        else
                        {
                            throw new Exception("not suport texdim:" + texdim);
                        }
                    }
                }
                else
                {
                    throw new Exception("not support type: " + materialProperty.type);
                }
            }

            MyLog.Log("自定义Shader:" + this.technique);
        }

        protected override string technique
        {
            get
            {
                return UnityEditor.AssetDatabase.GetAssetPath(this.source.shader) + ".json";
            }
        }
    }
}
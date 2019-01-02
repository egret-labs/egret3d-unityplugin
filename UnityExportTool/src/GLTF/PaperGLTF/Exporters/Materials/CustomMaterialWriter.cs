namespace PaperGLTF
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using Egret3DExportTools;
    public class CustomMaterialWriter : BaseMaterialWriter
    {
        protected override void Update()
        {
            var target = this.source;
            var shaderName = target.shader.name;
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
                    string texdim = materialProperty.textureDimension.ToString();
                    var tex = this.GetTexture(materialProperty.name, null);
                    if (tex != null)
                    {
                        if (texdim == "Tex2D")
                        {
                            var texPath = ResourceManager.instance.SaveTexture(tex as Texture2D, "");
                            this.values.SetString(materialProperty.name, texPath);

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
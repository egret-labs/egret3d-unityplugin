namespace Egret3DExportTools
{
    using System;
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;

    public class CustomParser : BaseMaterialParser
    {
        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var values = this.data.values;
            //自定义的value全部导出，不和默认值做过滤
            var target = this.source;
            var materialProperties = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { target });
            foreach (var materialProperty in materialProperties)
            {
                if (materialProperty.flags == MaterialProperty.PropFlags.HideInInspector)
                {
                    continue;
                }

                string type = materialProperty.type.ToString();
                if (type == "Float" || type == "Range")
                {
                    this.data.values.SetNumber(materialProperty.name, this.source.GetFloat(materialProperty.name, 0.0f), null);
                    // this.SetFloat(materialProperty.name, this.GetFloat(materialProperty.name, 0.0f), null);
                }
                else if (type == "Vector")
                {
                    this.data.values.SetVector4(materialProperty.name, this.source.GetVector4(materialProperty.name, Vector4.zero));
                }
                else if (type == "Color")
                {
                    this.data.values.SetColor(materialProperty.name, this.source.GetColor(materialProperty.name, Color.white));
                }
                else if (type == "Texture")
                {
                    var tex = this.source.GetTexture(materialProperty.name, null);
                    if (tex != null)
                    {
                        string texdim = materialProperty.textureDimension.ToString();
                        if (texdim == "Tex2D")
                        {
                            this.data.values.SetTexture(materialProperty.name, tex);

                            string propertyName = materialProperty.name + "_ST";
                            if (target.HasProperty(propertyName))
                            {
                                this.data.values.SetVector4(propertyName, this.source.GetVector4(propertyName, Vector4.zero));
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

            //
            this.shaderAsset = UnityEditor.AssetDatabase.GetAssetPath(this.source.shader) + ".json";
            MyLog.Log("自定义Shader:" + this.shaderAsset);
        }
    }
}
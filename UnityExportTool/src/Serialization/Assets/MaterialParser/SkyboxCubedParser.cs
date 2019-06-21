namespace Egret3DExportTools
{
    using UnityEngine;
    public class SkyboxCubedParser : BaseMaterialParser
    {
        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var source = this.source;
            var values = this.data.values;
            
            if(source.HasProperty("_Tex"))
            {
                values.SetCubemap("tCube", source.GetTexture("_Tex") as Cubemap);
            }
            else
            {
                MyLog.LogWarning("请检查材质" + source.name + "Cubemap属性是否有值");
            }            
        }
    }
}
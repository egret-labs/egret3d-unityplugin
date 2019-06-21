namespace Egret3DExportTools
{
    using UnityEngine;
    public class Texture2DArrayData : UnityEngine.Object
    {
        public string materialName;
        public Texture2D[] textures;
    }
    public class SkyboxParser : BaseMaterialParser
    {
        public override void CollectUniformValues()
        {
            base.CollectUniformValues();
            var source = this.source;
            var values = this.data.values;

            var frontTex = source.GetTexture("_FrontTex") as Texture2D;
            var backTex = source.GetTexture("_BackTex") as Texture2D;
            var leftTex = source.GetTexture("_LeftTex") as Texture2D;
            var rightTex = source.GetTexture("_RightTex") as Texture2D;
            var upTex = source.GetTexture("_UpTex") as Texture2D;
            var downTex = source.GetTexture("_DownTex") as Texture2D;

            /**
             *XxYyZz
             */
            var textureArr = new Texture2DArrayData();
            textureArr.textures = new Texture2D[6];
            textureArr.textures[0] = leftTex;
            textureArr.textures[1] = rightTex;
            textureArr.textures[2] = upTex;
            textureArr.textures[3] = downTex;
            textureArr.textures[4] = frontTex;
            textureArr.textures[5] = backTex;
            textureArr.materialName = source.name;

            Debug.Log("materialName:" + textureArr.materialName);

            this.data.values.SetTextureArray("tCube", textureArr);
        }

        public override void CollectStates()
        {
            var customConfig = ExportSetting.instance.GetCustomShader(this.shaderName);
            var isDoubleSide = this.isDoubleSide;
            var isTransparent = this.isTransparent;

            var blend = this.blendMode;
            // this.SetBlend(this.data.technique.states.enable, this.data.technique.states.functions, blend);
            this.SetCull(this.data.technique.states.enable, this.data.technique.states.functions, true, FrontFace.CCW, CullFace.FRONT);
            this.SetDepth(this.data.technique.states.enable, this.data.technique.states.functions, false, false);
        }
    }
}
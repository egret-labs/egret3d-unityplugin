using UnityEngine;
using System;

namespace Egret3DExportTools
{
    public class ParticleSystemParser : ComponentParser
    {
        public override bool WriteToJson(GameObject obj, Component component, MyJson_Object compJson)
        {
            ParticleSystem comp = component as ParticleSystem;
            if (!comp.emission.enabled || obj.GetComponent<ParticleSystemRenderer>() == null)
            {
                MyLog.LogWarning("无效的粒子组件:" + obj.name);
                return false;
            }

            //main
            {
                var main = comp.main;
                var mainItem = new MyJson_Tree(false);
                compJson["main"] = mainItem;
                mainItem.SetNumber("duration", main.duration);
                mainItem.SetBool("loop", main.loop);
                this.AddMinMaxCurve(mainItem, "startDelay", main.startDelay);
                this.AddMinMaxCurve(mainItem, "startLifetime", main.startLifetime);
                this.AddMinMaxCurve(mainItem, "startSpeed", main.startSpeed);

                mainItem.SetBool("startSize3D", main.startSize3D);
                if (main.startSize3D)
                {
                    this.AddMinMaxCurve(mainItem, "startSizeX", main.startSizeX);
                    this.AddMinMaxCurve(mainItem, "startSizeY", main.startSizeY);
                    this.AddMinMaxCurve(mainItem, "startSizeZ", main.startSizeZ);
                }
                else
                {
                    this.AddMinMaxCurve(mainItem, "startSizeX", main.startSize);
                    this.AddMinMaxCurve(mainItem, "startSizeY", main.startSize);
                    this.AddMinMaxCurve(mainItem, "startSizeZ", main.startSize);
                }

                mainItem.SetBool("_startRotation3D", main.startRotation3D);
                if (main.startRotation3D)
                {
                    this.AddMinMaxCurve(mainItem, "startRotationX", main.startRotationX);
                    this.AddMinMaxCurve(mainItem, "startRotationY", main.startRotationY);
                    this.AddMinMaxCurve(mainItem, "startRotationZ", main.startRotationZ);
                }
                else
                {
                    this.AddMinMaxCurve(mainItem, "startRotationX", main.startRotation);
                    this.AddMinMaxCurve(mainItem, "startRotationY", main.startRotation);
                    this.AddMinMaxCurve(mainItem, "startRotationZ", main.startRotation);
                }

                this.AddMinMaxGradient(mainItem, "startColor", main.startColor);
                this.AddMinMaxCurve(mainItem, "gravityModifier", main.gravityModifier);
                mainItem.SetEnum("_simulationSpace", main.simulationSpace);
                mainItem.SetEnum("scaleMode", main.scalingMode);
                mainItem.SetBool("playOnAwake", main.playOnAwake);
                if (ExportToolsSetting.instance.estimateMaxParticles)
                {
                    var value = this.EstimateMaxParticles(comp);
                    mainItem.SetInt("_maxParticles", value);
                    MyLog.Log(comp.gameObject.name + " 粒子估算:" + value);
                }
                else
                {
                    mainItem.SetInt("_maxParticles", main.maxParticles);
                }
            }

            //emission
            {
                var emissionItem = new MyJson_Tree(false);
                compJson["emission"] = emissionItem;
                this.AddMinMaxCurve(emissionItem, "rateOverTime", comp.emission.rateOverTime);
                emissionItem["bursts"] = new MyJson_Array();
                var bursts = new ParticleSystem.Burst[comp.emission.burstCount];
                comp.emission.GetBursts(bursts);
                foreach (var burst in bursts)
                {
                    MyJson_Array burstItem = new MyJson_Array();
                    burstItem.AddNumber(burst.time);
                    burstItem.AddInt(burst.minCount);
                    burstItem.AddInt(burst.maxCount);
                    burstItem.AddInt(burst.cycleCount);
                    burstItem.AddNumber(burst.repeatInterval);
                    (emissionItem["bursts"] as MyJson_Array).Add(burstItem);
                }
            }
            //shape
            if (comp.shape.enabled)
            {
                var shapItem = new MyJson_Tree(false);
                compJson["shape"] = shapItem;
                shapItem.SetEnum("shapeType", comp.shape.shapeType);
                shapItem.SetNumber("angle", comp.shape.angle);
                shapItem.SetNumber("length", comp.shape.length);
                shapItem.SetEnum("arcMode", comp.shape.arcMode);
                shapItem.SetNumber("arc", comp.shape.arc);
                shapItem.SetNumber("arcSpread", comp.shape.arcSpread);
                shapItem.SetEnum("radiusMode", comp.shape.radiusMode);
                shapItem.SetNumber("radius", comp.shape.radius);
                shapItem.SetNumber("radiusSpread", comp.shape.radiusSpread);
                shapItem.SetVector3("box", comp.shape.box);
                shapItem.SetBool("randomDirection", comp.shape.randomDirectionAmount > 0);
                shapItem.SetBool("spherizeDirection", comp.shape.sphericalDirectionAmount > 0);
                this.AddMinMaxCurve(shapItem, "arcSpeed", comp.shape.arcSpeed);
            }
            //velocityOverLifetiem
            if (comp.velocityOverLifetime.enabled)
            {
                var velocityOverItem = new MyJson_Tree(false);
                compJson["velocityOverLifetime"] = velocityOverItem;
                velocityOverItem.SetEnum("_mode", comp.velocityOverLifetime.x.mode);
                velocityOverItem.SetEnum("_space", comp.velocityOverLifetime.space);
                this.AddMinMaxCurve(velocityOverItem, "_x", comp.velocityOverLifetime.x);
                this.AddMinMaxCurve(velocityOverItem, "_y", comp.velocityOverLifetime.y);
                this.AddMinMaxCurve(velocityOverItem, "_z", comp.velocityOverLifetime.z);
            }
            //colorOverLifetime
            if (comp.colorOverLifetime.enabled)
            {
                var colorOverItem = new MyJson_Tree(false);
                compJson["colorOverLifetime"] = colorOverItem;
                this.AddMinMaxGradient(colorOverItem, "_color", comp.colorOverLifetime.color);
            }
            //sizeOverLifetime
            if (comp.sizeOverLifetime.enabled)
            {
                var sizeOverItem = new MyJson_Tree(false);
                compJson["sizeOverLifetime"] = sizeOverItem;
                sizeOverItem.SetBool("_separateAxes", comp.sizeOverLifetime.separateAxes);
                this.AddMinMaxCurve(sizeOverItem, "_size", comp.sizeOverLifetime.size);
                this.AddMinMaxCurve(sizeOverItem, "_x", comp.sizeOverLifetime.x);
                this.AddMinMaxCurve(sizeOverItem, "_y", comp.sizeOverLifetime.y);
                this.AddMinMaxCurve(sizeOverItem, "_z", comp.sizeOverLifetime.z);
            }
            //rotationOverLifetime
            if (comp.rotationOverLifetime.enabled)
            {
                var rotationOverItem = new MyJson_Tree(false);
                compJson["rotationOverLifetime"] = rotationOverItem;
                rotationOverItem.SetBool("_separateAxes", comp.rotationOverLifetime.separateAxes);
                this.AddMinMaxCurve(rotationOverItem, "_x", comp.rotationOverLifetime.x);
                this.AddMinMaxCurve(rotationOverItem, "_y", comp.rotationOverLifetime.y);
                this.AddMinMaxCurve(rotationOverItem, "_z", comp.rotationOverLifetime.z);
            }
            //textureSheetAnimationModule
            if (comp.textureSheetAnimation.enabled)
            {
                var textureSheetAnimation = new MyJson_Tree(false);
                compJson["textureSheetAnimation"] = textureSheetAnimation;
                textureSheetAnimation.SetInt("_numTilesX", comp.textureSheetAnimation.numTilesX);
                textureSheetAnimation.SetInt("_numTilesY", comp.textureSheetAnimation.numTilesY);
                textureSheetAnimation.SetEnum("_animation", comp.textureSheetAnimation.animation);
                textureSheetAnimation.SetBool("_useRandomRow", comp.textureSheetAnimation.useRandomRow);
                textureSheetAnimation.SetInt("_cycleCount", comp.textureSheetAnimation.cycleCount);
                textureSheetAnimation.SetInt("_rowIndex", comp.textureSheetAnimation.rowIndex);
                this.AddMinMaxCurve(textureSheetAnimation, "_frameOverTime", comp.textureSheetAnimation.frameOverTime, comp.textureSheetAnimation.numTilesX * comp.textureSheetAnimation.numTilesY);
                this.AddMinMaxCurve(textureSheetAnimation, "_startFrame", comp.textureSheetAnimation.startFrame);
            }

            return true;
        }
        /**
        *获取生命周期内爆发的粒子总数
        */
        private int GetBurstTotal(ParticleSystem comp)
        {
            int total = 0;
            var bursts = new ParticleSystem.Burst[comp.emission.burstCount];
            comp.emission.GetBursts(bursts);
            foreach (var burst in bursts)
            {
                total += burst.maxCount;
            }

            return total;
        }
        /**
        *获取曲线上最大值
        */
        private float GetCureMax(ParticleSystem.MinMaxCurve curve)
        {
            float max = 0;
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    {
                        max = curve.constant;
                        break;
                    }
                case ParticleSystemCurveMode.TwoConstants:
                    {
                        max = curve.constantMax;
                        break;
                    }
                case ParticleSystemCurveMode.Curve:
                    {
                        foreach (var keyFrame in curve.curve.keys)
                        {
                            if (keyFrame.value > max)
                            {
                                max = keyFrame.value * curve.curveMultiplier;
                            }
                        }
                        break;
                    }
                case ParticleSystemCurveMode.TwoCurves:
                    {
                        foreach (var keyFrame in curve.curveMax.keys)
                        {
                            if (keyFrame.value > max)
                            {
                                max = keyFrame.value * curve.curveMultiplier;
                            }
                        }
                        break;
                    }
            }

            return max;
        }

        /**
        *估算一下最大粒子数
        */
        private int EstimateMaxParticles(ParticleSystem comp)
        {
            var main = comp.main;
            if (main.maxParticles == 1)
            {
                return main.maxParticles;
            }
            var emission = comp.emission;
            var rateOverTimeMaxCount = this.GetCureMax(emission.rateOverTime);
            var maxLifetime = this.GetCureMax(main.startLifetime);
            var burstTotal = this.GetBurstTotal(comp);
            //谁小用谁
            var estimateValue = 0;
            if (main.loop || main.duration >= maxLifetime)
            {
                //每秒产生的最大粒子数 * 最大存活时间 + 生命周期内爆发的最大粒子数
                estimateValue = Mathf.CeilToInt(rateOverTimeMaxCount * maxLifetime + burstTotal);
            }
            else
            {
                //每秒产生的最大粒子数 * 最大存活时间 + 生命周期内爆发的最大粒子数
                estimateValue = Mathf.CeilToInt(rateOverTimeMaxCount * main.duration + burstTotal);
            }

            return main.maxParticles > estimateValue ? estimateValue : main.maxParticles;
        }

        protected void AddMinMaxCurve(MyJson_Tree compItem, string key, ParticleSystem.MinMaxCurve curve, float scale = 1.0f)
        {
            MyJson_Tree curveItem = new MyJson_Tree(false);
            curveItem.SetEnum("mode", curve.mode);
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    curveItem.SetNumber("constant", curve.constant);
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    curveItem.SetNumber("constantMin", curve.constantMin);
                    curveItem.SetNumber("constantMax", curve.constantMax);
                    break;
                case ParticleSystemCurveMode.Curve:
                    this.AddCurve(curveItem, "curve", FillCurve(curve.curve, curve.curveMultiplier * scale));
                    break;
                case ParticleSystemCurveMode.TwoCurves:
                    this.AddCurve(curveItem, "curveMin", FillCurve(curve.curveMin, curve.curveMultiplier * scale));
                    this.AddCurve(curveItem, "curveMax", FillCurve(curve.curveMax, curve.curveMultiplier * scale));
                    break;
            }

            compItem[key] = curveItem;
        }

        protected void AddMinMaxGradient(MyJson_Tree compItem, string key, ParticleSystem.MinMaxGradient gradient)
        {
            MyJson_Tree gradientItem = new MyJson_Tree(false);
            gradientItem.SetEnum("mode", gradient.mode);

            switch (gradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    gradientItem.SetColor("color", gradient.color);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    gradientItem.SetColor("colorMin", gradient.colorMin);
                    gradientItem.SetColor("colorMax", gradient.colorMax);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    this.AddGradient(gradientItem, "gradient", gradient.gradient);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    this.AddGradient(gradientItem, "gradientMin", gradient.gradientMin);
                    this.AddGradient(gradientItem, "gradientMax", gradient.gradientMax);
                    break;
            }

            compItem[key] = gradientItem;
        }

        protected void AddCurve(MyJson_Tree curveItem, string key, Keyframe[] keys)
        {
            MyJson_Array frmes = new MyJson_Array();
            foreach (Keyframe k in keys)
            {
                MyJson_Array keyItem = new MyJson_Array();
                keyItem.AddNumber(k.time);
                keyItem.AddNumber(k.value);
                frmes.Add(keyItem);
            }
            curveItem[key] = frmes;
        }

        protected Keyframe[] FillCurve(AnimationCurve curve, float scale)
        {
            var keys = curve.keys;
            Keyframe[] res = new Keyframe[4];
            var curveLen = keys.Length;
            if (curveLen > 4)
            {
                res[0] = keys[0];
                res[1] = keys[1];
                res[2] = keys[2];
                res[3] = keys[curveLen - 1];
            }
            else if (curveLen < 4)
            {
                //过渡模式不会只有一个的情况
                var time = 0.0f;
                for (int i = 0; i < curveLen - 1; i++)
                {
                    time = keys[i].time;
                    res[i] = keys[i];
                    res[i].value = curve.Evaluate(time) * scale;
                }

                var avgTime = (1.0f - time) / (4.0f - curveLen + 1.0f);
                for (int i = curveLen - 1; i < 4; i++)
                {
                    time += avgTime;
                    time = Mathf.Min(1.0f, time);
                    var key = new Keyframe();
                    key.time = time;
                    key.value = curve.Evaluate(key.time) * scale;
                    res[i] = key;
                }

                res[3] = keys[curveLen - 1];
                res[3].value = curve.Evaluate(1.0f) * scale;
            }
            else
            {
                res = keys;
            }

            return res;
        }

        protected GradientAlphaKey[] FillGradientAlpha(Gradient gradient)
        {
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
            GradientAlphaKey[] res = new GradientAlphaKey[4];
            var alphaKeysLen = alphaKeys.Length;
            if (alphaKeysLen > 4)
            {
                res[0] = alphaKeys[0];
                res[1] = alphaKeys[1];
                res[2] = alphaKeys[2];
                res[3] = alphaKeys[alphaKeysLen - 1];
            }
            else if (alphaKeysLen < 4)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i < alphaKeysLen)
                    {
                        res[i] = alphaKeys[i];
                    }
                    else
                    {
                        res[i] = alphaKeys[alphaKeysLen - 1];
                    }
                }

                GradientAlphaKey finalFrame = new GradientAlphaKey();
                finalFrame.time = 1.0f;
                finalFrame.alpha = gradient.Evaluate(finalFrame.time).a;
                res[3] = finalFrame;

            }
            else
            {
                res = alphaKeys;
            }

            return res;
        }

        protected GradientColorKey[] FillGradientColor(Gradient gradient)
        {
            GradientColorKey[] colorKeys = gradient.colorKeys;
            GradientColorKey[] res = new GradientColorKey[4];
            var colorKeysLen = colorKeys.Length;
            if (colorKeysLen > 4)
            {
                res[0] = colorKeys[0];
                res[1] = colorKeys[1];
                res[2] = colorKeys[2];
                res[3] = colorKeys[colorKeysLen - 1];
            }
            else if (colorKeysLen < 4)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i < colorKeysLen)
                    {
                        res[i] = colorKeys[i];
                    }
                    else
                    {
                        res[i] = colorKeys[colorKeysLen - 1];
                    }
                }

                GradientColorKey finalFrame = new GradientColorKey();
                finalFrame.time = 1.0f;
                finalFrame.color = gradient.Evaluate(finalFrame.time);
                res[3] = finalFrame;
            }
            else
            {
                res = colorKeys;
            }

            return res;
        }

        protected void AddGradient(MyJson_Tree gradientItem, string key, Gradient gradient)
        {
            //alpha和color过渡值最大为4，如果不够手动填充,如果超过，中间的截掉
            GradientAlphaKey[] alphaKeys = this.FillGradientAlpha(gradient);
            GradientColorKey[] colorKeys = this.FillGradientColor(gradient);

            MyJson_Tree gradients = new MyJson_Tree(false);
            gradients.SetEnum("mode", gradient.mode);
            var alphaKeysItem = new MyJson_Array();
            var colorKeysItem = new MyJson_Array();
            gradients["alphaKeys"] = alphaKeysItem;
            gradients["colorKeys"] = colorKeysItem;

            //alphaKey
            foreach (GradientAlphaKey _ak in alphaKeys)
            {
                MyJson_Tree akItem = new MyJson_Tree(false);
                int akHash = akItem.GetHashCode();
                akItem.SetNumber("alpha", _ak.alpha);
                akItem.SetNumber("time", _ak.time);
                alphaKeysItem.Add(akItem);
            }

            //colorKey
            foreach (GradientColorKey _ck in colorKeys)
            {
                MyJson_Tree ckItem = new MyJson_Tree(false);
                int ckHash = ckItem.GetHashCode();
                ckItem.SetColor("color", _ck.color);
                ckItem.SetNumber("time", _ck.time);
                colorKeysItem.Add(ckItem);
            }

            gradientItem[key] = gradients;
        }
    }
}


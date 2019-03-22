namespace Egret3DExportTools
{
    using System.Collections.Generic;
    using System.IO;
    using System;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;

    public class AnimationXWriter : GLTFExporter
    {
        private Transform _target;
        private UnityEngine.AnimationClip _animationClip;
        private List<Transform> _animationTargets = new List<Transform>(); // TODO 完善 children.

        public AnimationXWriter(Transform target, UnityEngine.AnimationClip animationClip) : base()
        {
            _target = target;
            _animationClip = animationClip;
        }

        protected override void Init()
        {
            base.Init();

            _root = new GLTFRoot
            {
                Accessors = new List<Accessor>(),
                Asset = new Asset
                {
                    Version = "2.0",
                    Generator = "paper",
                },
                Buffers = new List<GLTF.Schema.Buffer>(),
                BufferViews = new List<BufferView>() {
                    new BufferView {
                        Buffer = new BufferId(){
                            Id = 0,
                            Root = _root,
                        },
                        ByteOffset = 0,
                        ByteLength = 0,
                    }
                },
                Nodes = new List<Node>(),
                Scenes = new List<Scene>() {
                    new Scene() {
                        Nodes = new List<NodeId>(),
                    }
                },
                Animations = new List<GLTF.Schema.Animation>(),
            };

            _buffer = new GLTF.Schema.Buffer();
            _bufferId = new BufferId
            {
                Id = _root.Buffers.Count,
                Root = _root
            };
            _root.Buffers.Add(_buffer);
        }

        private UnityEditor.EditorCurveBinding[] _getCurveGroup(UnityEditor.EditorCurveBinding[] curves, UnityEditor.EditorCurveBinding curve)
        {
            var result = new UnityEditor.EditorCurveBinding[4];
            foreach (var eachCurve in curves)
            {
                if (eachCurve.path == curve.path && eachCurve.type == curve.type)
                {
                    switch (curve.propertyName)
                    {
                        case "m_LocalPosition.x":
                        case "m_LocalPosition.y":
                        case "m_LocalPosition.z":
                            if (eachCurve.propertyName == "m_LocalPosition.x")
                            {
                                result[0] = eachCurve;
                            }

                            if (eachCurve.propertyName == "m_LocalPosition.y")
                            {
                                result[1] = eachCurve;
                            }

                            if (eachCurve.propertyName == "m_LocalPosition.z")
                            {
                                result[2] = eachCurve;
                            }
                            break;

                        case "m_LocalRotation.x":
                        case "m_LocalRotation.y":
                        case "m_LocalRotation.z":
                        case "m_LocalRotation.w":
                            if (eachCurve.propertyName == "m_LocalRotation.x")
                            {
                                result[0] = eachCurve;
                            }

                            if (eachCurve.propertyName == "m_LocalRotation.y")
                            {
                                result[1] = eachCurve;
                            }

                            if (eachCurve.propertyName == "m_LocalRotation.z")
                            {
                                result[2] = eachCurve;
                            }

                            if (eachCurve.propertyName == "m_LocalRotation.w")
                            {
                                result[3] = eachCurve;
                            }
                            break;

                        case "localEulerAnglesRaw.x":
                        case "localEulerAnglesRaw.y":
                        case "localEulerAnglesRaw.z":
                            if (eachCurve.propertyName == "localEulerAnglesRaw.x")
                            {
                                result[0] = eachCurve;
                            }

                            if (eachCurve.propertyName == "localEulerAnglesRaw.y")
                            {
                                result[1] = eachCurve;
                            }

                            if (eachCurve.propertyName == "localEulerAnglesRaw.z")
                            {
                                result[2] = eachCurve;
                            }
                            break;

                        case "m_LocalScale.x":
                        case "m_LocalScale.y":
                        case "m_LocalScale.z":
                            if (eachCurve.propertyName == "m_LocalScale.x")
                            {
                                result[0] = eachCurve;
                            }

                            if (eachCurve.propertyName == "m_LocalScale.y")
                            {
                                result[1] = eachCurve;
                            }

                            if (eachCurve.propertyName == "m_LocalScale.z")
                            {
                                result[2] = eachCurve;
                            }
                            break;
                    }
                }
            }

            return result;
        }

        private int _getPlayTimes(UnityEngine.AnimationClip animationClip)
        {
            if (animationClip.wrapMode == UnityEngine.WrapMode.Loop)
            {
                return 0;
            }
            else if (animationClip.wrapMode == UnityEngine.WrapMode.Default)
            {
                var setting = UnityEditor.AnimationUtility.GetAnimationClipSettings(animationClip);
                return setting.loopTime ? 0 : 1;
            }

            return 1;
        }

        public override byte[] WriteGLTF()
        {
            this.BeginWrite();

            _exportAnimation(_animationClip);

            var res = this.EndWrite();
            return res;
        }

        private void _exportAnimation(UnityEngine.AnimationClip animationClip)
        {
            //
            var frameCount = (int)Math.Floor(animationClip.length * animationClip.frameRate) + 1;
            var curveBinds = UnityEditor.AnimationUtility.GetCurveBindings(animationClip);
            var ignoreCurves = new List<UnityEditor.EditorCurveBinding>();
            var glTFAnimation = new GLTF.Schema.Animation()
            {
                Name = animationClip.name,
                Channels = new List<AnimationChannel>(),
                Samplers = new List<AnimationSampler>(),
                Extensions = new Dictionary<string, IExtension>() {
                    {
                        AnimationExtensionFactory.EXTENSION_NAME,
                        new AnimationExtension () {
                            frameRate = animationClip.frameRate,
                            clips = new List<AnimationClip>() {
                                new AnimationClip() {
                                    name = animationClip.name,
                                    playTimes = _getPlayTimes(animationClip),
                                    position = 0.0f,
                                    duration = (float)Math.Round(animationClip.length, 6),
                                }
                            }
                        }
                    },
                },
            };
            var ext = glTFAnimation.Extensions[AnimationExtensionFactory.EXTENSION_NAME] as AnimationExtension;
            this._root.Animations.Add(glTFAnimation);
            // Input.
            var inputAccessor = new Accessor();
            inputAccessor.Count = frameCount;
            inputAccessor.Type = GLTFAccessorAttributeType.SCALAR;
            inputAccessor.ComponentType = GLTFComponentType.Float;
            inputAccessor.BufferView = new BufferViewId
            {
                Id = 0,
                Root = _root
            };
            this._root.Accessors.Add(inputAccessor);

            // Write input.
            for (var i = 0; i < frameCount; ++i)
            {
                //_bufferWriter.Write(Math.Round(Math.Min(animationClip.length * i / (frameCount - 1), animationClip.length), 6)); // TODO
                _bufferWriter.Write(i / animationClip.frameRate);
            }

            var MainTex_STy = new List<float>();
            foreach (var curveBind in curveBinds)
            {
                // Curve has been parsed.
                if (ignoreCurves.Contains(curveBind))
                {
                    continue;
                }
                // No target.
                var animationTarget = _target.Find(curveBind.path);
                if (animationTarget == null)
                {
                    continue;
                }
                // Create node.
                var nodeIndex = _animationTargets.IndexOf(animationTarget);
                if (nodeIndex < 0)
                {
                    _animationTargets.Add(animationTarget);
                    nodeIndex = _root.Nodes.Count;
                    _root.Nodes.Add(new Node()
                    {
                        Name = _target == animationTarget ? "__root__" : animationTarget.name,
                    });

                    if (animationTarget.transform.parent == _target)
                    {
                        _root.Scenes[0].Nodes.Add(
                            new NodeId()
                            {
                                Id = nodeIndex,
                                Root = _root,
                            }
                        );
                    }
                }
                // Output.
                var outputAccessor = new Accessor();
                outputAccessor.Count = frameCount;
                outputAccessor.ComponentType = GLTFComponentType.Float;
                outputAccessor.BufferView = inputAccessor.BufferView;
                outputAccessor.ByteOffset = (int)_bufferWriter.BaseStream.Position;
                this._root.Accessors.Add(outputAccessor);
                //
                var animationSampler = new AnimationSampler()
                {
                    Input = new AccessorId()
                    {
                        Id = this._root.Accessors.IndexOf(inputAccessor),
                        Root = _root,
                    },
                    Interpolation = InterpolationType.LINEAR,
                    Output = new AccessorId()
                    {
                        Id = this._root.Accessors.IndexOf(outputAccessor),
                        Root = _root,
                    },
                };
                glTFAnimation.Samplers.Add(animationSampler);
                //
                var animationChannel = new AnimationChannel()
                {
                    Sampler = new SamplerId()
                    {
                        Id = glTFAnimation.Samplers.IndexOf(animationSampler),
                        Root = _root,
                    },
                    Target = new AnimationChannelTarget()
                    {
                        Node = new NodeId()
                        {
                            Id = nodeIndex,
                            Root = _root,
                        }
                    }
                };
                glTFAnimation.Channels.Add(animationChannel);

                if (curveBind.type == typeof(Transform))
                {
                    var curveGroup = _getCurveGroup(curveBinds, curveBind);
                    ignoreCurves.AddRange(curveGroup);

                    switch (curveBind.propertyName)
                    {
                        case "m_LocalPosition.x":
                        case "m_LocalPosition.y":
                        case "m_LocalPosition.z":
                            animationChannel.Target.Path = GLTFAnimationChannelPath.translation;
                            outputAccessor.Type = GLTFAccessorAttributeType.VEC3;
                            
                            for (var i = 0; i < frameCount; ++i)
                            {
                                var time = i / animationClip.frameRate;
                                var curveX = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[0]);
                                var curveY = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[1]);
                                var curveZ = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[2]);
                                var value = curveX != null ? curveX.Evaluate(time) : animationTarget.transform.localPosition.x;
                                _bufferWriter.Write(value);
                                value = curveY != null ? curveY.Evaluate(time) : animationTarget.transform.localPosition.y;
                                _bufferWriter.Write(value);
                                value = curveZ != null ? curveZ.Evaluate(time) : animationTarget.transform.localPosition.z;
                                _bufferWriter.Write(value);
                            }
                            break;

                        case "m_LocalRotation.x":
                        case "m_LocalRotation.y":
                        case "m_LocalRotation.z":
                        case "m_LocalRotation.w":
                            animationChannel.Target.Path = GLTFAnimationChannelPath.rotation;
                            outputAccessor.Type = GLTFAccessorAttributeType.VEC4;

                            for (var i = 0; i < frameCount; ++i)
                            {
                                var time = i / animationClip.frameRate;
                                var curveX = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[0]);
                                var curveY = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[1]);
                                var curveZ = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[2]);
                                var curveW = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[3]);
                                var valueX = curveX != null ? curveX.Evaluate(time) : animationTarget.transform.localRotation.x;
                                var valueY = curveY != null ? curveY.Evaluate(time) : animationTarget.transform.localRotation.y;
                                var valueZ = curveZ != null ? curveZ.Evaluate(time) : animationTarget.transform.localRotation.z;
                                var valueW = curveW != null ? curveW.Evaluate(time) : animationTarget.transform.localRotation.w;

                                _bufferWriter.Write(valueX);
                                _bufferWriter.Write(valueY);
                                _bufferWriter.Write(valueZ);
                                _bufferWriter.Write(valueW);
                            }
                            break;

                        case "localEulerAnglesRaw.x":
                        case "localEulerAnglesRaw.y":
                        case "localEulerAnglesRaw.z":
                            animationChannel.Target.Path = GLTFAnimationChannelPath.rotation;
                            outputAccessor.Type = GLTFAccessorAttributeType.VEC4;

                            for (var i = 0; i < frameCount; ++i)
                            {
                                var time = i / animationClip.frameRate;
                                var curveX = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[0]);
                                var curveY = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[1]);
                                var curveZ = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[2]);
                                var valueX = curveX != null ? curveX.Evaluate(time) : animationTarget.transform.localEulerAngles.x;
                                var valueY = curveY != null ? curveY.Evaluate(time) : animationTarget.transform.localEulerAngles.y;
                                var valueZ = curveZ != null ? curveZ.Evaluate(time) : animationTarget.transform.localEulerAngles.z;
                                var quaternion = Quaternion.Euler(valueX, valueY, valueZ);

                                _bufferWriter.Write(quaternion.x);
                                _bufferWriter.Write(quaternion.y);
                                _bufferWriter.Write(quaternion.z);
                                _bufferWriter.Write(quaternion.w);
                            }
                            break;

                        case "m_LocalScale.x":
                        case "m_LocalScale.y":
                        case "m_LocalScale.z":
                            animationChannel.Target.Path = GLTFAnimationChannelPath.scale;
                            outputAccessor.Type = GLTFAccessorAttributeType.VEC3;

                            for (var i = 0; i < frameCount; ++i)
                            {
                                var time = i / animationClip.frameRate;
                                var curveX = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[0]);
                                var curveY = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[1]);
                                var curveZ = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveGroup[2]);
                                var value = curveX != null ? curveX.Evaluate(time) : animationTarget.transform.localScale.x;
                                _bufferWriter.Write(value);
                                value = curveY != null ? curveY.Evaluate(time) : animationTarget.transform.localScale.y;
                                _bufferWriter.Write(value);
                                value = curveZ != null ? curveZ.Evaluate(time) : animationTarget.transform.localScale.z;
                                _bufferWriter.Write(value);
                            }
                            break;
                    }
                }
                else
                {
                    animationChannel.Target.Path = GLTFAnimationChannelPath.custom;
                    outputAccessor.Type = GLTFAccessorAttributeType.SCALAR;

                    var type = "";
                    var property = "";
                    var uri = "";
                    var needUpdate = -1;

                    if (curveBind.type == typeof(GameObject))
                    {
                        type = "paper.GameObject";

                        switch (curveBind.propertyName)
                        {
                            case "m_IsActive":
                                property = "activeSelf";
                                animationSampler.Interpolation = InterpolationType.STEP;
                                break;
                        }

                        // for (var i = 0; i < frameCount; ++i) // TODO
                        // {
                        //     var time = animationClip.length * i / frameCountSO; // TODO
                        //     var curve = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveBind);
                        //     var value = curve.Evaluate(time);
                        //     _bufferWriter.Write(value);
                        // }
                    }
                    else if (curveBind.type == typeof(UnityEngine.MeshRenderer))
                    {
                        type = "egret3d.MeshRenderer";
                        uri = "materials/0/$/_uvTransform";
                        needUpdate = 1;
                        // animationSampler.Interpolation = InterpolationType.STEP;

                        switch (curveBind.propertyName)
                        {
                            case "material._MainTex_ST.z":
                                property = "0";
                                break;

                            case "material._MainTex_ST.w":
                                property = "1";
                                break;

                            case "material._MainTex_ST.x":
                                property = "2";
                                break;

                            case "material._MainTex_ST.y":
                                property = "3";
                                break;
                        }
                    }
                    else
                    {
                        Debug.Log("Unknown type and property." + curveBind.type.ToString() + curveBind.propertyName);
                    }

                    // Extensions.
                    animationChannel.Extensions = new Dictionary<string, IExtension>() {
                        {
                            AnimationExtensionFactory.EXTENSION_NAME,
                            new AnimationChannelExtension () {
                                type = type,
                                property = property,
                                uri = uri,
                                needUpdate = needUpdate,
                            }
                        },
                    };

                    for (var i = 0; i < frameCount; ++i)
                    {
                        var curve = UnityEditor.AnimationUtility.GetEditorCurve(animationClip, curveBind);
                        if (curve != null)
                        {
                            var value = curve.Evaluate(i / animationClip.frameRate);
                            if (curveBind.propertyName == "material._MainTex_ST.w")
                            {
                                if (i < MainTex_STy.Count)
                                {
                                    _bufferWriter.Write(1.0f - value - MainTex_STy[i]);
                                }
                                else
                                {
                                    _bufferWriter.Write(value);
                                }                                
                            }
                            else
                            {
                                _bufferWriter.Write(value);
                                if (curveBind.propertyName == "material._MainTex_ST.y")
                                {
                                    MainTex_STy.Add(value);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var evt in animationClip.events)
            {
                var glTFFrameEvent = new AnimationFrameEvent();
                glTFFrameEvent.name = evt.functionName;
                glTFFrameEvent.position = evt.time;
                glTFFrameEvent.intVariable = evt.intParameter;
                glTFFrameEvent.floatVariable = evt.floatParameter;
                glTFFrameEvent.stringVariable = evt.stringParameter;
                ext.events.Add(glTFFrameEvent);
            }

            ext.events.Sort();
        }
    }
}
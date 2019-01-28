namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;
    using Egret3DExportTools;

    public class MeshWriter : GLTFExporter
    {
        private Transform _target;
        public MeshWriter(Transform target) : base()
        {
            this._target = target;
        }

        protected override void Init()
        {
            base.Init();

            this._root = new GLTFRoot
            {
                Accessors = new List<Accessor>(),
                Asset = new Asset
                {
                    Version = "2.0",
                    Generator = "paper",
                    Extensions = new Dictionary<string, IExtension>(),
                },
                Buffers = new List<GLTF.Schema.Buffer>(),
                BufferViews = new List<BufferView>(),
                Meshes = new List<GLTF.Schema.Mesh>()
            };

            _root.Asset.Extensions.Add(CoordinateSystemExtensionFactory.EXTENSION_NAME, new CoordinateSystemExtension(CoordinateSystem.leftHand.ToString(), 1.0f));

            _buffer = new GLTF.Schema.Buffer();
            _bufferId = new BufferId
            {
                Id = _root.Buffers.Count,
                Root = _root
            };
            _root.Buffers.Add(_buffer);
        }

        public override byte[] WriteGLTF()
        {
            if (this._target.GetComponent<UnityEngine.SkinnedMeshRenderer>() == null &&
                (this._target.GetComponent<UnityEngine.MeshFilter>() == null ||
                this._target.GetComponent<UnityEngine.MeshRenderer>() == null) &&
                this._target.GetComponent<UnityEngine.ParticleSystemRenderer>() == null)
            {
                MyLog.LogWarning("Mesh glTF写入错误，请检查你的渲染器是否是SkinnedMeshRenderer,MeshRenderer,ParticleSystemRenderer中的任意一种");
                return new byte[0];
            }

            this.BeginWrite();
            ExportMesh();
            var res = this.EndWrite();
            return res;
        }

        private List<Transform> _getAllChildren(Transform transform, List<Transform> children = null)
        {
            if (children == null)
            {
                children = new List<Transform>();
            }

            for (var i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                children.Add(child);
                _getAllChildren(child, children);
            }

            return children;
        }

        //
        private void ExportMesh()
        {
            var mesh = new GLTF.Schema.Mesh();
            mesh.Primitives = new List<MeshPrimitive>();
            if (this._target.GetComponent<UnityEngine.SkinnedMeshRenderer>() != null)
            {
                var skin = this._target.GetComponent<UnityEngine.SkinnedMeshRenderer>();
                var parent = _target.parent;
                var allBones = _getAllChildren(parent);

                var gltfSKIN = new Skin
                {
                    Joints = new List<NodeId>()
                };
                _root.Nodes = new List<Node>();
                _root.Skins = new List<Skin>();
                _root.Skins.Add(gltfSKIN);

                foreach (var bone in allBones)
                {
                    var translation = bone.localPosition;
                    var rotation = bone.localRotation;
                    var scale = bone.localScale;
                    var node = new Node
                    {
                        Name = bone.gameObject.name,
                        Translation = new GLTF.Math.Vector3(translation.x, translation.y, translation.z),
                        Rotation = new GLTF.Math.Quaternion(rotation.x, rotation.y, rotation.z, rotation.w),
                        Scale = new GLTF.Math.Vector3(scale.x, scale.y, scale.z),
                    };

                    if (bone.childCount > 0)
                    {
                        // Debug.logger.Log(bone.childCount);
                        node.Children = new List<NodeId>();
                        for (var i = 0; i < bone.childCount; i++)
                        {
                            node.Children.Add(
                                new NodeId
                                {
                                    Id = allBones.IndexOf(bone.GetChild(i)),
                                    Root = _root
                                }
                            );
                        }
                    }
                    _root.Nodes.Add(node);
                }

                if (skin.rootBone != null)
                {
                    gltfSKIN.Skeleton = new NodeId
                    {
                        Id = allBones.IndexOf(skin.rootBone),
                        Root = _root
                    };
                }

                foreach (var bone in skin.bones)
                {
                    gltfSKIN.Joints.Add(
                        new NodeId
                        {
                            Id = allBones.IndexOf(bone),
                            Root = _root
                        }
                    );
                }

                mesh.Primitives.AddRange(ExportPrimitive(skin.sharedMesh, skin.sharedMaterials, gltfSKIN));
            }
            else if (this._target.GetComponent<UnityEngine.ParticleSystemRenderer>() != null)
            {
                var renderer = this._target.GetComponent<UnityEngine.ParticleSystemRenderer>();
                mesh.Primitives.AddRange(ExportPrimitive(renderer.mesh, renderer.sharedMaterials));
            }
            else
            {
                var filter = this._target.GetComponent<UnityEngine.MeshFilter>();
                var renderer = this._target.GetComponent<UnityEngine.MeshRenderer>();
                mesh.Primitives.AddRange(ExportPrimitive(filter.sharedMesh, renderer.sharedMaterials));
            }

            var id = new MeshId
            {
                Id = _root.Meshes.Count,
                Root = _root
            };
            _root.Meshes.Add(mesh);
        }

        // a mesh *might* decode to multiple prims if there are submeshes
        private MeshPrimitive[] ExportPrimitive(UnityEngine.Mesh meshObj, UnityEngine.Material[] materials, Skin skin = null)
        {
            MyLog.Log("Mesh属性:");
            var skinnedMeshRender = this._target.GetComponent<SkinnedMeshRenderer>();
            var root = skinnedMeshRender ? this._target.transform : null;
            var materialsObj = new List<UnityEngine.Material>(materials);

            var prims = new MeshPrimitive[meshObj.subMeshCount];

            var byteOffset = _bufferWriter.BaseStream.Position;
            var bufferViewId = ExportBufferView(0, (int)(_bufferWriter.BaseStream.Position - byteOffset));

            AccessorId aPosition = null, aNormal = null, aTangent = null,
                aColor0 = null, aTexcoord0 = null, aTexcoord1 = null,
                aBlendIndex = null, aBlendWeight = null;

            aPosition = ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(meshObj.vertices, SchemaExtensions.CoordinateSpaceConversionScale), bufferViewId, false);
            MyLog.Log("-------vertices:" + meshObj.vertices.Length);
            var setting = ExportToolsSetting.instance;
            var isInMeshIgnores = setting.IsInMeshIgnores(this._target.gameObject);
            if (meshObj.normals.Length != 0 && (isInMeshIgnores || setting.enableNormals))
            {
                MyLog.Log("-------normals:" + meshObj.normals.Length);
                aNormal = ExportAccessor(SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(meshObj.normals, SchemaExtensions.CoordinateSpaceConversionScale), bufferViewId, true);
            }

            /*if (meshObj.tangents.Length != 0)
            {
                aTangent = ExportAccessor(SchemaExtensions.ConvertVector4CoordinateSpaceAndCopy(meshObj.tangents, SchemaExtensions.TangentSpaceConversionScale), bufferViewId, true);
            }*/

            if (meshObj.colors.Length != 0 && (isInMeshIgnores || setting.enableColors))
            {
                MyLog.Log("-------colors:" + meshObj.colors.Length);
                aColor0 = ExportAccessor(meshObj.colors, bufferViewId);
            }

            if (meshObj.uv.Length != 0)
            {
                MyLog.Log("-------uv:" + meshObj.uv.Length);
                aTexcoord0 = ExportAccessor(SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv), bufferViewId);
            }

            var meshRender = this._target.GetComponent<MeshRenderer>();
            if (meshRender != null && meshRender.lightmapIndex >= 0)
            {
                MyLog.Log("-------uv2:" + meshObj.uv2.Length);
                aTexcoord1 = ExportAccessor(SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv2.Length > 0 ? meshObj.uv2 : meshObj.uv), bufferViewId);
                // aTexcoord1 = ExportAccessor(ConvertLightMapUVAndCopy(meshObj.uv2.Length > 0 ? meshObj.uv2 : meshObj.uv, meshRender.lightmapScaleOffset), bufferViewId);
            }
            else
            {
                if (meshObj.uv2.Length != 0)
                {
                    MyLog.Log("-------uv2:" + meshObj.uv2.Length);
                    aTexcoord1 = ExportAccessor(SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv2), bufferViewId);
                }
            }

            if (meshObj.boneWeights.Length != 0 && (isInMeshIgnores || setting.enableBones))
            {
                MyLog.Log("-------bones:" + meshObj.boneWeights.Length);
                aBlendIndex = ExportAccessor(SchemaExtensions.ConvertBlendIndexAndCopy(meshObj.boneWeights));
                aBlendWeight = ExportAccessor(SchemaExtensions.ConvertBlendWeightAndCopy(meshObj.boneWeights));
                if (skin != null)
                {
                    /*var index = 0;
                    var renderer = _target.GetComponent<SkinnedMeshRenderer>();
                    var bindposes = new Matrix4x4[renderer.bones.Length];

                    foreach (var bone in renderer.bones)
                    {
                        for (var i = 0; i < 16; ++i)
                        {
                            bindposes[index][i] = bone.worldToLocalMatrix[i];
                        }
                        index++;
                    }
                    skin.InverseBindMatrices = ExportAccessor(bindposes);*/
                    skin.InverseBindMatrices = ExportAccessor(meshObj.bindposes);
                }
            }

            this._root.BufferViews[bufferViewId.Id].ByteLength = (int)(this._bufferWriter.BaseStream.Position - byteOffset);


            MaterialId lastMaterialId = null;

            for (var submesh = 0; submesh < meshObj.subMeshCount; submesh++)
            {
                var primitive = new MeshPrimitive();

                var triangles = meshObj.GetTriangles(submesh);
                primitive.Indices = ExportAccessor(SchemaExtensions.FlipFacesAndCopy(triangles), true);

                primitive.Attributes = new Dictionary<string, AccessorId>();
                primitive.Attributes.Add(SemanticProperties.POSITION, aPosition);
                MyLog.Log("-------triangles:" + triangles.Length + "  submesh:" + submesh);
                if (aNormal != null)
                {
                    primitive.Attributes.Add(SemanticProperties.NORMAL, aNormal);
                }

                if (aTangent != null)
                {
                    primitive.Attributes.Add(SemanticProperties.TANGENT, aTangent);
                }

                if (aColor0 != null)
                {
                    primitive.Attributes.Add(SemanticProperties.Color(0), aColor0);
                }

                if (aTexcoord0 != null)
                {
                    primitive.Attributes.Add(SemanticProperties.TexCoord(0), aTexcoord0);
                }

                if (aTexcoord1 != null)
                {
                    primitive.Attributes.Add(SemanticProperties.TexCoord(1), aTexcoord1);
                }

                if (aBlendIndex != null && aBlendWeight != null)
                {
                    primitive.Attributes.Add(SemanticProperties.Joint(0), aBlendIndex);
                    primitive.Attributes.Add(SemanticProperties.Weight(0), aBlendWeight);
                }

                if (submesh < materialsObj.Count)
                {
                    primitive.Material = new MaterialId
                    {
                        Id = materialsObj.IndexOf(materialsObj[submesh]),
                        Root = _root
                    };
                    lastMaterialId = primitive.Material;
                }
                else
                {
                    primitive.Material = lastMaterialId;
                }

                prims[submesh] = primitive;
            }

            return prims;
        }
    }
}

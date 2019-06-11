namespace Egret3DExportTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using GLTF.Schema;
    using UnityEngine;
    using UnityGLTF.Extensions;
    using Egret3DExportTools;

    public class GLTFMeshSerializer : AssetSerializer
    {
        private UnityEngine.Mesh _mesh;
        protected override void InitGLTFRoot()
        {
            base.InitGLTFRoot();

            this._root.ExtensionsRequired.Add(CoordinateSystemExtension.EXTENSION_NAME);
            this._root.ExtensionsRequired.Add(AssetVersionExtension.EXTENSION_NAME);
            this._root.ExtensionsUsed.Add(CoordinateSystemExtension.EXTENSION_NAME);
            this._root.ExtensionsUsed.Add(AssetVersionExtension.EXTENSION_NAME);

            this._root.Accessors = new List<Accessor>();
            this._root.Buffers = new List<GLTF.Schema.Buffer>();
            this._root.BufferViews = new List<BufferView>();
            this._root.Meshes = new List<GLTF.Schema.Mesh>();

            this._root.Asset.Extensions.Add(CoordinateSystemExtension.EXTENSION_NAME, new CoordinateSystemExtension(CoordinateSystem.leftHand.ToString(), 1.0f));

            this._buffer = new GLTF.Schema.Buffer();
            this._bufferId = new BufferId
            {
                Id = this._root.Buffers.Count,
                Root = this._root
            };
            this._root.Buffers.Add(this._buffer);
        }
        protected override void Serialize(UnityEngine.Object sourceAsset)
        {
            this._mesh = sourceAsset as UnityEngine.Mesh;

            if (this._target.GetComponent<UnityEngine.SkinnedMeshRenderer>() == null &&
                (this._target.GetComponent<UnityEngine.MeshFilter>() == null ||
                this._target.GetComponent<UnityEngine.MeshRenderer>() == null) &&
                this._target.GetComponent<UnityEngine.ParticleSystemRenderer>() == null)
            {
                MyLog.LogWarning("Mesh glTF写入错误，请检查你的渲染器是否是SkinnedMeshRenderer,MeshRenderer,ParticleSystemRenderer中的任意一种");
                return;
            }

            this._bufferWriter = new BinaryWriter(new MemoryStream());
            ExportMesh();
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
            var bufferViewId = this._root.WriteBufferView(this._bufferId, 0, (int)(_bufferWriter.BaseStream.Position - byteOffset));

            AccessorId aPosition = null, aNormal = null, aTangent = null,
                aColor0 = null, aTexcoord0 = null, aTexcoord1 = null,
                aBlendIndex = null, aBlendWeight = null;

            aPosition = this._root.WriteAccessor(this._bufferId, SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(meshObj.vertices, SchemaExtensions.CoordinateSpaceConversionScale), bufferViewId, false, null, this._bufferWriter);
            MyLog.Log("-------vertices:" + meshObj.vertices.Length);
            if (meshObj.normals.Length != 0 && (ExportSetting.instance.mesh.normal))
            {
                MyLog.Log("-------normals:" + meshObj.normals.Length);
                aNormal = this._root.WriteAccessor(this._bufferId, SchemaExtensions.ConvertVector3CoordinateSpaceAndCopy(meshObj.normals, SchemaExtensions.CoordinateSpaceConversionScale), bufferViewId, true, null, this._bufferWriter);
            }

            if (meshObj.tangents.Length != 0 && (ExportSetting.instance.mesh.tangent))
            {
                aTangent = this._root.WriteAccessor(this._bufferId, SchemaExtensions.ConvertVector4CoordinateSpaceAndCopy(meshObj.tangents, SchemaExtensions.TangentSpaceConversionScale), bufferViewId, true, this._bufferWriter);
            }

            if (meshObj.colors.Length != 0 && (ExportSetting.instance.mesh.color))
            {
                MyLog.Log("-------colors:" + meshObj.colors.Length);
                aColor0 = this._root.WriteAccessor(this._bufferId, meshObj.colors, bufferViewId, this._bufferWriter);
            }

            if (meshObj.uv.Length != 0)
            {
                MyLog.Log("-------uv:" + meshObj.uv.Length);
                aTexcoord0 = this._root.WriteAccessor(this._bufferId, SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv), bufferViewId, this._bufferWriter);
            }

            var meshRender = this._target.GetComponent<MeshRenderer>();
            if (meshRender != null && meshRender.lightmapIndex >= 0)
            {
                MyLog.Log("-------uv2:" + meshObj.uv2.Length);
                aTexcoord1 = this._root.WriteAccessor(this._bufferId, SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv2.Length > 0 ? meshObj.uv2 : meshObj.uv), bufferViewId, this._bufferWriter);
                // aTexcoord1 = ExportAccessor(ConvertLightMapUVAndCopy(meshObj.uv2.Length > 0 ? meshObj.uv2 : meshObj.uv, meshRender.lightmapScaleOffset), bufferViewId);
            }
            else
            {
                if (meshObj.uv2.Length != 0 && (ExportSetting.instance.mesh.uv2))
                {
                    MyLog.Log("-------uv2:" + meshObj.uv2.Length);
                    aTexcoord1 = this._root.WriteAccessor(this._bufferId, SchemaExtensions.FlipTexCoordArrayVAndCopy(meshObj.uv2), bufferViewId, this._bufferWriter);
                }
            }

            if (meshObj.boneWeights.Length != 0 && (ExportSetting.instance.mesh.bone))
            {
                MyLog.Log("-------bones:" + meshObj.boneWeights.Length);
                aBlendIndex = this._root.WriteAccessor(this._bufferId, SchemaExtensions.ConvertBlendIndexAndCopy(meshObj.boneWeights), null, false, this._bufferWriter);
                aBlendWeight = this._root.WriteAccessor(this._bufferId, SchemaExtensions.ConvertBlendWeightAndCopy(meshObj.boneWeights), null, false, this._bufferWriter);
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
                    skin.InverseBindMatrices = this._root.WriteAccessor(this._bufferId, meshObj.bindposes, null, this._bufferWriter);
                }
            }

            this._root.BufferViews[bufferViewId.Id].ByteLength = (int)(this._bufferWriter.BaseStream.Position - byteOffset);


            MaterialId lastMaterialId = null;

            for (var submesh = 0; submesh < meshObj.subMeshCount; submesh++)
            {
                var primitive = new MeshPrimitive();

                var triangles = meshObj.GetTriangles(submesh);
                primitive.Indices = this._root.WriteAccessor(this._bufferId, SchemaExtensions.FlipFacesAndCopy(triangles), true, this._bufferWriter);

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

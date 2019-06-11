using System;
using System.Collections.Generic;
using System.IO;
using GLTF.Schema;
using UnityEngine;

namespace Egret3DExportTools
{
    public static class GLTFExtension
    {
        public static AccessorId WriteAccessor(this GLTFRoot root, BufferId bufferId, int[] arr, bool isIndices = false, BinaryWriter writer = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                MyLog.LogError("Accessors can not have a count of 0.");
            }

            var accessor = new Accessor();
            accessor.Count = count;
            accessor.Type = GLTFAccessorAttributeType.SCALAR;

            int min = arr[0];
            int max = arr[0];

            for (var i = 1; i < count; i++)
            {
                var cur = arr[i];

                if (cur < min)
                {
                    min = cur;
                }
                if (cur > max)
                {
                    max = cur;
                }
            }

            var byteOffset = writer.BaseStream.Position;

            if (max <= byte.MaxValue && min >= byte.MinValue && !isIndices)
            {
                accessor.ComponentType = GLTFComponentType.UnsignedByte;

                foreach (var v in arr)
                {
                    writer.Write((byte)v);
                }
            }
            else if (max <= sbyte.MaxValue && min >= sbyte.MinValue && !isIndices)
            {
                accessor.ComponentType = GLTFComponentType.Byte;

                foreach (var v in arr)
                {
                    writer.Write((sbyte)v);
                }
            }
            else if (max <= short.MaxValue && min >= short.MinValue && !isIndices)
            {
                accessor.ComponentType = GLTFComponentType.Short;

                foreach (var v in arr)
                {
                    writer.Write((short)v);
                }
            }
            else if (max <= ushort.MaxValue && min >= ushort.MinValue)
            {
                accessor.ComponentType = GLTFComponentType.UnsignedShort;

                foreach (var v in arr)
                {
                    writer.Write((ushort)v);
                }
            }
            else if (min >= uint.MinValue)
            {
                accessor.ComponentType = GLTFComponentType.UnsignedInt;

                foreach (var v in arr)
                {
                    writer.Write((uint)v);
                }
            }
            else
            {
                accessor.ComponentType = GLTFComponentType.Float;

                foreach (var v in arr)
                {
                    writer.Write((float)v);
                }
            }

            accessor.Min = new List<double> { min };
            accessor.Max = new List<double> { max };

            var byteLength = writer.BaseStream.Position - byteOffset;

            accessor.BufferView = root.WriteBufferView(bufferId, (int)byteOffset, (int)byteLength);

            var id = new AccessorId
            {
                Id = root.Accessors.Count,
                Root = root
            };

            root.Accessors.Add(accessor);

            return id;
        }

        public static AccessorId WriteAccessor(this GLTFRoot root, BufferId bufferId, Vector2[] arr, BufferViewId bufferViewId = null, BinaryWriter writer = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                MyLog.LogError("Accessors can not have a count of 0.");
            }

            var accessor = new Accessor();
            accessor.ComponentType = GLTFComponentType.Float;
            accessor.Count = count;
            accessor.Type = GLTFAccessorAttributeType.VEC2;

            float minX = arr[0].x;
            float minY = arr[0].y;
            float maxX = arr[0].x;
            float maxY = arr[0].y;

            for (var i = 1; i < count; i++)
            {
                var cur = arr[i];

                if (cur.x < minX)
                {
                    minX = cur.x;
                }
                if (cur.y < minY)
                {
                    minY = cur.y;
                }
                if (cur.x > maxX)
                {
                    maxX = cur.x;
                }
                if (cur.y > maxY)
                {
                    maxY = cur.y;
                }
            }

            accessor.Min = new List<double> { minX, minY };
            accessor.Max = new List<double> { maxX, maxY };

            var byteOffset = writer.BaseStream.Position;

            foreach (var vec in arr)
            {
                writer.Write(vec.x);
                writer.Write(vec.y);
            }

            var byteLength = writer.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = root.WriteBufferView(bufferId, (int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = root.Accessors.Count,
                Root = root
            };
            root.Accessors.Add(accessor);

            return id;
        }

        public static AccessorId WriteAccessor(this GLTFRoot root, BufferId bufferId, Vector3[] arr, BufferViewId bufferViewId = null, bool normalized = false, Transform rootNode = null, BinaryWriter writer = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                MyLog.LogError("Accessors can not have a count of 0.");
            }

            var accessor = new Accessor();
            accessor.ComponentType = GLTFComponentType.Float;
            accessor.Count = count;
            accessor.Type = GLTFAccessorAttributeType.VEC3;

            /*float minX = arr[0].x;
            float minY = arr[0].y;
            float minZ = arr[0].z;
            float maxX = arr[0].x;
            float maxY = arr[0].y;
            float maxZ = arr[0].z;

            for (var i = 1; i < count; i++)
            {
                var cur = arr[i];

                if (cur.x < minX)
                {
                    minX = cur.x;
                }
                if (cur.y < minY)
                {
                    minY = cur.y;
                }
                if (cur.z < minZ)
                {
                    minZ = cur.z;
                }
                if (cur.x > maxX)
                {
                    maxX = cur.x;
                }
                if (cur.y > maxY)
                {
                    maxY = cur.y;
                }
                if (cur.z > maxZ)
                {
                    maxZ = cur.z;
                }
            }*/

            accessor.Normalized = normalized;
            //accessor.Min = new List<double> { minX, minY, minZ };
            //accessor.Max = new List<double> { maxX, maxY, maxZ };

            var byteOffset = writer.BaseStream.Position;
            Matrix4x4 mA = rootNode != null ? rootNode.localToWorldMatrix : new Matrix4x4();
            Matrix4x4 mB = rootNode != null ? rootNode.parent.worldToLocalMatrix : new Matrix4x4();

            foreach (var vec in arr)
            {
                writer.Write(vec.x);
                writer.Write(vec.y);
                writer.Write(vec.z);
            }

            var byteLength = writer.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = root.WriteBufferView(bufferId, (int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = root.Accessors.Count,
                Root = root
            };
            root.Accessors.Add(accessor);

            return id;
        }

        public static AccessorId WriteAccessor(this GLTFRoot root, BufferId bufferId, Vector4[] arr, BufferViewId bufferViewId = null, bool normalized = false, BinaryWriter writer = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                MyLog.LogError("Accessors can not have a count of 0.");
            }

            var accessor = new Accessor();
            accessor.ComponentType = GLTFComponentType.Float;
            accessor.Count = count;
            accessor.Type = GLTFAccessorAttributeType.VEC4;

            float minX = arr[0].x;
            float minY = arr[0].y;
            float minZ = arr[0].z;
            float minW = arr[0].w;
            float maxX = arr[0].x;
            float maxY = arr[0].y;
            float maxZ = arr[0].z;
            float maxW = arr[0].w;

            for (var i = 1; i < count; i++)
            {
                var cur = arr[i];

                if (cur.x < minX)
                {
                    minX = cur.x;
                }
                if (cur.y < minY)
                {
                    minY = cur.y;
                }
                if (cur.z < minZ)
                {
                    minZ = cur.z;
                }
                if (cur.w < minW)
                {
                    minW = cur.w;
                }
                if (cur.x > maxX)
                {
                    maxX = cur.x;
                }
                if (cur.y > maxY)
                {
                    maxY = cur.y;
                }
                if (cur.z > maxZ)
                {
                    maxZ = cur.z;
                }
                if (cur.w > maxW)
                {
                    maxW = cur.w;
                }
            }
            accessor.Normalized = normalized;
            accessor.Min = new List<double> { minX, minY, minZ, minW };
            accessor.Max = new List<double> { maxX, maxY, maxZ, maxW };

            var byteOffset = writer.BaseStream.Position;

            foreach (var vec in arr)
            {
                writer.Write(vec.x);
                writer.Write(vec.y);
                writer.Write(vec.z);
                writer.Write(vec.w);
            }

            var byteLength = writer.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = root.WriteBufferView(bufferId, (int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = root.Accessors.Count,
                Root = root
            };
            root.Accessors.Add(accessor);

            return id;
        }

        public static AccessorId WriteAccessor(this GLTFRoot root, BufferId bufferId, Matrix4x4[] arr, BufferViewId bufferViewId = null, BinaryWriter writer = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                MyLog.LogError("Accessors can not have a count of 0.");
            }

            var accessor = new Accessor();
            accessor.ComponentType = GLTFComponentType.Float;
            accessor.Count = count;
            accessor.Type = GLTFAccessorAttributeType.MAT4;

            var byteOffset = writer.BaseStream.Position;

            foreach (var vec in arr)
            {
                writer.Write(vec.m00);
                writer.Write(vec.m10);
                writer.Write(vec.m20);
                writer.Write(vec.m30);
                writer.Write(vec.m01);
                writer.Write(vec.m11);
                writer.Write(vec.m21);
                writer.Write(vec.m31);
                writer.Write(vec.m02);
                writer.Write(vec.m12);
                writer.Write(vec.m22);
                writer.Write(vec.m32);
                writer.Write(vec.m03);
                writer.Write(vec.m13);
                writer.Write(vec.m23);
                writer.Write(vec.m33);
            }

            var byteLength = writer.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = root.WriteBufferView(bufferId, (int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = root.Accessors.Count,
                Root = root
            };
            root.Accessors.Add(accessor);

            return id;
        }

        public static AccessorId WriteAccessor(this GLTFRoot root, BufferId bufferId, UnityEngine.Color[] arr, BufferViewId bufferViewId = null, BinaryWriter writer = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                MyLog.LogError("Accessors can not have a count of 0.");
            }

            var accessor = new Accessor();
            accessor.ComponentType = GLTFComponentType.Float;
            accessor.Count = count;
            accessor.Type = GLTFAccessorAttributeType.VEC4;

            float minR = arr[0].r;
            float minG = arr[0].g;
            float minB = arr[0].b;
            float minA = arr[0].a;
            float maxR = arr[0].r;
            float maxG = arr[0].g;
            float maxB = arr[0].b;
            float maxA = arr[0].a;

            for (var i = 1; i < count; i++)
            {
                var cur = arr[i];

                if (cur.r < minR)
                {
                    minR = cur.r;
                }
                if (cur.g < minG)
                {
                    minG = cur.g;
                }
                if (cur.b < minB)
                {
                    minB = cur.b;
                }
                if (cur.a < minA)
                {
                    minA = cur.a;
                }
                if (cur.r > maxR)
                {
                    maxR = cur.r;
                }
                if (cur.g > maxG)
                {
                    maxG = cur.g;
                }
                if (cur.b > maxB)
                {
                    maxB = cur.b;
                }
                if (cur.a > maxA)
                {
                    maxA = cur.a;
                }
            }

            accessor.Normalized = true;
            accessor.Min = new List<double> { minR, minG, minB, minA };
            accessor.Max = new List<double> { maxR, maxG, maxB, maxA };

            var byteOffset = writer.BaseStream.Position;

            foreach (var color in arr)
            {
                writer.Write(color.r);
                writer.Write(color.g);
                writer.Write(color.b);
                writer.Write(color.a);
            }

            var byteLength = writer.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = root.WriteBufferView(bufferId, (int)byteOffset, (int)byteLength);
            }

            var id = new AccessorId
            {
                Id = root.Accessors.Count,
                Root = root
            };
            root.Accessors.Add(accessor);

            return id;
        }

        public static BufferViewId WriteBufferView(this GLTFRoot root, BufferId bufferId, int byteOffset, int byteLength)
        {
            var bufferView = new BufferView
            {
                Buffer = bufferId,
                ByteOffset = byteOffset,
                ByteLength = byteLength,
            };

            var id = new BufferViewId
            {
                Id = root.BufferViews.Count,
                Root = root
            };

            root.BufferViews.Add(bufferView);

            return id;
        }

        public static byte[] WriteBinary(byte[] json, byte[] bin, BinaryWriter writer)
        {
            var resize = new List<byte>(json);
            while (resize.Count % 4 != 0)
            {
                resize.Add(0);
            }
            json = resize.ToArray();

            resize = new List<byte>(bin);
            while (resize.Count % 4 != 0)
            {
                resize.Add(0);
            }
            bin = resize.ToArray();
            //
            writer.Write(0x46546c67);
            writer.Write(2);
            writer.Write(0);
            //JSON CHUNK
            writer.Write(json.Length);
            writer.Write(0x4e4f534a);
            writer.Write(json);
            //BIN CHUNK
            writer.Write(bin.Length);
            writer.Write(0x004e4942);
            writer.Write(bin);
            //全部写完，长度重写
            var ms = writer.BaseStream as MemoryStream;
            var length = (uint)ms.Length;
            ms.Position = 8;
            ms.Write(BitConverter.GetBytes((UInt32)length), 0, 4);

            writer.Close();

            var res = ms.ToArray();
            ms.Dispose();

            return res;
        }
    }
}
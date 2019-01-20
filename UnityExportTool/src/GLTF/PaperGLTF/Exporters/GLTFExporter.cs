namespace Egret3DExportTools
{
    using GLTF.Schema;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public abstract class GLTFExporter
    {
        protected GLTFRoot _root;
        protected BufferId _bufferId;
        protected GLTF.Schema.Buffer _buffer;
        protected BinaryWriter _bufferWriter;
        protected StreamWriter _streamWriter;

        public GLTFExporter()
        {
            this.Init();
        }

        public virtual byte[] WriteGLTF()
        {
            return null;
        }

        protected virtual void Init()
        {

        }

        protected void BeginWrite()
        {
            var ms = new MemoryStream();
            this._bufferWriter = new BinaryWriter(ms);
        }

        protected byte[] EndWrite()
        {
            var ms = this._bufferWriter.BaseStream as MemoryStream;
            var binArr = new List<byte>(ms.ToArray());
            while (binArr.Count % 4 != 0)
            {
                binArr.Add(0);
            }
            var binBuffer = binArr.ToArray();
            _bufferWriter.Close();

            _buffer.Uri = "";
            _buffer.ByteLength = binBuffer.Length;

            ms = new MemoryStream();

            _streamWriter = new StreamWriter(ms);
            _root.Serialize(_streamWriter);
            _streamWriter.Close();
            var jsonList = new List<byte>(ms.ToArray());

            while (jsonList.Count % 4 != 0)
            {
                jsonList.Add(0);
            }
            var jsonBuffer = jsonList.ToArray();

            ms = new MemoryStream();

            var writer = new System.IO.BinaryWriter(ms);
            writer.Write(0x46546c67);
            writer.Write(2);
            writer.Write(0);
            //JSON CHUNK
            writer.Write(jsonBuffer.Length);
            writer.Write(0x4e4f534a);
            writer.Write(jsonBuffer);
            //BIN CHUNK
            writer.Write(binBuffer.Length);
            writer.Write(0x004e4942);
            writer.Write(binBuffer);

            //全部写完，长度重写
            var length = (uint)ms.Length;
            ms.Position = 8;
            ms.Write(BitConverter.GetBytes((UInt32)length), 0, 4);

            writer.Close();

            var res = ms.ToArray();
            ms.Dispose();

            return res;
        }

        protected AccessorId ExportAccessor(int[] arr, bool isIndices = false)
        {
            var count = arr.Length;

            if (count == 0)
            {
                throw new Exception("Accessors can not have a count of 0.");
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

            var byteOffset = _bufferWriter.BaseStream.Position;

            if (max <= byte.MaxValue && min >= byte.MinValue && !isIndices)
            {
                accessor.ComponentType = GLTFComponentType.UnsignedByte;

                foreach (var v in arr)
                {
                    _bufferWriter.Write((byte)v);
                }
            }
            else if (max <= sbyte.MaxValue && min >= sbyte.MinValue && !isIndices)
            {
                accessor.ComponentType = GLTFComponentType.Byte;

                foreach (var v in arr)
                {
                    _bufferWriter.Write((sbyte)v);
                }
            }
            else if (max <= short.MaxValue && min >= short.MinValue && !isIndices)
            {
                accessor.ComponentType = GLTFComponentType.Short;

                foreach (var v in arr)
                {
                    _bufferWriter.Write((short)v);
                }
            }
            else if (max <= ushort.MaxValue && min >= ushort.MinValue)
            {
                accessor.ComponentType = GLTFComponentType.UnsignedShort;

                foreach (var v in arr)
                {
                    _bufferWriter.Write((ushort)v);
                }
            }
            else if (min >= uint.MinValue)
            {
                accessor.ComponentType = GLTFComponentType.UnsignedInt;

                foreach (var v in arr)
                {
                    _bufferWriter.Write((uint)v);
                }
            }
            else
            {
                accessor.ComponentType = GLTFComponentType.Float;

                foreach (var v in arr)
                {
                    _bufferWriter.Write((float)v);
                }
            }

            accessor.Min = new List<double> { min };
            accessor.Max = new List<double> { max };

            var byteLength = _bufferWriter.BaseStream.Position - byteOffset;

            accessor.BufferView = ExportBufferView((int)byteOffset, (int)byteLength);

            var id = new AccessorId
            {
                Id = _root.Accessors.Count,
                Root = _root
            };
            _root.Accessors.Add(accessor);

            return id;
        }

        protected AccessorId ExportAccessor(Vector2[] arr, BufferViewId bufferViewId = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                throw new Exception("Accessors can not have a count of 0.");
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

            var byteOffset = _bufferWriter.BaseStream.Position;

            foreach (var vec in arr)
            {
                _bufferWriter.Write(vec.x);
                _bufferWriter.Write(vec.y);
            }

            var byteLength = _bufferWriter.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = ExportBufferView((int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = _root.Accessors.Count,
                Root = _root
            };
            _root.Accessors.Add(accessor);

            return id;
        }

        protected AccessorId ExportAccessor(Vector3[] arr, BufferViewId bufferViewId = null, bool normalized = false, Transform root = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                throw new Exception("Accessors can not have a count of 0.");
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

            var byteOffset = _bufferWriter.BaseStream.Position;
            Matrix4x4 mA = root != null ? root.localToWorldMatrix : new Matrix4x4();
            Matrix4x4 mB = root != null ? root.parent.worldToLocalMatrix : new Matrix4x4();

            foreach (var vec in arr)
            {
                if (root != null)
                {
                    if (normalized)
                    {
                        var v = mB.MultiplyVector(mA.MultiplyVector(vec));
                        _bufferWriter.Write(v.x);
                        _bufferWriter.Write(v.y);
                        _bufferWriter.Write(v.z);
                    }
                    else
                    {
                        var v = mB.MultiplyPoint(mA.MultiplyPoint(vec));
                        _bufferWriter.Write(v.x);
                        _bufferWriter.Write(v.y);
                        _bufferWriter.Write(v.z);
                    }
                }
                else
                {
                    _bufferWriter.Write(vec.x);
                    _bufferWriter.Write(vec.y);
                    _bufferWriter.Write(vec.z);
                }
            }

            var byteLength = _bufferWriter.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = ExportBufferView((int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = _root.Accessors.Count,
                Root = _root
            };
            _root.Accessors.Add(accessor);

            return id;
        }

        protected AccessorId ExportAccessor(Vector4[] arr, BufferViewId bufferViewId = null, bool normalized = false)
        {
            var count = arr.Length;

            if (count == 0)
            {
                throw new Exception("Accessors can not have a count of 0.");
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

            var byteOffset = _bufferWriter.BaseStream.Position;

            foreach (var vec in arr)
            {
                _bufferWriter.Write(vec.x);
                _bufferWriter.Write(vec.y);
                _bufferWriter.Write(vec.z);
                _bufferWriter.Write(vec.w);
            }

            var byteLength = _bufferWriter.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = ExportBufferView((int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = _root.Accessors.Count,
                Root = _root
            };
            _root.Accessors.Add(accessor);

            return id;
        }

        protected AccessorId ExportAccessor(Matrix4x4[] arr, BufferViewId bufferViewId = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                throw new Exception("Accessors can not have a count of 0.");
            }

            var accessor = new Accessor();
            accessor.ComponentType = GLTFComponentType.Float;
            accessor.Count = count;
            accessor.Type = GLTFAccessorAttributeType.MAT4;
            
            var byteOffset = _bufferWriter.BaseStream.Position;

            foreach (var vec in arr)
            {
                _bufferWriter.Write(vec.m00);
                _bufferWriter.Write(vec.m10);
                _bufferWriter.Write(vec.m20);
                _bufferWriter.Write(vec.m30);
                _bufferWriter.Write(vec.m01);
                _bufferWriter.Write(vec.m11);
                _bufferWriter.Write(vec.m21);
                _bufferWriter.Write(vec.m31);
                _bufferWriter.Write(vec.m02);
                _bufferWriter.Write(vec.m12);
                _bufferWriter.Write(vec.m22);
                _bufferWriter.Write(vec.m32);
                _bufferWriter.Write(vec.m03);
                _bufferWriter.Write(vec.m13);
                _bufferWriter.Write(vec.m23);
                _bufferWriter.Write(vec.m33);
            }

            var byteLength = _bufferWriter.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = ExportBufferView((int)byteOffset, (int)byteLength);
            }


            var id = new AccessorId
            {
                Id = _root.Accessors.Count,
                Root = _root
            };
            _root.Accessors.Add(accessor);

            return id;
        }

        protected AccessorId ExportAccessor(UnityEngine.Color[] arr, BufferViewId bufferViewId = null)
        {
            var count = arr.Length;

            if (count == 0)
            {
                throw new Exception("Accessors can not have a count of 0.");
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

            var byteOffset = _bufferWriter.BaseStream.Position;

            foreach (var color in arr)
            {
                _bufferWriter.Write(color.r);
                _bufferWriter.Write(color.g);
                _bufferWriter.Write(color.b);
                _bufferWriter.Write(color.a);
            }

            var byteLength = _bufferWriter.BaseStream.Position - byteOffset;

            if (bufferViewId != null)
            {
                accessor.BufferView = bufferViewId;
                accessor.ByteOffset = (int)byteOffset;
            }
            else
            {
                accessor.BufferView = ExportBufferView((int)byteOffset, (int)byteLength);
            }

            var id = new AccessorId
            {
                Id = _root.Accessors.Count,
                Root = _root
            };
            _root.Accessors.Add(accessor);

            return id;
        }

        protected BufferViewId ExportBufferView(int byteOffset, int byteLength)
        {
            var bufferView = new BufferView
            {
                Buffer = _bufferId,
                ByteOffset = byteOffset,
                ByteLength = byteLength,
            };

            var id = new BufferViewId
            {
                Id = _root.BufferViews.Count,
                Root = _root
            };

            _root.BufferViews.Add(bufferView);

            return id;
        }
    }
}

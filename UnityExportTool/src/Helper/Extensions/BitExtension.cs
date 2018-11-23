using System;
using UnityEngine;


public static class BitExtension
{
    public static byte[] GetBytes(this Vector2 pos)
    {
        byte[] buf = new byte[8];
        BitConverter.GetBytes(pos.x).CopyTo(buf, 0);
        BitConverter.GetBytes(pos.y).CopyTo(buf, 4);
        return buf;
    }
    public static byte[] GetBytes(this Vector3 pos)
    {
        byte[] buf = new byte[12];
        BitConverter.GetBytes(pos.x).CopyTo(buf, 0);
        BitConverter.GetBytes(pos.y).CopyTo(buf, 4);
        BitConverter.GetBytes(pos.z).CopyTo(buf, 8);
        return buf;
    }
    public static byte[] GetBytes(this Vector4 pos)
    {
        byte[] buf = new byte[16];
        BitConverter.GetBytes(pos.x).CopyTo(buf, 0);
        BitConverter.GetBytes(pos.y).CopyTo(buf, 4);
        BitConverter.GetBytes(pos.z).CopyTo(buf, 8);
        BitConverter.GetBytes(pos.w).CopyTo(buf, 12);
        return buf;
    }
    public static byte[] GetBytes(this Quaternion pos)
    {
        byte[] buf = new byte[16];
        BitConverter.GetBytes(pos.x).CopyTo(buf, 0);
        BitConverter.GetBytes(pos.y).CopyTo(buf, 4);
        BitConverter.GetBytes(pos.z).CopyTo(buf, 8);
        BitConverter.GetBytes(pos.w).CopyTo(buf, 12);
        return buf;
    }
    public static byte[] GetBytes(this Bounds bound)
    {
        byte[] buf = new byte[24];
        bound.center.GetBytes().CopyTo(buf, 0);
        bound.size.GetBytes().CopyTo(buf, 12);
        return buf;
    }
    public static byte[] GetBytes(this Color32 color)
    {
        byte[] buf = new byte[16];
        BitConverter.GetBytes((float)color.r).CopyTo(buf, 0);
        BitConverter.GetBytes((float)color.g).CopyTo(buf, 4);
        BitConverter.GetBytes((float)color.b).CopyTo(buf, 8);
        BitConverter.GetBytes((float)color.a).CopyTo(buf, 12);
        return buf;
    }
    public static byte[] GetBytes(this string str)
    {

        byte[] bs = System.Text.Encoding.UTF8.GetBytes(str);
        byte[] bnew = new byte[bs.Length + 1];
        bnew[0] = (byte)bs.Length;
        bs.CopyTo(bnew, 1);
        return bnew;
    }
    public static Vector2 ToVector2(this byte[] buf, int pos)
    {
        Vector2 vec;
        vec.x = BitConverter.ToSingle(buf, pos + 0);
        vec.y = BitConverter.ToSingle(buf, pos + 4);
        return vec;
    }
    public static Vector3 ToVector3(this byte[] buf, int pos)
    {
        Vector3 vec;
        vec.x = BitConverter.ToSingle(buf, pos + 0);
        vec.y = BitConverter.ToSingle(buf, pos + 4);
        vec.z = BitConverter.ToSingle(buf, pos + 8);
        return vec;
    }
    public static Vector4 ToVector4(this byte[] buf, int pos)
    {
        Vector4 vec;
        vec.x = BitConverter.ToSingle(buf, pos + 0);
        vec.y = BitConverter.ToSingle(buf, pos + 4);
        vec.z = BitConverter.ToSingle(buf, pos + 8);
        vec.w = BitConverter.ToSingle(buf, pos + 12);
        return vec;
    }
    public static Quaternion ToQuaternion(this byte[] buf, int pos)
    {
        Quaternion vec;
        vec.x = BitConverter.ToSingle(buf, pos + 0);
        vec.y = BitConverter.ToSingle(buf, pos + 4);
        vec.z = BitConverter.ToSingle(buf, pos + 8);
        vec.w = BitConverter.ToSingle(buf, pos + 12);
        return vec;
    }
    public static Bounds ToBounds(this byte[] bytes, int pos)
    {
        Bounds b = new Bounds(bytes.ToVector3(pos + 0), bytes.ToVector3(pos + 12));
        return b;
    }
    public static Color32 ToColor32(this byte[] buf, int pos)
    {
        Color32 c = new Color32();
        c.a = buf[pos + 0];
        c.r = buf[pos + 1];
        c.g = buf[pos + 2];
        c.b = buf[pos + 3];
        return c;
    }
    public static string ReadString(this byte[] buf, int pos, out int seekoffset)
    {
        int len = buf[pos];
        string str = System.Text.Encoding.UTF8.GetString(buf, pos + 1, len);

        seekoffset = len + 1;
        return str;
    }
    public static string ToString(this byte[] buf, int pos)
    {
        int len = buf[pos];
        string str = System.Text.Encoding.UTF8.GetString(buf, pos + 1, len);
        return str;
    }
    
    public static void MatrixDeCompose(Matrix4x4 m, out Vector3 pos, out Vector3 scale, out Quaternion quat)
    {
        //quat
        Vector3 vf = m.MultiplyVector(Vector3.forward);
        Vector3 vu = m.MultiplyVector(Vector3.up);
        quat = Quaternion.LookRotation(vf, vu);
        //pos
        pos = new Vector3(m.m03, m.m13, m.m23);
        //去掉旋转和偏移
        m.m03 = 0; m.m13 = 0; m.m23 = 0;
        Matrix4x4 im = Matrix4x4.TRS(Vector3.zero, quat, Vector3.one);
        m *= im.inverse;
        //scale
        scale = new Vector3(m.m00, m.m11, m.m22);

    }
}

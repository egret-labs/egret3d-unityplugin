using System;
using UnityEngine;

namespace FB.PosePlus
{

    [Serializable]
    public class PoseBoneMatrix : ICloneable
    {
        [SerializeField]
        public Vector3 t;
        [SerializeField]
        public Quaternion r = Quaternion.identity;

        public void UpdateTran(Transform trans, Matrix4x4 tpose, Transform rootbone, bool bAdd)
        {
            var mat = rootbone.localToWorldMatrix;
            var mat2 = Matrix4x4.TRS(t, r, Vector3.one);
            mat2 = mat2 * tpose.inverse;
            mat = mat * mat2;// * tpose;
            Vector3 vy = mat.GetColumn(1);
            Vector3 vz = mat.GetColumn(2);
            trans.position = mat.GetColumn(3);
            if (vz.magnitude <= 0.01 || vy.magnitude <= 0.01)
            {
//                Debug.Log(rootbone.localToWorldMatrix);
//                Debug.Log(mat2);
//                Debug.Log(tpose);
				Debug.Log ("TPoseScale:"+GetScale(tpose));
                Debug.LogError("error mat:" + vy + "," + vz);

                return;

            }
            trans.rotation = Quaternion.LookRotation(vz, vy);


            return;
        }

		static Vector3 GetScale(Matrix4x4 m){
			float x = Mathf.Sqrt (m.m00 * m.m00 + m.m01 * m.m01 + m.m02 + m.m02);
			float y = Mathf.Sqrt (m.m10 * m.m10 + m.m11 * m.m11 + m.m12 + m.m12);
			float z = Mathf.Sqrt (m.m20 * m.m20 + m.m21 * m.m21 + m.m22 + m.m22);
			return new Vector3 (x,y,z);
		}

        //unity默认实现的四元数相等精度太低
        static bool QuaternionEqual(Quaternion left, Quaternion right)
        {
            return left.x == right.x && left.y == right.y && left.z == right.z && left.w == right.w;

        }
        public void Record(Transform root, Transform trans, Matrix4x4 tpose, PoseBoneMatrix last)
        {            
            root.localScale.Set(1.0f, 1.0f, 1.0f); //
            var mat = root.localToWorldMatrix;


            var mat2 = trans.localToWorldMatrix;
            mat = mat.inverse * mat2;
            mat = mat * tpose;
            Vector3 vy = mat.GetColumn(1);
            Vector3 vz = mat.GetColumn(2);
            this.r = Quaternion.LookRotation(vz, vy);
            this.t = mat.GetColumn(3);

            //UnityEngine.Debug.Log("name:" + trans.name + " x:" + this.t.x + " y:" + this.t.y + " z:" + this.t.z);
        }

        public void Save(System.IO.Stream stream, PoseBoneMatrix last)
        {
            stream.Write(r.GetBytes(), 0, 16);
            stream.Write(t.GetBytes(), 0, 12);
        }
        public void Load(System.IO.Stream stream, PoseBoneMatrix last)
        {
            {
                byte[] buf = new byte[16];
                stream.Read(buf, 0, 16);
                r = buf.ToQuaternion(0);
            }
            {
                byte[] buf = new byte[12];
                stream.Read(buf, 0, 12);
                t = buf.ToVector3(0);
            }
        }

        public object Clone()
        {
            PoseBoneMatrix bm = new PoseBoneMatrix();
            bm.r = this.r;
            if (r.w == 0)
            {
                bm.r = Quaternion.identity;
            }
            bm.t = this.t;
            return bm;
        }
        public static PoseBoneMatrix Lerp(PoseBoneMatrix left, PoseBoneMatrix right, float lerp)
        {
            PoseBoneMatrix m = new PoseBoneMatrix();
            m.r = Quaternion.Lerp(left.r, right.r, lerp);
            if (float.IsNaN(m.r.x))
            {
                m.r = Quaternion.identity;
            }
            m.t = Vector3.Lerp(left.t, right.t, lerp);
            return m;
        }
    }
}

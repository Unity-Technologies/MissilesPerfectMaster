/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public static class Utility {

    public struct WaitForSeconds
    {
        private float period_;
        private double start_;
        public WaitForSeconds(float period, double update_time)
        {
            period_ = period;
            start_ = update_time;
        }
        public bool end(double update_time)
        {
            return update_time - start_ > period_;
        }
    }

    public static void MirrorX(ref Quaternion q)
    {
        q.y = -q.y;
        q.z = -q.z;
    }

    public static Quaternion Inverse(ref Quaternion q)
    {
        return new Quaternion(-q.x, -q.y, -q.z, q.w);
    }

    public static Color Lerp3FacttorUnclamped(ref Color a, ref Color b, float t)
    {
        return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, 1f);        
    }

    public static string MatrixToString(ref Matrix4x4 mat)
    {
        return string.Format("{0} {1} {2} {3}\n{4} {5} {6} {7}\n{8} {9} {10} {11}\n{12} {13} {14} {15}",
                             mat.m00, mat.m01, mat.m02, mat.m03,
                             mat.m10, mat.m11, mat.m12, mat.m13,
                             mat.m20, mat.m21, mat.m22, mat.m23,
                             mat.m30, mat.m31, mat.m32, mat.m33);
    }

    public static void PlaneNormalize(this Vector4 vec)
    {
        float rlen = 1f/Mathf.Sqrt(vec.x*vec.x + vec.y*vec.y + vec.z*vec.z);
        vec.x *= rlen;
        vec.y *= rlen;
        vec.z *= rlen;
        vec.w *= rlen;
    }

    public static void GetPlanesFromFrustum(Vector4[] planes, ref Matrix4x4 vp)
    {
        Debug.Assert(planes.Length >= 6);
        int idx = 0;
        
        // left
        planes[idx].x = vp.m30 + vp.m00;
        planes[idx].y = vp.m31 + vp.m01;
        planes[idx].z = vp.m32 + vp.m02;
        planes[idx].w = vp.m33 + vp.m03;
        planes[idx].PlaneNormalize();
        ++idx;
        // right
        planes[idx].x = vp.m30 - vp.m00;
        planes[idx].y = vp.m31 - vp.m01;
        planes[idx].z = vp.m32 - vp.m02;
        planes[idx].w = vp.m33 - vp.m03;
        planes[idx].PlaneNormalize();
        ++idx;

        // bottom
        planes[idx].x = vp.m30 + vp.m10;
        planes[idx].y = vp.m31 + vp.m11;
        planes[idx].z = vp.m32 + vp.m12;
        planes[idx].w = vp.m33 + vp.m13;
        planes[idx].PlaneNormalize();
        ++idx;
        // top
        planes[idx].x = vp.m30 - vp.m10;
        planes[idx].y = vp.m31 - vp.m11;
        planes[idx].z = vp.m32 - vp.m12;
        planes[idx].w = vp.m33 - vp.m13;
        planes[idx].PlaneNormalize();
        ++idx;

        // near
        planes[idx].x = vp.m30 + vp.m20;
        planes[idx].y = vp.m31 + vp.m21;
        planes[idx].z = vp.m32 + vp.m22;
        planes[idx].w = vp.m33 + vp.m23;
        planes[idx].PlaneNormalize();
        ++idx;
        // far
        planes[idx].x = vp.m30 - vp.m20;
        planes[idx].y = vp.m31 - vp.m21;
        planes[idx].z = vp.m32 - vp.m22;
        planes[idx].w = vp.m33 - vp.m23;
        planes[idx].PlaneNormalize();
        ++idx;
    }

    public static bool InFrustum(Vector4[] planes, ref Vector3 pos, float radius)
    {
        for (var i = 0; i < 6; ++i) {
            if (planes[i].x * pos.x +
                planes[i].y * pos.y +
                planes[i].z * pos.z +
                planes[i].w <= -radius) {
                return false;
            }
        }
        return true;
    }
}

} // namespace UTJ {

/*
 * End of Utility.cs
 */

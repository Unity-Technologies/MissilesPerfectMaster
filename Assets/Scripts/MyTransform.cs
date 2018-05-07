/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public struct MyTransform
{
    public Vector3 position_;
    public Quaternion rotation_;
    private Matrix4x4 rot_matrix_;

    public void init()
    {
        init(ref CV.Vector3Zero, ref CV.QuaternionIdentity);
    }
    
    public void init(ref Vector3 position, ref Quaternion rotation)
    {
        position_ = position;
        rotation_ = rotation;
        update();
        rot_matrix_.m03 = 0f;
        rot_matrix_.m13 = 0f;
        rot_matrix_.m23 = 0f;
        rot_matrix_.m03 = 0f;
        rot_matrix_.m03 = 0f;
        rot_matrix_.m13 = 0f;
        rot_matrix_.m23 = 0f;
        rot_matrix_.m33 = 1f;
    }

    public void setRotation(ref Quaternion q)
    {
        rotation_ = q;
        update();
    }

    public void setPositionAndRotation(ref MyTransform tfm)
    {
        position_ = tfm.position_;
        rotation_ = tfm.rotation_;
        update();
    }

    public void multiplyRotationFromLeft(ref Quaternion q)
    {
        rotation_ = q * rotation_;
        update();
    }

    public void normalize()
    {
        var v2 = rotation_.x * rotation_.x;
        v2 += rotation_.y * rotation_.y;
        v2 += rotation_.z * rotation_.z;
        v2 += rotation_.w * rotation_.w;
        if (v2 == 0) {
            rotation_.w = 1f;
        } else {
            float inv = 1f / Mathf.Sqrt(v2);
            rotation_.x *= inv;
            rotation_.y *= inv;
            rotation_.z *= inv;
            rotation_.w *= inv;
        }
        update();
    }

    private void update()
    {
        // 1 - 2*qy2 - 2*qz2     2*qx*qy - 2*qz*qw     2*qx*qz + 2*qy*qw
        // 2*qx*qy + 2*qz*qw     1 - 2*qx2 - 2*qz2     2*qy*qz - 2*qx*qw
        // 2*qx*qz - 2*qy*qw     2*qy*qz + 2*qx*qw     1 - 2*qx2 - 2*qy2
        var qx22 = 2f * rotation_.x * rotation_.x;
        var qy22 = 2f * rotation_.y * rotation_.y;
        var qz22 = 2f * rotation_.z * rotation_.z;
        var qxy2 = 2f * rotation_.x * rotation_.y;
        var qzw2 = 2f * rotation_.z * rotation_.w;
        var qxz2 = 2f * rotation_.x * rotation_.z;
        var qyw2 = 2f * rotation_.y * rotation_.w;
        var qyz2 = 2f * rotation_.y * rotation_.z;
        var qxw2 = 2f * rotation_.x * rotation_.w;
        rot_matrix_.m00 = 1f - qy22 - qz22;
        rot_matrix_.m01 = qxy2 - qzw2;
        rot_matrix_.m02 = qxz2 + qyw2;
        rot_matrix_.m10 = qxy2 + qzw2;
        rot_matrix_.m11 = 1f - qx22 - qz22;
        rot_matrix_.m12 = qyz2 - qxw2;
        rot_matrix_.m20 = qxz2 - qyw2;
        rot_matrix_.m21 = qyz2 + qxw2;
        rot_matrix_.m22 = 1f - qx22 - qy22;
    }

    public Vector3 transformPosition(ref Vector3 pos)
    {
        return new Vector3(rot_matrix_.m00 * pos.x + rot_matrix_.m01 * pos.y + rot_matrix_.m02 * pos.z + position_.x,
                           rot_matrix_.m10 * pos.x + rot_matrix_.m11 * pos.y + rot_matrix_.m12 * pos.z + position_.y,
                           rot_matrix_.m20 * pos.x + rot_matrix_.m21 * pos.y + rot_matrix_.m22 * pos.z + position_.z);
    }

    public Vector3 transformPositionZ(float z)
    {
        return new Vector3(rot_matrix_.m02 * z + position_.x,
                           rot_matrix_.m12 * z + position_.y,
                           rot_matrix_.m22 * z + position_.z);
    }

    public Vector3 transformVector(ref Vector3 dir)
    {
        return new Vector3(rot_matrix_.m00 * dir.x + rot_matrix_.m01 * dir.y + rot_matrix_.m02 * dir.z,
                           rot_matrix_.m10 * dir.x + rot_matrix_.m11 * dir.y + rot_matrix_.m12 * dir.z,
                           rot_matrix_.m20 * dir.x + rot_matrix_.m21 * dir.y + rot_matrix_.m22 * dir.z);
    }

    public Vector3 transformVectorX(float x)
    {
        return new Vector3(rot_matrix_.m00 * x,
                           rot_matrix_.m10 * x,
                           rot_matrix_.m20 * x);
    }

    public Vector3 transformVectorXY(float x, float y)
    {
        return new Vector3(rot_matrix_.m00 * x + rot_matrix_.m01 * y,
                           rot_matrix_.m10 * x + rot_matrix_.m11 * y,
                           rot_matrix_.m20 * x + rot_matrix_.m21 * y);
    }

    public Vector3 transformVectorXZ(float x, float z)
    {
        return new Vector3(rot_matrix_.m00 * x + rot_matrix_.m02 * z,
                           rot_matrix_.m10 * x + rot_matrix_.m12 * z,
                           rot_matrix_.m20 * x + rot_matrix_.m22 * z);
    }

    public Vector3 transformVectorZ(float z)
    {
        return new Vector3(rot_matrix_.m02 * z,
                           rot_matrix_.m12 * z,
                           rot_matrix_.m22 * z);
    }

    public Quaternion getInverseRotation() {
        return Utility.Inverse(ref rotation_);
    }

    public Matrix4x4 getTRS()
    {
        return Matrix4x4.TRS(position_, rotation_, new Vector3(1f, 1f, 1f));
    }

    public Matrix4x4 getInverseR()
    {
        var mat_rot = Matrix4x4.TRS(CV.Vector3Zero,
                                    rotation_,
                                    CV.Vector3One);
        var mat = mat_rot.transpose;
        return mat;
    }

    public MyTransform add(ref Vector3 offset)
    {
        var transform = new MyTransform();
        transform.position_ = transformPosition(ref offset);
        transform.rotation_ = rotation_;
        transform.rot_matrix_ = rot_matrix_;
        return transform;
    }

    public void getLocalToWorldMatrix(ref Matrix4x4 mat)
    {
        mat.SetTRS(position_, rotation_, CV.Vector3One);
    }
}

} // namespace UTJ {

/*
 * End of MyTransform.cs
 */

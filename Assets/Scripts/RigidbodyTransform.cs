/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public struct RigidbodyTransform
{
    public MyTransform transform_;
    public Vector3 velocity_;
    private Vector3 acceleration_;
    public float damper_;
    public Vector3 r_velocity_;
    public Vector3 r_acceleration_;
    public float r_damper_;

    public void init()
    {
        init(ref CV.Vector3Zero, ref CV.QuaternionIdentity); 
    }
    public void init(ref Vector3 position, ref Quaternion rotation)
    {
        transform_.init(ref position, ref rotation);
        velocity_ = CV.Vector3Zero;
        acceleration_ = CV.Vector3Zero;
        damper_ = 0f;
        r_velocity_ = CV.Vector3Zero;
        r_acceleration_ = CV.Vector3Zero;
        r_damper_ = 0f;
    }

    public void setPositionAndRotation(ref MyTransform tfm)
    {
        transform_.setPositionAndRotation(ref tfm);
        velocity_ = CV.Vector3Zero;
        acceleration_ = CV.Vector3Zero;
        r_velocity_ = CV.Vector3Zero;
        r_acceleration_ = CV.Vector3Zero;
    }

    public void setPosition(ref Vector3 pos)
    {
        transform_.position_ = pos;
        velocity_ = CV.Vector3Zero;
        acceleration_ = CV.Vector3Zero;
    }
    public void setPosition(float x, float y, float z)
    {
        transform_.position_.x = x;
        transform_.position_.y = y;
        transform_.position_.z = z;
        velocity_ = CV.Vector3Zero;
        acceleration_ = CV.Vector3Zero;
    }

    public void setRotation(ref Quaternion rot)
    {
        // transform_.rotation_ = rot;
        transform_.setRotation(ref rot);
        r_velocity_ = CV.Vector3Zero;
        r_acceleration_ = CV.Vector3Zero;
    }

    public void setDamper(float damper)
    {
        damper_ = damper;
    }

    public void setRotateDamper(float damper)
    {
        r_damper_ = damper;
    }

    public void addForce(ref Vector3 v)
    {
        acceleration_.x += v.x;
        acceleration_.y += v.y;
        acceleration_.z += v.z;
    }
    public void addForceX(float v)
    {
        acceleration_.x += v;
    }
    public void addForceY(float v)
    {
        acceleration_.y += v;
    }
    public void addForceZ(float v)
    {
        acceleration_.z += v;
    }
    public void addForceXY(float x, float y)
    {
        acceleration_.x += x;
        acceleration_.y += y;
    }

    public void addRelativeForceX(float x)
    {
        var force = transform_.transformVectorX(x);
        addForce(ref force);
    }
    public void addRelativeForceXY(float x, float y)
    {
        var force = transform_.transformVectorXY(x, y);
        addForce(ref force);
    }
    public void addRelativeForceXZ(float x, float z)
    {
        var force = transform_.transformVectorXZ(x, z);
        addForce(ref force);
    }
    public void addRelativeForceZ(float z)
    {
        var force = transform_.transformVectorZ(z);
        addForce(ref force);
    }
    public void addRelativeForce(ref Vector3 power)
    {
        var force = transform_.transformVector(ref power);
        addForce(ref force);
    }

    public void setAcceleration(ref Vector3 a)
    {
        acceleration_ = a;
    }

    public void setVelocity(ref Vector3 v)
    {
        velocity_ = v;
    }
    public void setVelocity(float x, float y, float z)
    {
        velocity_.x = x;
        velocity_.y = y;
        velocity_.z = z;
    }
    public void setVelocity(Vector3 velocity)
    {
        velocity_ = velocity;
    }

    public void addTorque(ref Vector3 torque)
    {
        r_acceleration_.x += torque.x;
        r_acceleration_.y += torque.y;
        r_acceleration_.z += torque.z;
    }
    public void addTorque(float x, float y, float z)
    {
        r_acceleration_.x += x;
        r_acceleration_.y += y;
        r_acceleration_.z += z;
    }
    public void addTorqueX(float torque)
    {
        r_acceleration_.x += torque;
    }
    public void addTorqueY(float torque)
    {
        r_acceleration_.y += torque;
    }
    public void addTorqueZ(float torque)
    {
        r_acceleration_.z += torque;
    }
    public void addRelativeTorqueXY(float x, float y)
    {
        var t = transform_.transformVectorXY(x, y);
        r_acceleration_ += t;
    }
    public void addRelativeTorqueX(float torque)
    {
        var t = transform_.transformVectorX(torque);
        r_acceleration_ += t;
    }
    public void addRelativeTorqueZ(float torque)
    {
        var t = transform_.transformVectorZ(torque);
        r_acceleration_ += t;
    }

    public void solveForGround(float ground, float dt)
    {
        float predicted_y = transform_.position_.y + ((velocity_.y + (acceleration_.y * dt)) * dt);
        if (predicted_y > ground)
            return;
        velocity_.y = (ground - transform_.position_.y)/dt;
        acceleration_.y = 0f;
    }

    public void update(float dt)
    {
        velocity_.x += acceleration_.x * dt;
        velocity_.y += acceleration_.y * dt;
        velocity_.z += acceleration_.z * dt;
        acceleration_.x = acceleration_.y = acceleration_.z = 0f; // clear acceleration
        // apply dampler
        velocity_.x -= velocity_.x * damper_ * dt;
        velocity_.y -= velocity_.y * damper_ * dt;
        velocity_.z -= velocity_.z * damper_ * dt;
        // update position
        transform_.position_.x += velocity_.x * dt;
        transform_.position_.y += velocity_.y * dt;
        transform_.position_.z += velocity_.z * dt;

        /*
         * for rotation
         */
        // update velocity
        r_velocity_.x += r_acceleration_.x * dt;
        r_velocity_.y += r_acceleration_.y * dt;
        r_velocity_.z += r_acceleration_.z * dt;
        r_acceleration_.x = r_acceleration_.y = r_acceleration_.z = 0f; // clear acceleration
        // apply dampler
        r_velocity_.x -= r_velocity_.x * r_damper_ * dt;
        r_velocity_.y -= r_velocity_.y * r_damper_ * dt;
        r_velocity_.z -= r_velocity_.z * r_damper_ * dt;
        // update rotation
        var nx = r_velocity_.x * dt;
        var ny = r_velocity_.y * dt;
        var nz = r_velocity_.z * dt;
        var len2 = nx*nx + ny*ny + nz*nz; // sin^2
        var w = Mathf.Sqrt(1f - len2); // (sin^2 + cos^2) = 1
        var q = new Quaternion(nx, ny, nz, w);
        transform_.multiplyRotationFromLeft(ref q);
        // normalize
        transform_.normalize();
    }

    public void addSpringForce(ref Vector3 target, float ratio)
    {
        addForceX((target.x - transform_.position_.x) * ratio);
        addForceY((target.y - transform_.position_.y) * ratio);
        addForceZ((target.z - transform_.position_.z) * ratio);
    }
    public void addSpringForceX(float target_x, float ratio)
    {
        addForceX((target_x - transform_.position_.x) * ratio);
    }
    public void addSpringForceY(float target_y, float ratio)
    {
        addForceY((target_y - transform_.position_.y) * ratio);
    }
    public void addSpringForceZ(float target_z, float ratio)
    {
        addForceZ((target_z - transform_.position_.z) * ratio);
    }
    public void addSpringForceXY(float target_x, float target_y, float ratio)
    {
        addForceX((target_x - transform_.position_.x) * ratio);
        addForceY((target_y - transform_.position_.y) * ratio);
    }

    public void addTargetTorque(ref Vector3 target, float torque_level)
    {
        addTargetTorque(ref target, torque_level, -1f /* max_level */);
    }
    public void addTargetTorque(ref Vector3 target, float torque_level, float max_level)
    {
        var diff = target - transform_.position_;
        diff.Normalize();
        addOrientTorque(ref diff, torque_level, max_level);
    }

    public void addOrientTorque(ref Vector3 dir, float torque_level)
    {
        addOrientTorque(ref dir, torque_level, -1f /* max level */);
    }
    public void addOrientTorque(ref Vector3 dir, float torque_level, float max_level)
    {
        var forward = transform_.transformVectorZ(1f);
        var torque = Vector3.Cross(forward, dir);
        torque.x *= torque_level;
        torque.y *= torque_level;
        torque.z *= torque_level;
        if (max_level > 0f) {
            var level = torque.magnitude;
            if (max_level < level) {
                float r = max_level/level;
                torque *= r;
            }
        }
        addTorque(ref torque);
    }

    public void addHorizontalStableTorque(float torque_level)
    {
        var left = transform_.transformVector(ref CV.Vector3Left);
        var hori_left = new Vector3(left.x, 0f, left.z).normalized;
        var tq0 = Vector3.Cross(left, hori_left) * torque_level;
        addTorque(ref tq0);
        var forward = transform_.transformVector(ref CV.Vector3Forward);
        var hori_forward = new Vector3(forward.x, 0f, forward.z).normalized;
        var tq1 = Vector3.Cross(forward, hori_forward) * torque_level;
        addTorque(ref tq1);
    }

    public void addSpringTorque(ref Vector3 target, float torque_level)
    {
        var diff = target - transform_.position_;
        var q = Quaternion.LookRotation(diff);
        addSpringTorque(ref q, torque_level);
    }

    public void addSpringTorqueCalcUp(ref Vector3 target, float torque_level)
    {
        var diff = target - transform_.position_;
        var up = transform_.transformVector(ref CV.Vector3Up);
        var q = Quaternion.LookRotation(diff, up);
        addSpringTorque(ref q, torque_level);
    }

    public void addSpringTorque(ref Quaternion target, float torque_level)
    {
        /**
         * Quaternion(x, y, z, w)
         * q.x = sin(theta/2)*ax
         * q.y = sin(theta/2)*ay
         * q.z = sin(theta/2)*az
         * q.w = cos(theta/2)
         */
// #if false
#if true
        // reference
        // var q = target * Utility.Inverse(ref transform_.rotation_);
        var q = target * transform_.getInverseRotation();
        if (q.w < 0f) {            // over 180 degree, take shorter way!
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        var torque = new Vector3(q.x, q.y, q.z) * torque_level;
        addTorque(ref torque);
#else
        // optimized
#if true
        // faster than below
        var qx = target.w * (-transform_.rotation_.x) + target.x * transform_.rotation_.w + target.y * (-transform_.rotation_.z) - target.z * (-transform_.rotation_.y);
        var qy = target.w * (-transform_.rotation_.y) + target.y * transform_.rotation_.w + target.z * (-transform_.rotation_.x) - target.x * (-transform_.rotation_.z);
        var qz = target.w * (-transform_.rotation_.z) + target.z * transform_.rotation_.w + target.x * (-transform_.rotation_.y) - target.y * (-transform_.rotation_.x);
        var qw = target.w * transform_.rotation_.w - target.x * (-transform_.rotation_.x) - target.y * (-transform_.rotation_.y) - target.z * (-transform_.rotation_.z);
#else
        // slower than above. because IL won't use registers as expected..
        var lx = target.x;
        var ly = target.y;
        var lz = target.z;
        var lw = target.w;
        var rx = -transform_.rotation_.x;
        var ry = -transform_.rotation_.y;
        var rz = -transform_.rotation_.z;
        var rw = transform_.rotation_.w;
        var qx = lw * rx + lx * rw + ly * rz - lz * ry;
        var qy = lw * ry + ly * rw + lz * rx - lx * rz;
        var qz = lw * rz + lz * rw + lx * ry - ly * rx;
        var qw = lw * rw - lx * rx - ly * ry - lz * rz;
#endif
        if (qw < 0f) {
            qx = -qx;
            qy = -qy;
            qz = -qz;
            qw = -qw;
        }
        addTorque(qx*torque_level, qy*torque_level, qz*torque_level);
#endif
    }

    public void updateVerlet(float dt, float damper)
    {
        var tmp_x = transform_.position_.x;
        var tmp_y = transform_.position_.y;
        var tmp_z = transform_.position_.z;
        var diff_x = tmp_x - velocity_.x;
        var diff_y = tmp_y - velocity_.y;
        var diff_z = tmp_z - velocity_.z;
        var dt2 = dt*dt;
        transform_.position_.x += diff_x * damper + acceleration_.x * dt2;
        transform_.position_.y += diff_y * damper + acceleration_.y * dt2;
        transform_.position_.z += diff_z * damper + acceleration_.z * dt2;
        acceleration_ = CV.Vector3Zero;
        velocity_.x = tmp_x;
        velocity_.y = tmp_y;
        velocity_.z = tmp_z;
    }

    public void restrictPositionVerletFixed(ref Vector3 pos, float length)
    {
        var diff = pos - transform_.position_;
        var len = diff.magnitude;
        if (len > 0f) {
            diff *= (1f - length/len);
            transform_.position_ += diff * 0.5f;
        }
    }
}

} // namespace UTJ {

/*
 * End of RigidbodyTransform.cs
 */

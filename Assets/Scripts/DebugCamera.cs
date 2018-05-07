/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public class DebugCamera : CameraBase
{
    private RigidbodyTransform rigidbody_;

    public static DebugCamera create()
    {
        var camera = new DebugCamera();
        camera.init();
        return camera;
    }

    public override void init()
    {
        base.init();
        rigidbody_.setPosition(ref CV.Vector3Zero);
        rigidbody_.setRotation(ref CV.QuaternionIdentity);
        rigidbody_.setRotateDamper(10f);
    }

    public override void update(float dt, double update_time)
    {
        update_in_pause(1f/60f);
    }

    public void setup(CameraBase prev_camera)
    {
        rigidbody_.setPositionAndRotation(ref prev_camera.transform_);
    }

    public void update_in_pause(float dt)
    {
        float hori = InputManager.Instance.getAnalog(InputManager.Button.Horizontal);
        float vert = InputManager.Instance.getAnalog(InputManager.Button.Vertical);
        if (InputManager.Instance.isButton(InputManager.Button.Right)) {
            rigidbody_.addRelativeForceXZ(hori*100f, vert*100f);
            var f = -rigidbody_.velocity_*10f;
            rigidbody_.addForce(ref f);
        } else {
            rigidbody_.addRelativeTorqueXY(-vert*8f, hori*8f);
            if (InputManager.Instance.isButton(InputManager.Button.Left)) {
                rigidbody_.addSpringTorque(ref CV.QuaternionIdentity, 20f);
            }
        }
        if (!InputManager.Instance.isButton(InputManager.Button.Left) &&
            !InputManager.Instance.isButton(InputManager.Button.Right)) {
            var f = -rigidbody_.velocity_*100f;
            rigidbody_.addForce(ref f);
        }
        rigidbody_.update(dt);

    }

    public override void renderUpdate(int front, CameraBase dummy, ref DrawBuffer draw_buffer)
    {
        applyTransform(ref rigidbody_.transform_.position_, ref rigidbody_.transform_.rotation_);
    }
}

} // namespace UTJ {

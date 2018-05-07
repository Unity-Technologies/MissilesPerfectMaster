/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public abstract class CameraBase : Task
{
    private const float SCREEN_HEIGHT = 360f; // size of FinalCamera
    public bool active_;
    public MyTransform transform_;
    private Matrix4x4 screen_matrix_;

    public override void init()
    {
        base.init();
        transform_.init();
        active_ = true;
    }

    public void applyTransform(ref Vector3 pos, ref Quaternion rot)
    {
        transform_.position_ = pos;
        transform_.rotation_ = rot;
    }

    public override void renderUpdate(int front, CameraBase dummy, ref DrawBuffer draw_buffer)
    {
        if (active_) {
            draw_buffer.registCamera(ref transform_);
            var view_matrix = transform_.getTRS();
            var projection_matrix = SystemManager.Instance.ProjectionMatrix;
            screen_matrix_ = projection_matrix * view_matrix.inverse;
        }
    }

    public Vector3 getScreenPoint(ref Vector3 world_position)
    {
        var v = screen_matrix_.MultiplyPoint(world_position);
        float w = SCREEN_HEIGHT*((float)Screen.width/(float)Screen.height);
        float h = SCREEN_HEIGHT;
        return new Vector3(v.x * (-w), v.y * (-h), v.z);
    }
}

} // namespace UTJ {

/*
 * End of CameraBase.cs
 */

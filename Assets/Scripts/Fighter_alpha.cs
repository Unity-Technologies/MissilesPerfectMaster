/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public partial class Fighter : Task
{
    private double hit_time_;

    public void alpha_init(ref Vector3 position, ref Quaternion rotation)
    {
        rigidbody_.init(ref position, ref rotation);
        MissileManager.Instance.setTargetRadius(target_id_, 1f /* radius */);
        enumerator_ = alpha_act(); // この瞬間は実行されない
        on_update_ = new OnUpdateFunc(alpha_update);
        on_render_update_ = new OnRenderUpdateFunc(alpha_render_update);
        // life_ = 50f;
        rigidbody_.setDamper(10f);
        rigidbody_.setRotateDamper(1f);
        hit_time_ = -99999;
    }

    public IEnumerator alpha_act()
    {
        for (;;) {
            yield return null;
        }
    }

    private void alpha_normal_act(float dt, double update_time)
    {
        if (target_fighter_ == null || MyRandom.Probability(0.025f, dt)) {
            target_fighter_ = Fighter.searchFarest(fighter_id_, ref rigidbody_.transform_.position_);
        }
        if (target_fighter_ != null) {
            rigidbody_.addSpringTorqueCalcUp(ref target_fighter_.rigidbody_.transform_.position_,
                                             2f /* torque_level */);
            rigidbody_.addSpringTorqueCalcUp(ref CV.Vector3Zero,
                                             0.1f /* torque_level */);
            rigidbody_.addRelativeForceZ(250f);
            var roti = rigidbody_.transform_.getInverseRotation();
            var rvec = roti * rigidbody_.r_velocity_;
            rigidbody_.addRelativeTorqueZ(rvec.y*2f);
            if (MyRandom.Probability(3.5f/6f, dt)) {
                var offset = new Vector3(0f, -0.3f, 0f);
                var lpos = rigidbody_.transform_.transformPosition(ref offset);
                var num = MyRandom.Range(6, 8);
                for (var i = 0; i < num; ++i) {
                    MissileManager.Instance.spawn(ref lpos,
                                                  ref rigidbody_.transform_.rotation_,
                                                  target_fighter_.target_id_, update_time);
                }
                Spark.Instance.spawn(ref lpos, Spark.Type.Bullet, update_time);
            }
            if (MyRandom.Probability(0.2f, dt)) {
                rigidbody_.addRelativeTorqueX(MyRandom.Probability(0.5f) ? 20f : -20f);
            }
        }
    }

    private void alpha_update(float dt, double update_time)
    {
        if (MissileManager.Instance.checkHitAndClear(target_id_)) {
            hit_time_ = update_time;
        }
        if (update_time - hit_time_ < 0.5f) {
            rigidbody_.setRotateDamper(0.2f);
            if (update_time - hit_time_ < 0.017f) {
                var torque = MyRandom.onSphere(25f);
                rigidbody_.addTorque(ref torque);
            }
        } else {
            rigidbody_.setRotateDamper(1f);
            alpha_normal_act(dt, update_time);
        }

        rigidbody_.update(dt);
    }

    private void alpha_render_update(int front, ref DrawBuffer draw_buffer)
    {
        draw_buffer.regist(ref rigidbody_.transform_, DrawBuffer.Type.FighterAlpha);
    }

    public float getHitElapsed(double update_time)
    {
        return (float)(update_time - hit_time_);
    }
}

} // namespace UTJ {

/*
 * End of Fighter_alpha.cs
 */

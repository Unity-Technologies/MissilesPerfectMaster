/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace UTJ {

public partial class Fighter : Task
{
    const int POOL_MAX = 256;
    private static Fighter[] pool_;
    private static int pool_index_;
    private static int[] target_table_;

    public static void createPool()
    {
        pool_ = new Fighter[POOL_MAX];
        for (var i = 0; i < POOL_MAX; ++i) {
            var fighter = new Fighter();
            fighter.fighter_id_ = i;
            fighter.alive_ = false;
            pool_[i] = fighter;
        }
        pool_index_ = 0;

        target_table_ = new int[POOL_MAX];
        for (var i = 0; i < POOL_MAX; ++i) {
            target_table_[i] = -1;
        }
    }
    

    public enum Type {
        None,
        Alpha,
    }
    private enum Phase {
        Alive,
        Dying,
    }

    private int fighter_id_;
    private int target_id_;
    public RigidbodyTransform rigidbody_;
    private IEnumerator enumerator_;
    private double update_time_;
    private Phase phase_;
    private Vector3 target_position_;
    private Fighter target_fighter_;
    private delegate void OnUpdateFunc(float dt, double update_time);
    private OnUpdateFunc on_update_;
    private delegate void OnRenderUpdateFunc(int front, ref DrawBuffer draw_buffer);
    private OnRenderUpdateFunc on_render_update_;

    public static Fighter create(Type type, ref Vector3 position, ref Quaternion rotation, double update_time)
    {
        Fighter fighter = Fighter.create(update_time);
        fighter.phase_ = Phase.Alive;
        fighter.init();
        switch (type) {
            case Type.None:
                Debug.Assert(false);
                break;
            case Type.Alpha:
                fighter.alpha_init(ref position, ref rotation);
                break;
        }
        return fighter;
    }

    private static Fighter create(double update_time)
    {
        int cnt = 0;
        while (pool_[pool_index_].alive_) {
            ++pool_index_;
            if (pool_index_ >= POOL_MAX)
                pool_index_ = 0;
            ++cnt;
            if (cnt >= POOL_MAX) {
                Debug.LogError("EXCEED Fighter POOL!");
                break;
            }
        }
        var fighter_id = pool_index_;
        var fighter = pool_[fighter_id];
        int target_id = MissileManager.Instance.registTarget(update_time);
        fighter.target_id_ = target_id;
        fighter.target_fighter_ = null;
        target_table_[fighter.target_id_] = fighter_id;
        return fighter;
    }

    private void calc_lock_position_center(ref Vector3 position)
    {
        position = rigidbody_.transform_.position_;
    }

    public override void destroy()
    {
        target_table_[target_id_] = -1;
        MissileManager.Instance.killTarget(target_id_, update_time_);
        enumerator_ = null;
        base.destroy();
    }

    public override void update(float dt, double update_time)
    {
        if (phase_ == Phase.Dying) {
            destroy();
            return;
        }

        update_time_ = update_time;
        if (enumerator_ != null) {
            enumerator_.MoveNext();
        }
        if (alive_) {
            on_update_(dt, update_time);
            MissileManager.Instance.updateTarget(target_id_,
                                                 ref rigidbody_.transform_.position_);
        }
    }

    public override void renderUpdate(int front, CameraBase camera, ref DrawBuffer draw_buffer)
    {
        on_render_update_(front, ref draw_buffer);
    }


    public static Fighter searchClosest(ref Vector3 pos, Fighter exclude = null)
    {
        int exclude_fighter_id = -1;
        if (exclude != null) {
            exclude_fighter_id = exclude.fighter_id_;
        }
        return searchClosest(exclude_fighter_id, ref pos);
    }
    public static Fighter searchClosest(int exclude_fighter_id, ref Vector3 pos)
    {
        Profiler.BeginSample("searchClosest");
        Fighter result_fighter = null;
        float max_value = System.Single.MaxValue;
        for (var i = 0; i < POOL_MAX; ++i) {
            if (i == exclude_fighter_id) {
                continue;
            }
            Fighter fighter = pool_[i];
            if (fighter.alive_) {
                var diff = fighter.rigidbody_.transform_.position_ - pos;
                var len2 = diff.x*diff.x + diff.y*diff.y + diff.z*diff.z;
                if (len2 < max_value) {
                    result_fighter = fighter;
                    max_value = len2;
                }
            }
        }
        Profiler.EndSample();
        return result_fighter;
    }

    public static Fighter searchFarest(ref Vector3 pos)
    {
        return searchFarest(-1, ref pos);
    }
    public static Fighter searchFarest(int exclude_fighter_id, ref Vector3 pos)
    {
        Profiler.BeginSample("searchFarest");
        Fighter result_fighter = null;
        float min_value = 0f;
        for (var i = 0; i < POOL_MAX; ++i) {
            if (i == exclude_fighter_id) {
                continue;
            }
            Fighter fighter = pool_[i];
            if (fighter.alive_) {
                var diff = fighter.rigidbody_.transform_.position_ - pos;
                var len2 = diff.x*diff.x + diff.y*diff.y + diff.z*diff.z;
                if (len2 > min_value) {
                    result_fighter = fighter;
                    min_value = len2;
                }
            }
        }
        Profiler.EndSample();
        return result_fighter;
    }
}

} // namespace UTJ {

/*
 * End of Fighter.cs
 */

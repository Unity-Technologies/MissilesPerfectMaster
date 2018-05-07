/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTJ {

public class GameManager
{
    // singleton
    static GameManager instance_;
    public static GameManager Instance { get { return instance_ ?? (instance_ = new GameManager()); } }

    private IEnumerator enumerator_;
    private double update_time_;

    public void init(bool debug)
    {
        if (!debug) {
            enumerator_ = act();    // この瞬間は実行されない
        } else {
            enumerator_ = act_debug();
        }
    }

    public void update(float dt, double update_time)
    {
        update_time_ = update_time;
        enumerator_.MoveNext();
    }

    private IEnumerator act()
    {
        const float RANGE = 300f;
        for (var i = 0; i < 100; ++i) {
            var position = new Vector3(MyRandom.Probability(0.5f) ? -RANGE : RANGE,
                                       MyRandom.Range(-RANGE, RANGE),
                                       MyRandom.Range(-RANGE, RANGE));
            var rotation = Quaternion.LookRotation(MyRandom.onSphere(1f));
            Fighter.create(Fighter.Type.Alpha, ref position, ref rotation, update_time_);
        }
        for (;;) {
            yield return null;
        }
    }

    private IEnumerator act_debug()
    {
        int target_id = MissileManager.Instance.registTarget(update_time_);
        MissileManager.Instance.setTargetRadius(target_id, 1f /* radius */);
        {
            var pos = new Vector3(0f, 0f, 10f);
            MissileManager.Instance.updateTarget(target_id, ref pos);
        }
        for (;;) {
            var lpos = new Vector3(0f, 0f, -40f);
            var rot = Quaternion.Euler(0f, -30f, 0f);
            MissileManager.Instance.spawn(ref lpos,
                                          ref rot,
                                          target_id, update_time_);
            for (var i = new Utility.WaitForSeconds(2f, update_time_); !i.end(update_time_);) {
                var pos = new Vector3(100f, 0f, 10f);
                MissileManager.Instance.updateTarget(target_id, ref pos);
                MissileManager.Instance.checkHitAndClear(target_id);
                yield return null;
            }
        }
    }
}

} // namespace UTJ {

/*
 * End of GameManager.cs
 */

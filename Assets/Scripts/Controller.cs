/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

public class Controller
{
    // singleton
    static Controller instance_;
    public static Controller Instance { get { return instance_ ?? (instance_ = new Controller()); } }

    public struct Control
    {
        public int horizontal_;
        public bool left_button_;
        public bool right_button_;
        public bool jump_button_;
        public bool pause_button_;
        public bool left_button_down_;
        public bool right_button_down_;
        public bool jump_button_down_;
        public bool pause_button_down_;
        public bool left_button_up_;
        public bool right_button_up_;
        public bool jump_button_up_;
        public bool pause_button_up_;
        public float flick_x_;
        public float flick_y_;

        public void clear()
        {
            horizontal_ = 0;
            left_button_ = false;
            right_button_ = false;
            jump_button_ = false;
            pause_button_ = false;
            left_button_down_ = false;
            right_button_down_ = false;
            jump_button_down_ = false;
            pause_button_down_ = false;
            left_button_up_ = false;
            right_button_up_ = false;
            jump_button_up_ = false;
            pause_button_up_ = false;
            flick_x_ = 0f;
            flick_y_ = 0f;
        }

        public float getHorizontal()
        {
            return ((float)horizontal_)*(1f/127f);
        }
        
        public bool isLeftButton()      { return left_button_; }
        public bool isRightButton()     { return right_button_; }
        public bool isJumpButton()      { return jump_button_; }
        public bool isPauseButton()     { return pause_button_; }
        public bool isLeftButtonDown()  { return left_button_down_; }
        public bool isRightButtonDown()    { return right_button_down_; }
        public bool isJumpButtonDown()    { return jump_button_down_; }
        public bool isPauseButtonDown()    { return pause_button_down_; }
        public bool isLeftButtonUp()    { return left_button_up_; }
        public bool isRightButtonUp()    { return right_button_up_; }
        public bool isJumpButtonUp()    { return jump_button_up_; }
        public bool isPauseButtonUp()    { return pause_button_up_; }

        public void fetch_device()
        {
            float hori = InputManager.Instance.getAnalog(InputManager.Button.Horizontal);
            horizontal_ = (int)(hori*127);
            left_button_ = InputManager.Instance.isButton(InputManager.Button.Left);
            right_button_ = InputManager.Instance.isButton(InputManager.Button.Right);
            jump_button_ = InputManager.Instance.isButton(InputManager.Button.Jump);

            if (InputManager.Instance.touched(0 /* index */)) {
                var touched_position0 = InputManager.Instance.getTouchedPosition(0 /* index */);
                if (touched_position0.x > 0f) {
                    horizontal_ = (int)(1f*127);
                } else {
                    horizontal_ = (int)(-1f*127);
                }
                if (touched_position0.y < 0f) {
                    if (touched_position0.x > 0f) {
                        right_button_ = true;
                    } else {
                        left_button_ = true;
                    }
                }
                if (InputManager.Instance.touched(1 /* index */)) {
                    var touched_position1 = InputManager.Instance.getTouchedPosition(1 /* index */);
                    if ((touched_position0.x > 0f && touched_position1.x < 0f) ||
                        (touched_position0.x < 0f && touched_position1.x > 0f)) {
                        jump_button_ = true;
                    }
                }
            }
            var vec = InputManager.Instance.getFlickVector();
            flick_x_ = vec.x * 256f;
            flick_y_ = vec.y * 256f;
        }

        public void fetch_pause_button()
        {
            if (InputManager.Instance.touched(0 /* index */)) {
                pause_button_ = true;
            } else {
                pause_button_ = false;
            }
        }
    }

    public Control latest_;
    private IEnumerator enumerator_;
    private double update_time_;

    public void init(bool auto)
    {
        latest_.clear();
        set(auto);
    }

    public void set(bool auto)
    {
        if (auto) {
            enumerator_ = fetch_auto();
        } else {
            enumerator_ = null;
        }
    }

    private void fetch_auto_update()
    {
        float time = Mathf.Repeat((float)update_time_, 32f);
        if (time < 4f) {
            latest_.horizontal_ = -127;
        } else if (time < 8f) {
            latest_.horizontal_ = 127;
        } else if (time < 10f) {
            latest_.horizontal_ = 0;
        } else if (time < 16f) {
            latest_.horizontal_ = 127;
        } else if (time < 20f) {
            latest_.horizontal_ = -127;
        } else if (time < 24f) {
            latest_.horizontal_ = 127;
        } else if (time < 25f) {
            latest_.horizontal_ = -127;
        } else if (time < 32f) {
            latest_.horizontal_ = 127;
        }
    }

    private IEnumerator fetch_auto()
    {
        for (;;) {
            for (var w = new Utility.WaitForSeconds(MyRandom.Range(2f, 4f), update_time_);
                 !w.end(update_time_);) {
                if (latest_.horizontal_ < 0f) {
                    for (var w0 = new Utility.WaitForSeconds(MyRandom.Range(0.1f, 1f), update_time_);
                         !w0.end(update_time_);) {
                        latest_.left_button_ = true;
                        yield return null;
                    }
                } else {
                    for (var w0 = new Utility.WaitForSeconds(MyRandom.Range(0.1f, 1f), update_time_);
                         !w0.end(update_time_);) {
                        latest_.right_button_ = true;
                        yield return null;
                    }
                }
                for (var w0 = new Utility.WaitForSeconds(MyRandom.Range(0.1f, 0.6f), update_time_);
                     !w0.end(update_time_);) {
                    yield return null;
                }
                yield return null;
            }
            if (MyRandom.Probability(0.5f)) {
                for (var w = new Utility.WaitForSeconds(MyRandom.Range(0.8f, 1.2f), update_time_);
                     !w.end(update_time_);) {
                    latest_.jump_button_ = true;
                    yield return null;
                }
            }
            if (MyRandom.Probability(0.1f)) {
                for (var w = new Utility.WaitForSeconds(MyRandom.Range(3f, 4f), update_time_);
                     !w.end(update_time_);) {
                    for (var w0 = new Utility.WaitForSeconds(MyRandom.Range(0.2f, 0.4f), update_time_);
                         !w0.end(update_time_);) {
                        latest_.left_button_ = true;
                        yield return null;
                    }
                    for (var w0 = new Utility.WaitForSeconds(MyRandom.Range(0.2f, 0.4f), update_time_);
                         !w0.end(update_time_);) {
                        latest_.right_button_ = true;
                        yield return null;
                    }
                    yield return null;
                }
            }
            for (var w = new Utility.WaitForSeconds(MyRandom.Range(0.2f, 0.8f), update_time_);
                 !w.end(update_time_);) {
                yield return null;
            }
            yield return null;
        }
    }

    public void fetch(double update_time)
    {
        var prev_left_button = latest_.left_button_;
        var prev_right_button = latest_.right_button_;
        var prev_jump_button = latest_.jump_button_;
        var prev_pause_button = latest_.pause_button_;
        latest_.clear();
        latest_.fetch_pause_button();
        if (enumerator_ != null) {
            update_time_ = update_time;
            fetch_auto_update();
            enumerator_.MoveNext();
        } else {
            latest_.fetch_device();
        }
        latest_.left_button_down_ = latest_.left_button_ && !prev_left_button;
        latest_.right_button_down_ = latest_.right_button_ && !prev_right_button;
        latest_.jump_button_down_ = latest_.jump_button_ && !prev_jump_button;
        latest_.pause_button_down_ = latest_.pause_button_ && !prev_pause_button;
        latest_.left_button_up_ = !latest_.left_button_ && prev_left_button;
        latest_.right_button_up_ = !latest_.right_button_ && prev_right_button;
        latest_.jump_button_up_ = !latest_.jump_button_ && prev_jump_button;
        latest_.pause_button_up_ = !latest_.pause_button_ && prev_pause_button;
    }

    public Control getLatest()
    {
        return latest_;
    }
}

} // namespace UTJ {

/*
 * End of Controller.cs
 */

/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public struct InputBuffer
{
    public const int INPUT_MAX = 8;
    public int[] buttons_;
    public bool[] touched_;
    public Vector2[] touched_position_;
    public Vector2 flick_vector_;
}

public class InputManager
{
    // singleton
    static InputManager instance_;
    public static InputManager Instance { get { return instance_ ?? (instance_ = new InputManager()); } }

    public const int ONE = 4096;
    public const float INV_ONE = 1f/((float)ONE);

    public enum Button {
        Horizontal,
        Vertical,
        Left,
        Right,
        Jump,
    }

    public InputBuffer input_buffer_;
    private Vector2 prev_mouse_position_;

    public void init()
    {
        input_buffer_ = new InputBuffer();
        input_buffer_.buttons_ = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
        input_buffer_.touched_ = new bool[2] { false, false };
        input_buffer_.touched_position_ = new Vector2[2] { Vector2.zero, Vector2.zero, };
        input_buffer_.flick_vector_ = new Vector2(0f, 0f);
    }

    public int getButton(Button button)
    {
        return input_buffer_.buttons_[(int)button];
    }
    public bool isButton(Button button)
    {
        return input_buffer_.buttons_[(int)button] != 0;
    }
    public float getAnalog(Button button)
    {
        return (float)(input_buffer_.buttons_[(int)button]) * INV_ONE;
    }
    public bool touched(int index)
    {
        return input_buffer_.touched_[index];
    }
    public Vector2 getTouchedPosition(int index)
    {
        return input_buffer_.touched_position_[index];
    }
    public Vector2 getFlickVector()
    {
        return input_buffer_.flick_vector_;
    }

    private void set_buttons()
    {
        int[] buttons = input_buffer_.buttons_;
        buttons[(int)InputManager.Button.Horizontal] = (int)(Input.GetAxisRaw("Horizontal") * InputManager.ONE);
        buttons[(int)InputManager.Button.Vertical] = (int)(Input.GetAxisRaw("Vertical") * InputManager.ONE);
        buttons[(int)InputManager.Button.Left] = (Input.GetKey(KeyCode.Z) ? 1 : 0) | (Input.GetButton("Fire3") ? 1 : 0);
        buttons[(int)InputManager.Button.Right] = (Input.GetKey(KeyCode.X) ? 1 : 0) | (Input.GetButton("Fire2") ? 1 : 0);
        bool jump = false;
        if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow)) {
            jump = true;
        } else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) {
            jump = true;
        }
        buttons[(int)InputManager.Button.Jump] = (int)(jump ? 1 : 0);
    }
    private void set_touched(bool touched, ref Vector2 pos, int index)
    {
        input_buffer_.touched_[index] = touched;
        input_buffer_.touched_position_[index] = pos;
    }
    private void set_flick()
    {
        bool treated = false;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
            var diff = Input.GetTouch(0).deltaPosition;
            input_buffer_.flick_vector_.x = diff.x / (float)Screen.height;
            input_buffer_.flick_vector_.y = diff.y / (float)Screen.height;
            treated = true;
        }
        if (Input.GetMouseButtonDown(0)) {
            prev_mouse_position_ = Input.mousePosition;
        }
        if (Input.GetMouseButton(0)) {
            Vector2 pos = Input.mousePosition;
            Vector2 diff = pos - prev_mouse_position_;
            input_buffer_.flick_vector_.x = diff.x / (float)Screen.height;
            input_buffer_.flick_vector_.y = diff.y / (float)Screen.height;
            prev_mouse_position_ = pos;
            treated = true;
        }
        if (!treated) {
            input_buffer_.flick_vector_.x = 0f;
            input_buffer_.flick_vector_.y = 0f;
        }
    }

    public void update()
    {
        set_buttons();

        bool clicked0 = false;
        bool clicked1 = false;
        var clicked_position0 = new Vector2(0f, 0f);
        var clicked_position1 = new Vector2(0f, 0f);
        if (Input.touchCount > 0) {
            clicked_position0 = Input.GetTouch(0).position;
            clicked0 = true;
            if (Input.touchCount > 1) {
                clicked_position1 = Input.GetTouch(1).position;
                clicked1 = true;
            }
        } else if (Input.GetMouseButton(0)) {
            clicked_position0 = Input.mousePosition;
            clicked0 = true;
        }
        clicked_position0.x -= Screen.width*0.5f;
        clicked_position0.y -= Screen.height*0.5f;
        clicked_position1.x -= Screen.width*0.5f;
        clicked_position1.y -= Screen.height*0.5f;
        set_touched(clicked0, ref clicked_position0, 0 /* index */);
        set_touched(clicked1, ref clicked_position1, 1 /* index */);
        set_flick();
    }
}

} // namespace UTJ {

/*
 * End of InputManager.cs
 */

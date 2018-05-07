/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public struct DrawBuffer
{
    public enum Type
    {
        None,
        Empty,
        MuscleMotionPlayer,
        FighterAlpha,
        DragonHead,
        DragonBody,
        DragonTail,
    }

    public enum SE
    {
        None,
        Bullet,
        Explosion,
        Laser,
        Shield,
    }

    public enum BGM
    {
        Keep,
        Stop,
        Pause,
        Resume,
        Battle,
    }

    public const int OBJECT_MAX = 1024;

    public struct ObjectBuffer
    {
        public MyTransform transform_;
        public Type type_;
        public int versatile_data_;

        public void init()
        {
            transform_.init();
            type_ = Type.None;
            versatile_data_ = 0;
        }
        public void set(ref MyTransform transform, Type type, int versatile_data)
        {
            transform_ = transform;
            type_ = type;
            versatile_data_ = versatile_data;
        }
        public void set(ref Vector3 position, ref Quaternion rotation, Type type, int versatile_data)
        {
            transform_.position_ = position;
            transform_.setRotation(ref rotation);
            type_ = type;
            versatile_data_ = versatile_data;
        }
    }

    public MyTransform camera_transform_;
    public ObjectBuffer[] object_buffer_;
    public int object_num_;
    public SE[] se_;
    public BGM bgm_;
    public Motion motion_;

    private int audio_idx_;

    public void init()
    {
        object_buffer_ = new DrawBuffer.ObjectBuffer[OBJECT_MAX];
        for (int i = 0; i < OBJECT_MAX; ++i) {
            object_buffer_[i].init();
        }
        object_num_ = 0;

        se_ = new SE[SystemManager.AUDIO_CHANNEL_MAX];
        for (var i = 0; i < SystemManager.AUDIO_CHANNEL_MAX; ++i) {
            se_[i] = SE.None;
        }
        audio_idx_ = 0;
        
        bgm_ = BGM.Keep;
    }

    public void beginRender()
    {
        object_num_ = 0;
        audio_idx_ = 0;
    }

    public void endRender()
    {
    }

    public void regist(ref MyTransform transform, Type type, int versatile_data = 0)
    {
        object_buffer_[object_num_].set(ref transform, type, versatile_data);
        ++object_num_;
        if (object_num_ > OBJECT_MAX) {
            Debug.LogError("EXCEED Fighter POOL!");
            Debug.Assert(false);
        }
    }

    public void regist(ref Vector3 position, ref Quaternion rotation, Type type, int versatile_data = 0)
    {
        object_buffer_[object_num_].set(ref position, ref rotation, type, versatile_data);
        ++object_num_;
        if (object_num_ > OBJECT_MAX) {
            Debug.LogError("EXCEED Fighter POOL!");
            Debug.Assert(false);
        }
    }

    public void registCamera(ref MyTransform transform)
    {
        camera_transform_ = transform;
    }

    public void registSound(SE se)
    {
        if (audio_idx_ >= SystemManager.AUDIO_CHANNEL_MAX) {
            Debug.Log("max audio channel is used.");
            return;
        }
        se_[audio_idx_] = se;
        ++audio_idx_;
    }

    public void registBgm(BGM bgm)
    {
        bgm_ = bgm;
    }

    public void registMotion(Motion motion)
    {
        motion_ = motion;
    }
}

} // namespace UTJ {

/*
 * End of DrawBuffer.cs
 */

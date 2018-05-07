/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;

namespace UTJ {

public abstract class Task
{
    public Task prev_;
    public Task next_;
    public bool alive_;
    public abstract void update(float dt, double update_time);
    public abstract void renderUpdate(int front, CameraBase camera, ref DrawBuffer draw_buffer);

    public virtual void init()
    {
        alive_ = true;
        TaskManager.Instance.add(this);
    }

    public virtual void destroy()
    {
        TaskManager.Instance.remove(this);
        alive_ = false;
        /* don't touch next_ and prev_ here! */
    }
}

} // namespace UTJ {

/*
 * End of Task.cs
 */

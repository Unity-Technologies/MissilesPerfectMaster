/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections.Generic;

namespace UTJ {

public class TaskManager {

    // singleton
    static TaskManager instance_;
    public static TaskManager Instance { get { return instance_ ?? (instance_ = new TaskManager()); } }

    private Task first_;
    private Task last_;
    private List<Task> add_task_list_;
    private List<Task> del_task_list_;
    private int count_;

    public void init()
    {
        const int MAX_CAPACITY = 1024;
        first_ = null;
        last_ = null;
        count_ = 0;
        add_task_list_ = new List<Task>();
        add_task_list_.Capacity = MAX_CAPACITY;
        del_task_list_ = new List<Task>();
        del_task_list_.Capacity = MAX_CAPACITY;
    }

    public int getCount()
    {
        return count_;
    }

    // Taskが呼ぶ
    public void add(Task task)
    {
        add_task_list_.Add(task);
    }

    // Taskが呼ぶ
    public void remove(Task task)
    {
        del_task_list_.Add(task);
    }

    public void restart()
    {
        for (var it = first_; it != null; it = it.next_) {
            it.alive_ = false;
        }
        first_ = null;
        last_ = null;
        add_task_list_.Clear();
        del_task_list_.Clear();
        count_ = 0;
    }
    
    public void update(float dt, double update_time)
    {
        // update
        count_ = 0;
        for (var it = first_; it != null; it = it.next_) {
            it.update(dt, update_time);
            ++count_;
        }

        // add
        foreach (var it in add_task_list_) {
            if (first_ == null) {
                Debug.Assert(last_ == null);
                first_ = it;
                last_ = it;
                it.next_ = it.prev_ = null;
            } else {
                it.prev_ = last_;
                it.next_ = null;
                last_.next_ = it;
                last_ = it;
            }
        }
        add_task_list_.Clear();

        // delete
        foreach (var it in del_task_list_) {
            if (it.prev_ == null) {
                first_ = it.next_;
            } else {
                it.prev_.next_ = it.next_;
            }
            if (it.next_ == null) {
                last_ = it.prev_;
            } else {
                it.next_.prev_ = it.prev_;
            }
            it.next_ = it.prev_ = null;
        }
        del_task_list_.Clear();
    }

    public void renderUpdate(int front, CameraBase camera, ref DrawBuffer draw_buffer)
    {
        for (var it = first_; it != null; it = it.next_) {
            it.renderUpdate(front, camera, ref draw_buffer);
        }
    }
}

} // namespace UTJ {

/*
 * End of TaskManager.cs
 */

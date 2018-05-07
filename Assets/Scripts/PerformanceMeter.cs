/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */
using UnityEngine;

namespace UTJ {

public class PerformanceMeter
{
    // singleton
    static PerformanceMeter instance_;
    public static PerformanceMeter Instance { get { return instance_ ?? (instance_ = new PerformanceMeter()); } }

    private System.Diagnostics.Stopwatch stopwatch_;
    private const float FRAME_PERIOD = 1f/59.94f;
    private float fps_;
    private float display_fps_;

    private long render_start_tick_;
    private long render_tick_;

    private long behaviour_update_start_tick_;
    private long behaviour_update_tick_;

#if !UNITY_EDITOR && (UNITY_PS4 || UNITY_PSP2 || UNITY_SWITCH)
    private long console_render_start_tick_;
    private long console_render_tick_;
#endif

    private int gc_start_count_;
    private int frame_count_;
    private int frame_diff_;
    private bool recording_;

    public void init()
    {
        gc_start_count_ = System.GC.CollectionCount(0 /* generation */);
        stopwatch_ = new System.Diagnostics.Stopwatch();
        stopwatch_.Start();
        frame_count_ = 0;
        frame_diff_ = 0;
        recording_ = false;
    }

    public void setRecording() { recording_ = true; }

    public void setFrameDiff(int value)
    {
        frame_diff_ = value;
    }

    public void beginUpdate()
    {
    }
    public void endUpdate()
    {
    }

    public void beginRenderUpdate()
    {
    }
    public void endRenderUpdate()
    {
    }

    public void beginBehaviourUpdate()
    {
        behaviour_update_start_tick_ = stopwatch_.ElapsedTicks;
    }
    public void endBehaviourUpdate()
    {
        behaviour_update_tick_ = stopwatch_.ElapsedTicks - behaviour_update_start_tick_;
        ++frame_count_;
    }

    public void beginConsoleRender()
    {
#if !UNITY_EDITOR && (UNITY_PS4 || UNITY_PSP2 || UNITY_SWITCH)
        console_render_start_tick_ = stopwatch_.ElapsedTicks;
#endif
    }
    public void endConsoleRender()
    {
#if !UNITY_EDITOR && (UNITY_PS4 || UNITY_PSP2 || UNITY_SWITCH)
        console_render_tick_ = stopwatch_.ElapsedTicks - console_render_start_tick_;
#endif
    }

    public void beginRender()
    {
        long period = stopwatch_.ElapsedTicks - render_start_tick_;
        fps_ = (float)((double)System.Diagnostics.Stopwatch.Frequency / (double)period);
        display_fps_ = Mathf.Lerp(display_fps_, fps_, 0.25f);
        render_start_tick_ = stopwatch_.ElapsedTicks;
    }
    public void endRender()
    {
        render_tick_ = stopwatch_.ElapsedTicks - render_start_tick_;
    }

    public bool wasSlowLoop() {
        return !recording_ && fps_ < 50f;
    }

    public float getDisplayFPS() { return display_fps_; }
    public float getFPS() { return fps_; }

    
    public void drawMeters(int front)
    {
        if (recording_) {
            return;
        }
#pragma warning disable 162
        const int bar_x = -560;
        const int bar_y = 300;
        const int width = 560;
        const int height = 4;
        float frame_tick = FRAME_PERIOD * (float)System.Diagnostics.Stopwatch.Frequency;

        int x = bar_x;
        int y = bar_y;
        int w;
        int h = height;

        MySprite.Instance.put(x, y, width, height, MySprite.Kind.Square, MySprite.Type.Black);
        w = (int)((float)render_tick_/frame_tick * (float)width);
        MySprite.Instance.put(x, y, w, h, MySprite.Kind.Square, MySprite.Type.Red);
        w = (int)((float)behaviour_update_tick_/frame_tick * (float)width);
        MySprite.Instance.put(x, y, w, h, MySprite.Kind.Square, MySprite.Type.Yellow);
        x = bar_x;
        y -= 8;

        y += 32;
        int fps100 = (int)(display_fps_);
        MyFont.Instance.putNumber(fps100, 3 /* keta */, 0.5f /* scale */,
                                  x, y, MyFont.Type.White);
        int game_fps = SystemManager.Instance.getFPS();
        MyFont.Instance.putNumber(game_fps, 3 /* keta */, 0.5f /* scale */,
                                  x+40, y, MyFont.Type.Blue);
        int missile_draw_max = MissileManager.Instance.missileDrawMax;
        MyFont.Instance.putNumber(missile_draw_max, 4 /* keta */, 0.5f /* scale */,
                                  x+80, y, MyFont.Type.Blue);
        int gc_count = System.GC.CollectionCount(0 /* generation */) - gc_start_count_;
        MyFont.Instance.putNumber(gc_count, 8 /* keta */, 0.5f /* scale */,
                                  x+150, y, MyFont.Type.Yellow);
        MyFont.Instance.putNumber(frame_count_, 8 /* keta */, 0.5f /* scale */,
                                  x+250, y, MyFont.Type.Green);
        MyFont.Instance.putNumber(frame_diff_, 4 /* keta */, 0.5f /* scale */,
                                  x+310, y, MyFont.Type.Green);
        MyFont.Instance.putNumber(MissileManager.Instance.exist_missile_count_, 8 /* keta */, 0.5f /* scale */,
                                  x+360, y, MyFont.Type.Red);
    }
}

} // namespace UTJ {

/*
 * End of PerformanceMeter.cs
 */

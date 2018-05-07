/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Profiling;

namespace UTJ {

public class SystemManager : MonoBehaviour {

    // singleton
    static SystemManager instance_;
    public static SystemManager Instance { get { return instance_ ?? (instance_ = GameObject.Find("system_manager").GetComponent<SystemManager>()); } }

    static readonly int shader_CurrentTime = Shader.PropertyToID("_CurrentTime");

    [SerializeField] bool debug_mode_;
    [SerializeField] Material final_material_;
    [SerializeField] Material spark_material_;
    [SerializeField] Material debris_material_;
    [SerializeField] Sprite[] sprites_;
    [SerializeField] Material sprite_material_;
    [SerializeField] Font font_;
    [SerializeField] Material font_material_;
    [SerializeField] Mesh fighter_alpha_mesh_;
    [SerializeField] Material fighter_alpha_material_;
    [SerializeField] Mesh alpha_burner_mesh_;
    [SerializeField] Material alpha_burner_material_;
    Matrix4x4[] alpha_matrices_;
    const int ALPHA_MAX = 128;
    Vector4[] frustum_planes_;

    Camera camera_;
    GameObject camera_final_holder_;
    Camera camera_final_;
    RenderTexture render_texture_;
    public Matrix4x4 ProjectionMatrix { get; set; }

    bool meter_draw_;

    const int DefaultFps = 60;
    const float RENDER_FPS = 60f;
    const float RENDER_DT = 1f/RENDER_FPS;
    public System.Diagnostics.Stopwatch stopwatch_;
    int rendering_front_;
    public int getRenderingFront() { return rendering_front_; }
    DrawBuffer[] draw_buffer_;
    float dt_;
    public void setFPS(int fps)    {
        dt_ = 1f / (float)fps;
    }
    public int getFPS() {
        if (dt_ <= 0f) {
            return 0;
        } else {
            return (int)(1f/dt_ + 0.5f);
        }
    }

    DebugCamera debug_camera_;
    SpectatorCamera spectator_camera_;

    long update_frame_;
    long render_frame_;
    long render_sync_frame_;
    double update_time_;
    bool pause_;

    // audio
    public const int AUDIO_CHANNEL_MAX = 4;

    const int AUDIOSOURCE_EXPLOSION_MAX = 8;
    AudioSource[] audio_sources_explosion_;
    int audio_source_explosion_index_;
    public AudioClip se_explosion_;

    const int AUDIOSOURCE_LASER_MAX = 4;
    AudioSource[] audio_sources_laser_;
    int audio_source_laser_index_;
    public AudioClip se_laser_;

    bool spectator_mode_;
    bool initialized_ = false;

    void OnApplicationQuit()
    {
        final_material_.mainTexture = null; // suppress error-messages on console.
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneGUI;
        UnityEditor.SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
    }

    void Start()
    {
        instance_ = GameObject.Find("system_manager").GetComponent<SystemManager>();;
        initialize();
    }

    void OnDisable()
    {
        finalize();
#if UNITY_EDITOR
        UnityEditor.SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
    }

    void set_camera()
    {
        if (!spectator_mode_) {
            debug_camera_.setup(spectator_camera_);
        }
        debug_camera_.active_ = !spectator_mode_;
        spectator_camera_.active_ = spectator_mode_;
    }

    void initialize()
    {
        MyRandom.setSeed(12345L);
        camera_final_ = GameObject.Find("FinalCamera").GetComponent<Camera>();
        camera_final_.enabled = false;
        if ((float)Screen.width / (float)Screen.height < 16f/9f) {
            var size = camera_final_.orthographicSize * ((16f/9f) * ((float)Screen.height/(float)Screen.width));
            camera_final_.orthographicSize = size;
        }
        meter_draw_ = true;

        Application.targetFrameRate = (int)RENDER_FPS; // necessary for iOS because the default is 30fps.
        // QualitySettings.vSyncCount = 1;

        stopwatch_ = new System.Diagnostics.Stopwatch();
        stopwatch_.Start();
        rendering_front_ = 0;

        setFPS(DefaultFps);
        update_frame_ = 0;
        update_time_ = 100.0;    // ゼロクリア状態を過去のものにするため増やしておく
        render_frame_ = 0;
        render_sync_frame_ = 0;
        pause_ = false;
        spectator_mode_ = true;

        camera_ = GameObject.Find("Main Camera").GetComponent<Camera>();
        ProjectionMatrix = camera_.projectionMatrix;

        MissileManager.Instance.initialize(camera_);
        InputManager.Instance.init();
        Controller.Instance.init(false /* auto */);
        TaskManager.Instance.init();
        Fighter.createPool();
        Spark.Instance.init(spark_material_);
        Debris.Instance.init(debris_material_);
        MySprite.Instance.init(sprites_, sprite_material_);
        MySpriteRenderer.Instance.init(camera_);
        MyFont.Instance.init(font_, font_material_);
        MyFontRenderer.Instance.init();

        PerformanceMeter.Instance.init();

        draw_buffer_ = new DrawBuffer[2];
        for (int i = 0; i < 2; ++i) {
            draw_buffer_[i].init();
        }

        debug_camera_ = DebugCamera.create();
        spectator_camera_ = SpectatorCamera.create();
        set_camera();

        // audio
        audio_sources_explosion_ = new AudioSource[AUDIOSOURCE_EXPLOSION_MAX];
        for (var i = 0; i < AUDIOSOURCE_EXPLOSION_MAX; ++i) {
            audio_sources_explosion_[i] = gameObject.AddComponent<AudioSource>();
            audio_sources_explosion_[i].clip = se_explosion_;
            audio_sources_explosion_[i].volume = 0.01f;
            audio_sources_explosion_[i].pitch = 0.25f;
        }
        audio_source_explosion_index_ = 0;
        audio_sources_laser_ = new AudioSource[AUDIOSOURCE_LASER_MAX];
        for (var i = 0; i < AUDIOSOURCE_LASER_MAX; ++i) {
            audio_sources_laser_[i] = gameObject.AddComponent<AudioSource>();
            audio_sources_laser_[i].clip = se_laser_;
            audio_sources_laser_[i].volume = 0.025f;
        }
        audio_source_laser_index_ = 0;
    
        GameManager.Instance.init(debug_mode_);

#if UNITY_PS4 || UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        int rw = 1920;
        int rh = 1080;
#else
        int rw = 1024;
        int rh = 576;
#endif
        render_texture_ = new RenderTexture(rw, rh, 24 /* depth */, RenderTextureFormat.ARGB32);
        render_texture_.Create();
        camera_.targetTexture = render_texture_;
        final_material_.mainTexture = render_texture_;
        alpha_matrices_ = new Matrix4x4[ALPHA_MAX];
        frustum_planes_ = new Vector4[6];

        initialized_ = true;
        camera_final_.enabled = true;
    }

    void finalize()
    {
        MissileManager.Instance.finalize();
    }

    int get_front()
    {
        int updating_front = rendering_front_;            // don't flip
        return updating_front;
    }

    void main_loop()
    {
        PerformanceMeter.Instance.beginUpdate();
        int updating_front = get_front();

        // fetch
        Controller.Instance.fetch(update_time_);
        var controller = Controller.Instance.getLatest();

         // update
        float dt = dt_;
        if (pause_) {
            dt = 0f;
        }
        spectator_camera_.rotateOffsetRotation(-controller.flick_y_, controller.flick_x_);
        UnityEngine.Profiling.Profiler.BeginSample("Task update");
        GameManager.Instance.update(dt, update_time_);
        TaskManager.Instance.update(dt, update_time_);
        UnityEngine.Profiling.Profiler.EndSample();
        ++update_frame_;
        update_time_ += dt;

        UnityEngine.Profiling.Profiler.BeginSample("MissileManager.update");
        MissileManager.Instance.update(dt, update_time_);
        UnityEngine.Profiling.Profiler.EndSample();
        PerformanceMeter.Instance.endUpdate();

        PerformanceMeter.Instance.beginRenderUpdate();
        CameraBase current_camera = spectator_mode_ ? spectator_camera_ as CameraBase : debug_camera_ as CameraBase;
        // begin
        UnityEngine.Profiling.Profiler.BeginSample("renderUpdate_begin");
        MySprite.Instance.begin();
        MyFont.Instance.begin();
        Spark.Instance.begin();
        UnityEngine.Profiling.Profiler.EndSample();

        // renderUpdate
        UnityEngine.Profiling.Profiler.BeginSample("renderUpdate");
         draw_buffer_[updating_front].beginRender();
        TaskManager.Instance.renderUpdate(updating_front,
                                          current_camera,
                                          ref draw_buffer_[updating_front]);
        draw_buffer_[updating_front].endRender();
        UnityEngine.Profiling.Profiler.EndSample();
        
        // performance meter
        if (meter_draw_) {
            PerformanceMeter.Instance.drawMeters(updating_front);
        }

        // end
        UnityEngine.Profiling.Profiler.BeginSample("renderUpdate_end");
        Spark.Instance.end();
        MyFont.Instance.end();
        MySprite.Instance.end();
        UnityEngine.Profiling.Profiler.EndSample();

        PerformanceMeter.Instance.endRenderUpdate();
    }

    public float realtimeSinceStartup { get { return ((float)stopwatch_.ElapsedTicks) /  (float)System.Diagnostics.Stopwatch.Frequency; } }

    public void registSound(DrawBuffer.SE se)
    {
        int front = get_front();
        draw_buffer_[front].registSound(se);
    }

    void render(ref DrawBuffer draw_buffer)
    {
        // camera
        camera_.transform.position = draw_buffer.camera_transform_.position_;
        camera_.transform.rotation = draw_buffer.camera_transform_.rotation_;
        camera_.enabled = true;
        var vp = camera_.projectionMatrix * camera_.worldToCameraMatrix;
        Utility.GetPlanesFromFrustum(frustum_planes_, ref vp);

        int alpha_count = 0;
        for (var i = 0; i < draw_buffer.object_num_; ++i) {
            switch (draw_buffer.object_buffer_[i].type_) {
                case DrawBuffer.Type.None:
                    Debug.Assert(false);
                    break;
                case DrawBuffer.Type.Empty:
                    break;
                case DrawBuffer.Type.FighterAlpha:
                    if (Utility.InFrustum(frustum_planes_,
                                          ref draw_buffer.object_buffer_[i].transform_.position_,
                                          2f /* radius */)) {
                        draw_buffer.object_buffer_[i].transform_.getLocalToWorldMatrix(ref alpha_matrices_[alpha_count]);
                        ++alpha_count;
                    }
                    break;
            }
        }
        Graphics.DrawMeshInstanced(fighter_alpha_mesh_, 0 /* submeshIndex */,
                                   fighter_alpha_material_,
                                   alpha_matrices_, 
                                   alpha_count,
                                   null,
                                   UnityEngine.Rendering.ShadowCastingMode.Off,
                                   false /* receiveShadows */,
                                   0 /* layer */,
                                   null /* camera */);
        alpha_burner_material_.SetFloat(shader_CurrentTime, (float)update_time_);
        Graphics.DrawMeshInstanced(alpha_burner_mesh_, 0 /* submeshIndex */,
                                   alpha_burner_material_,
                                   alpha_matrices_, 
                                   alpha_count,
                                   null,
                                   UnityEngine.Rendering.ShadowCastingMode.Off,
                                   false /* receiveShadows */,
                                   0 /* layer */,
                                   null /* camera */);
        // audio
        for (var i = 0; i < AUDIO_CHANNEL_MAX; ++i) {
            if (draw_buffer.se_[i] != DrawBuffer.SE.None) {
                switch (draw_buffer.se_[i]) {
                    case DrawBuffer.SE.Explosion:
                        audio_sources_explosion_[audio_source_explosion_index_].Play();
                        ++audio_source_explosion_index_;
                        if (audio_source_explosion_index_ >= AUDIOSOURCE_EXPLOSION_MAX) {
                            audio_source_explosion_index_ = 0;
                        }
                        break;
                    case DrawBuffer.SE.Laser:
                        audio_sources_laser_[audio_source_laser_index_].Play();
                        ++audio_source_laser_index_;
                        if (audio_source_laser_index_ >= AUDIOSOURCE_LASER_MAX) {
                            audio_source_laser_index_ = 0;
                        }
                        break;
                }
                draw_buffer.se_[i] = DrawBuffer.SE.None;
            }
        }
    }

    void camera_update()
    {
    }

    void unity_update()
    {
        ProjectionMatrix = camera_.projectionMatrix;

        double render_time = update_time_;
        UnityEngine.Profiling.Profiler.BeginSample("SystemManager.render");
        render(ref draw_buffer_[rendering_front_]);
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("SystemManager.render components");
        Spark.Instance.render(camera_, render_time);
        Debris.Instance.render(camera_, render_time);

        MySprite.Instance.render();
        MyFont.Instance.render();
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void end_of_frame()
    {
        if (Time.deltaTime > 0) {
            ++render_sync_frame_;
            ++render_frame_;
            stopwatch_.Start();
        } else {
            stopwatch_.Stop();
        }
    }

    // The Update
    void Update()
    {
        PerformanceMeter.Instance.beginRender();
        if (!initialized_) {
            return;
        }
        // MissileManager.Instance.SyncComputeBuffer();

        PerformanceMeter.Instance.beginBehaviourUpdate();
        
        InputManager.Instance.update();
        UnityEngine.Profiling.Profiler.BeginSample("main_loop");
        main_loop();
        UnityEngine.Profiling.Profiler.EndSample();
        // MissileManager.Instance.SyncComputeBuffer();
        UnityEngine.Profiling.Profiler.BeginSample("unity_update");
        unity_update();
        UnityEngine.Profiling.Profiler.EndSample();
        // MissileManager.Instance.SyncComputeBuffer();
        end_of_frame();
    }

    void LateUpdate()
    {
        if (!initialized_) {
            return;
        }
        camera_update();
        PerformanceMeter.Instance.endBehaviourUpdate();
        // MissileManager.Instance.SyncComputeBuffer();
    }

    void OnRenderObject()
    {
        // MissileManager.Instance.SyncComputeBuffer();
    }


    // pause menu callbacks
    public void OnPauseClick()
    {
        pause_ = !pause_;
    }
    public void OnRestartClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("main");
    }
    public void OnFPSSliderChange(float value)
    {
        if (value <= 0f) {
            dt_ = 0f;
        } else {
            dt_ = 1f/value;
        }
    }
    public void OnMeterToggle()
    {
        meter_draw_ = !meter_draw_;
    }
    public void OnMissileMaxChange()
    {
        MissileManager.Instance.changeMissileDrawMax();
    }

#if UNITY_EDITOR
    void OnSceneGUI(UnityEditor.SceneView sceneView)
    {
        MissileManager.Instance.onSceneGUI(sceneView.camera);
    }
#endif
    
}

} // namespace UTJ {

/*
 * End of SystemManager.cs
 */

/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

#if (UNITY_2018_1_OR_NEWER && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)) || (!UNITY_EDITOR_WIN && UNITY_SWITCH)
# define ENABLE_GPUREADBACK
# define GPUREADBACK_COULD_BE_QUEUED
#endif

#if UNITY_SWITCH || UNITY_PS4
# define SYNC_COMPUTE_END_OF_FRAME
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace UTJ {

[StructLayout(LayoutKind.Sequential)]
public struct SpawnData
{
    public Vector3 position_;
    public int missile_id_;
    public Quaternion rotation_;
    public int target_id_;
    public int valid_;
    public float random_value_;
    public float random_value_second_;
}

[StructLayout(LayoutKind.Sequential)]
public struct TargetData
{
    public Vector3 position_;
    public float sqr_radius_;
    public float dead_time_;
    public float spawn_time_;
    public float dummy0_;
    public float dummy1_;
}

[StructLayout(LayoutKind.Sequential)]
public struct MissileData
{
    public Vector3 position_;
    public float spawn_time_;
    public Vector3 omega_;
    public float dead_time_;
    public Quaternion rotation_;
    public int target_id_;
    public float random_value_;
    public float dummy0_;
    public float dummy1_;
}

[StructLayout(LayoutKind.Sequential)]
public struct ResultData
{
    public byte cond_;
    public byte dist_;
    public byte target_id_;
    public byte frame_count_;
}

[StructLayout(LayoutKind.Sequential)]
public struct SortData
{
    // public float key_;
    // public int missile_id_;
    public int packed;
}

public class MissileManager : MonoBehaviour {

    // singleton
    static MissileManager instance_;
    public static MissileManager Instance { get { return instance_ ?? (instance_ = GameObject.Find("missile_manager").GetComponent<MissileManager>()); } }

    Camera camera_;
    Transform camera_transform_;
    [SerializeField] Mesh mesh_missile_;
    [SerializeField] Material material_missile_;
    [SerializeField] Mesh mesh_burner_;
    [SerializeField] Material material_burner_;

    Mesh mesh_trail_;
    [SerializeField] Material material_trail_;

    Mesh mesh_explosion_;
    [SerializeField] Material material_explosion_;

    // const float FLOAT_MAX = System.Single.MaxValue;
    const float FLOAT_MAX = 1e+38f;
    const int THREAD_MAX = 512;
    const int MISSILE_MAX = THREAD_MAX*16;
    const int SPAWN_MAX = 64;
    const int TARGET_MAX = 256;
    const int TRAIL_LENGTH = 32;
    const float MISSILE_ALIVE_PERIOD = 20f;
    const float MISSILE_ALIVE_PERIOD_AFTER_TARGET_DEATH = 2f;
    const float TRAIL_REMAIN_PERIOD_AFTER_MISSILE_DEATH = 1.2f;
    // shader ids
    static readonly int shader_CurrentTime = Shader.PropertyToID("_CurrentTime");
    static readonly int shader_DT = Shader.PropertyToID("_DT");
    static readonly int shader_CamUp = Shader.PropertyToID("_CamUp");
    static readonly int shader_MatrixView2 = Shader.PropertyToID("_MatrixView2");
    static readonly int shader_CamPos = Shader.PropertyToID("_CamPos");
    static readonly int shader_FrameCount = Shader.PropertyToID("_FrameCount");
    static readonly int shader_DisplayNum = Shader.PropertyToID("_DisplayNum");

    // GPU data
    uint[] missile_drawindirect_args_;
    uint[] burner_drawindirect_args_;
    uint[] trail_drawindirect_args_;
    uint[] explosion_drawindirect_args_;
    MissileData[] missile_data_;
    SpawnData[] spawn_data_;
    TargetData[] target_data_;
    ResultData[] missile_result_list_;
    SortData[] missile_sort_key_list_;
    Vector4[] frustum_planes_;
    Vector4[] trail_data_;
    int[] trail_index_list_;
    int frame_count_ = -1;

    // CPU data
    float[] missile_status_list_;
    byte[] target_hit_list_;
    int spawn_index_;

    // compute shaders
    [SerializeField] ComputeShader cshader_spawn_;
    int ckernel_spawn_;
    [SerializeField] ComputeShader cshader_update_;
    int ckernel_update_;
    [SerializeField] ComputeShader cshader_sort_;
    int ckernel_sort_;

    // compute buffers
    ComputeBuffer cbuffer_missile_drawindirect_args_;
    ComputeBuffer cbuffer_missile_;
    ComputeBuffer cbuffer_burner_drawindirect_args_;
    ComputeBuffer cbuffer_spawn_;
    ComputeBuffer cbuffer_target_;
    ComputeBuffer cbuffer_missile_result_;
    ComputeBuffer cbuffer_missile_sort_key_list_;
    ComputeBuffer cbuffer_frustum_planes_;
    ComputeBuffer cbuffer_trail_drawindirect_args_;
    ComputeBuffer cbuffer_trail_;
    ComputeBuffer cbuffer_trail_index_;
    ComputeBuffer cbuffer_explosion_drawindirect_args_;

    // misc
    Bounds unlimited_bounds_;
    int next_spawn_missile_idx_;
    float drawn_update_time_;
    int drawn_missile_alive_count_;
    int missile_draw_max_;

    // debug information
    public int exist_missile_count_;

#if ENABLE_GPUREADBACK
# if GPUREADBACK_COULD_BE_QUEUED
    const int MAX_RQUESTS = 4;
    UnityEngine.Experimental.Rendering.AsyncGPUReadbackRequest[] requests_;
    int request_index_;
# else
    UnityEngine.Experimental.Rendering.AsyncGPUReadbackRequest requests_;
# endif
#endif

    public void initialize(Camera camera)
    {
        Debug.Assert(SystemInfo.supportsInstancing);
#if ENABLE_GPUREADBACK
        Debug.Assert(SystemInfo.supportsAsyncGPUReadback);
#endif
        camera_ = camera;
        camera_transform_ = camera_.transform;

        // mesh trail
        {
            var vertices = new Vector3[TRAIL_LENGTH*2];
            var triangles = new int[(TRAIL_LENGTH-1)*6];
            for (var i = 0; i < TRAIL_LENGTH-1; ++i) {
                triangles[i*6+0] = (i+0)*2+0;
                triangles[i*6+1] = (i+0)*2+1;
                triangles[i*6+2] = (i+1)*2+0;
                triangles[i*6+3] = (i+1)*2+0;
                triangles[i*6+4] = (i+0)*2+1;
                triangles[i*6+5] = (i+1)*2+1;
            }
            mesh_trail_ = new Mesh();
            mesh_trail_.name = "trail";
            mesh_trail_.vertices = vertices;
            mesh_trail_.triangles = triangles;
        }
        // mesh explosion
        {
            var vertices = new Vector3[4];
            var triangles = new int[6];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            triangles[3] = 2;
            triangles[4] = 1;
            triangles[5] = 3;
            mesh_explosion_ = new Mesh();
            mesh_explosion_.name = "explosion";
            mesh_explosion_.vertices = vertices;
            mesh_explosion_.triangles = triangles;
        }

        missile_drawindirect_args_ = new uint[5] { 0, 0, 0, 0, 0 };
        burner_drawindirect_args_ = new uint[5] { 0, 0, 0, 0, 0 };
        missile_data_ = new MissileData[MISSILE_MAX];
        for (var i = 0; i < missile_data_.Length; ++i) {
            missile_data_[i].position_ = CV.Vector3Zero;
            missile_data_[i].spawn_time_ = FLOAT_MAX;
            missile_data_[i].omega_ = CV.Vector3Zero;
            missile_data_[i].rotation_ = CV.QuaternionIdentity;
            missile_data_[i].target_id_ = -1;
            missile_data_[i].dead_time_ = FLOAT_MAX;
        }
        spawn_data_ = new SpawnData[SPAWN_MAX];
        for (var i = 0; i < spawn_data_.Length; ++i) {
            spawn_data_[i].missile_id_ = -1;
            spawn_data_[i].target_id_ = -1;
            spawn_data_[i].valid_ = 0;
        }
        target_data_ = new TargetData[TARGET_MAX];
        for (var i = 0; i < target_data_.Length; ++i) {
            target_data_[i].dead_time_ = FLOAT_MAX;
            target_data_[i].spawn_time_ = FLOAT_MAX;
        }

        missile_result_list_ = new ResultData[MISSILE_MAX*2];
        for (var i = 0; i < missile_result_list_.Length; ++i) {
            missile_result_list_[i].cond_ = 0;
            missile_result_list_[i].dist_ = 0;
            missile_result_list_[i].target_id_ = 0;
            missile_result_list_[i].frame_count_ = 0;
        }

        missile_sort_key_list_ = new SortData[MISSILE_MAX];
        missile_status_list_ = new float[MISSILE_MAX];
        for (var i = 0; i < missile_status_list_.Length; ++i) {
            missile_status_list_[i] = 0f;
        }
        frustum_planes_ = new Vector4[6];
        for (var i = 0; i < frustum_planes_.Length; ++i) {
            frustum_planes_[i] = new Vector4(0,0,0,0);
        }

        target_hit_list_ = new byte[TARGET_MAX];
        for (var i = 0; i < target_hit_list_.Length; ++i) {
            target_hit_list_[i] = 0;
        }
        spawn_index_ = 0;

        trail_drawindirect_args_ = new uint[5] { 0, 0, 0, 0, 0 };
        trail_data_ = new Vector4[TRAIL_LENGTH*MISSILE_MAX];
        for (var i = 0; i < trail_data_.Length; ++i) {
            trail_data_[i].x = 0f;
            trail_data_[i].y = 0f;
            trail_data_[i].z = 0f;
            trail_data_[i].w = 0f;
        }
        trail_index_list_ = new int[MISSILE_MAX];
        for (var i = 0; i < trail_index_list_.Length; ++i) {
            trail_index_list_[i] = 0;
        }

        explosion_drawindirect_args_ = new uint[5] { 0, 0, 0, 0, 0 };
        
        /* compute buffers */
        // missile
        cbuffer_missile_drawindirect_args_ = new ComputeBuffer(1 /* count */,
                                                               (missile_drawindirect_args_.Length *
                                                                Marshal.SizeOf(typeof(uint))) /* stride */,
                                                               ComputeBufferType.IndirectArguments);
        missile_drawindirect_args_[0] = mesh_missile_.GetIndexCount(0 /* submesh */);
        cbuffer_missile_drawindirect_args_.SetData(missile_drawindirect_args_);
        cbuffer_burner_drawindirect_args_ = new ComputeBuffer(1 /* count */,
                                                              (burner_drawindirect_args_.Length *
                                                               Marshal.SizeOf(typeof(uint))) /* stride */,
                                                              ComputeBufferType.IndirectArguments);
        burner_drawindirect_args_[0] = mesh_burner_.GetIndexCount(0 /* submesh */);
        cbuffer_burner_drawindirect_args_.SetData(burner_drawindirect_args_);
        cbuffer_missile_ = new ComputeBuffer(missile_data_.Length, Marshal.SizeOf(typeof(MissileData)));
        cbuffer_missile_.SetData(missile_data_);
        cbuffer_spawn_ = new ComputeBuffer(spawn_data_.Length, Marshal.SizeOf(typeof(SpawnData)));
        cbuffer_target_ = new ComputeBuffer(target_data_.Length, Marshal.SizeOf(typeof(TargetData)));
        cbuffer_target_.SetData(target_data_);
        cbuffer_missile_result_ = new ComputeBuffer(missile_result_list_.Length, Marshal.SizeOf(typeof(ResultData)));
        cbuffer_missile_result_.SetData(missile_result_list_);
        cbuffer_missile_sort_key_list_ = new ComputeBuffer(missile_sort_key_list_.Length, Marshal.SizeOf(typeof(SortData)));
        cbuffer_missile_sort_key_list_.SetData(missile_sort_key_list_);
        cbuffer_frustum_planes_ = new ComputeBuffer(frustum_planes_.Length, Marshal.SizeOf(typeof(Vector4)));
        cbuffer_frustum_planes_.SetData(frustum_planes_);

        // trail
        cbuffer_trail_drawindirect_args_ = new ComputeBuffer(1 /* count */,
                                                             (trail_drawindirect_args_.Length *
                                                              Marshal.SizeOf(typeof(uint))) /* stride */,
                                                             ComputeBufferType.IndirectArguments);
        trail_drawindirect_args_[0] = mesh_trail_.GetIndexCount(0 /* submesh */);
        cbuffer_trail_drawindirect_args_.SetData(trail_drawindirect_args_);
        cbuffer_trail_ = new ComputeBuffer(trail_data_.Length, Marshal.SizeOf(typeof(Vector4)));
        cbuffer_trail_.SetData(trail_data_);
        cbuffer_trail_index_ = new ComputeBuffer(trail_index_list_.Length, Marshal.SizeOf(typeof(int)));
        cbuffer_trail_index_.SetData(trail_index_list_);

        // explosion
        cbuffer_explosion_drawindirect_args_ = new ComputeBuffer(1 /* count */,
                                                                 (explosion_drawindirect_args_.Length *
                                                                  Marshal.SizeOf(typeof(uint))) /* stride */,
                                                                 ComputeBufferType.IndirectArguments);
        explosion_drawindirect_args_[0] = mesh_explosion_.GetIndexCount(0 /* submesh */);
        cbuffer_explosion_drawindirect_args_.SetData(explosion_drawindirect_args_);

#if ENABLE_GPUREADBACK
# if GPUREADBACK_COULD_BE_QUEUED
        requests_ = new UnityEngine.Experimental.Rendering.AsyncGPUReadbackRequest[MAX_RQUESTS];
        request_index_ = 0;
        requests_[request_index_] = UnityEngine.Experimental.Rendering.AsyncGPUReadback.Request(cbuffer_missile_result_);
        ++request_index_;
        request_index_ %= MAX_RQUESTS;
# else
        requests_ = UnityEngine.Experimental.Rendering.AsyncGPUReadback.Request(cbuffer_missile_result_);
# endif
#endif

        // setup for missile_spawn compute
        ckernel_spawn_ = cshader_spawn_.FindKernel("missile_spawn");
        cshader_spawn_.SetBuffer(ckernel_spawn_, "cbuffer_spawn", cbuffer_spawn_);
        cshader_spawn_.SetBuffer(ckernel_spawn_, "cbuffer_missile", cbuffer_missile_);
        cshader_spawn_.SetBuffer(ckernel_spawn_, "cbuffer_trail", cbuffer_trail_);
        cshader_spawn_.SetBuffer(ckernel_spawn_, "cbuffer_trail_index", cbuffer_trail_index_);
        // setup for missile_update compute
        ckernel_update_ = cshader_update_.FindKernel("missile_update");
        cshader_update_.SetBuffer(ckernel_update_, "cbuffer_missile", cbuffer_missile_);
        cshader_update_.SetBuffer(ckernel_update_, "cbuffer_target", cbuffer_target_);
        cshader_update_.SetBuffer(ckernel_update_, "cbuffer_missile_result", cbuffer_missile_result_);
        cshader_update_.SetFloat("_MissileAlivePeriod", MISSILE_ALIVE_PERIOD);
        cshader_update_.SetFloat("_MissileAlivePeriodAfterTargetDeath", MISSILE_ALIVE_PERIOD_AFTER_TARGET_DEATH);
        cshader_update_.SetFloat("_TrailRemainPeriodAfterMissileDeath", TRAIL_REMAIN_PERIOD_AFTER_MISSILE_DEATH);
        cshader_update_.SetBuffer(ckernel_update_, "cbuffer_trail", cbuffer_trail_);
        cshader_update_.SetBuffer(ckernel_update_, "cbuffer_trail_index", cbuffer_trail_index_);
        cshader_update_.SetBuffer(ckernel_update_, "cbuffer_frustum_planes", cbuffer_frustum_planes_);
        cshader_update_.SetBuffer(ckernel_update_, "cbuffer_missile_sort_key_list", cbuffer_missile_sort_key_list_);
        // setup for missile_update compute
        ckernel_sort_ = cshader_sort_.FindKernel("missile_sort");
        cshader_sort_.SetBuffer(ckernel_sort_, "cbuffer_missile_sort_key_list", cbuffer_missile_sort_key_list_);

        // setup for missile shader
        material_missile_.SetBuffer("cbuffer_missile", cbuffer_missile_);
        material_missile_.SetBuffer("cbuffer_missile_sort_key_list", cbuffer_missile_sort_key_list_);
        // setup for burner shader
        material_burner_.SetBuffer("cbuffer_missile", cbuffer_missile_);
        material_burner_.SetBuffer("cbuffer_missile_sort_key_list", cbuffer_missile_sort_key_list_);
        // setup for trail shader
        material_trail_.SetBuffer("cbuffer_trail", cbuffer_trail_);
        material_trail_.SetBuffer("cbuffer_trail_index", cbuffer_trail_index_);
        material_trail_.SetBuffer("cbuffer_missile_sort_key_list", cbuffer_missile_sort_key_list_);
        // setup for explosion shader
        material_explosion_.SetBuffer("cbuffer_missile", cbuffer_missile_);
        material_explosion_.SetBuffer("cbuffer_missile_sort_key_list", cbuffer_missile_sort_key_list_);

        unlimited_bounds_ = new Bounds(Vector3.zero, new Vector3(999999f, 999999f, 999999f));
        next_spawn_missile_idx_ = 0;
        drawn_update_time_ = 0f;
        drawn_missile_alive_count_ = 0;
#if UNITY_STANDALONE_WIN
        missile_draw_max_ = 4096;
#else
        missile_draw_max_ = 1024;
#endif

#if SYNC_COMPUTE_END_OF_FRAME
        StartCoroutine(frame_loop());
#endif
    }

    IEnumerator frame_loop()
    {
        for (;;) {
            yield return new WaitForEndOfFrame();
#if SYNC_COMPUTE_END_OF_FRAME
            sync_compute_buffer();
#endif
        }
    }
    private void sync_compute_buffer()
    {
#if !ENABLE_GPUREADBACK
        if ((frame_count_ % 2) == 0) {
            UnityEngine.Profiling.Profiler.BeginSample("<GetData>");
            cbuffer_missile_result_.GetData(missile_result_list_);
            UnityEngine.Profiling.Profiler.EndSample();
        }
#endif
    }
    public void SyncComputeBuffer()
    {
#if !SYNC_COMPUTE_END_OF_FRAME
        sync_compute_buffer();
#endif
    }

    public void finalize()
    {
        // release compute buffers
        cbuffer_explosion_drawindirect_args_.Release();
        cbuffer_trail_index_.Release();
        cbuffer_trail_.Release();
        cbuffer_trail_drawindirect_args_.Release();
        cbuffer_frustum_planes_.Release();
        cbuffer_missile_sort_key_list_.Release();
        cbuffer_missile_result_.Release();
        cbuffer_target_.Release();
        cbuffer_spawn_.Release();
        cbuffer_missile_.Release();
        cbuffer_burner_drawindirect_args_.Release();
        cbuffer_missile_drawindirect_args_.Release();
    }
    
    private void dispatch_compute(float dt, float current_time)
    {
        // set data for compute
        UnityEngine.Profiling.Profiler.BeginSample("<SetData>spawn_data_");
        cbuffer_spawn_.SetData(spawn_data_);
        UnityEngine.Profiling.Profiler.EndSample();
        cshader_spawn_.SetFloat(shader_CurrentTime, current_time);
        UnityEngine.Profiling.Profiler.BeginSample("<SetData>target_data_");
        cbuffer_target_.SetData(target_data_);
        UnityEngine.Profiling.Profiler.EndSample();
        cshader_update_.SetFloat(shader_DT, dt);
        cshader_update_.SetFloat(shader_CurrentTime, current_time);
        var view = camera_.worldToCameraMatrix;
        cshader_update_.SetVector(shader_MatrixView2, new Vector4(view.m20, view.m21, view.m22, view.m23));
        cshader_update_.SetVector(shader_CamPos, camera_transform_.position);
        cshader_update_.SetInt(shader_FrameCount, frame_count_);
        {
            var proj = camera_.projectionMatrix;
            var matrix_vp = proj * view;
            Utility.GetPlanesFromFrustum(frustum_planes_, ref matrix_vp);
            UnityEngine.Profiling.Profiler.BeginSample("<SetData>frustum_planes_");
            cbuffer_frustum_planes_.SetData(frustum_planes_);
            UnityEngine.Profiling.Profiler.EndSample();
        }
        if (spawn_index_ > 0) {
            cshader_spawn_.Dispatch(ckernel_spawn_, 1, 1, 1);
        }
        cshader_update_.Dispatch(ckernel_update_, MISSILE_MAX/THREAD_MAX, 1, 1);
        cshader_sort_.Dispatch(ckernel_sort_, 1, 1, 1);
    }

    private void draw(Camera camera, float current_time, int missile_alive_count)
    {
        Debug.Assert(missile_alive_count >= 0);
        exist_missile_count_ = missile_alive_count;
        missile_alive_count = missile_draw_max_ < missile_alive_count ? missile_draw_max_ : missile_alive_count;

        // set data for missile
        missile_drawindirect_args_[1] = (uint)missile_alive_count;
        cbuffer_missile_drawindirect_args_.SetData(missile_drawindirect_args_);
        material_missile_.SetFloat(shader_CurrentTime, current_time);
        // set data for burner
        burner_drawindirect_args_[1] = (uint)missile_alive_count;
        cbuffer_burner_drawindirect_args_.SetData(burner_drawindirect_args_);
        material_burner_.SetFloat(shader_CurrentTime, current_time);
        // set data for trail
        material_trail_.SetFloat(shader_CurrentTime, current_time);
        material_trail_.SetInt(shader_DisplayNum, missile_alive_count);
        trail_drawindirect_args_[1] = (uint)missile_alive_count;
        cbuffer_trail_drawindirect_args_.SetData(trail_drawindirect_args_);
        // set data for explosion
        material_explosion_.SetFloat(shader_CurrentTime, current_time);
        material_explosion_.SetVector(shader_CamUp, camera_transform_.TransformVector(CV.Vector3Up));
        explosion_drawindirect_args_[1] = (uint)missile_alive_count;
        cbuffer_explosion_drawindirect_args_.SetData(explosion_drawindirect_args_);

        if (missile_alive_count > 0) {
            Graphics.DrawMeshInstancedIndirect(mesh_missile_,
                                               0 /* submesh */,
                                               material_missile_,
                                               unlimited_bounds_,
                                               cbuffer_missile_drawindirect_args_,
                                               0 /* argsOffset */,
                                               null /* properties */,
                                               UnityEngine.Rendering.ShadowCastingMode.Off,
                                               false /* receiveShadows */,
                                               0 /* layer */,
                                               camera);
            Graphics.DrawMeshInstancedIndirect(mesh_burner_,
                                               0 /* submesh */,
                                               material_burner_,
                                               unlimited_bounds_,
                                               cbuffer_burner_drawindirect_args_,
                                               0 /* argsOffset */,
                                               null /* properties */,
                                               UnityEngine.Rendering.ShadowCastingMode.Off,
                                               false /* receiveShadows */,
                                               0 /* layer */,
                                               camera);
            Graphics.DrawMeshInstancedIndirect(mesh_trail_,
                                               0 /* submesh */,
                                               material_trail_,
                                               unlimited_bounds_,
                                               cbuffer_trail_drawindirect_args_,
                                               0 /* argsOffset */,
                                               null /* properties */,
                                               UnityEngine.Rendering.ShadowCastingMode.Off,
                                               false /* receiveShadows */,
                                               0 /* layer */,
                                               camera);
            Graphics.DrawMeshInstancedIndirect(mesh_explosion_,
                                               0 /* submesh */,
                                               material_explosion_,
                                               unlimited_bounds_,
                                               cbuffer_explosion_drawindirect_args_,
                                               0 /* argsOffset */,
                                               null /* properties */,
                                               UnityEngine.Rendering.ShadowCastingMode.Off,
                                               false /* receiveShadows */,
                                               0 /* layer */,
                                               camera);
        }
    }

    private int check_missile_result(float dt)
    {
        UnityEngine.Profiling.Profiler.BeginSample("<check>missile_result_list_");
        int max_vol = 0;
        for (var i = 0; i < MISSILE_MAX; ++i) {
            int cond0 = missile_result_list_[i].cond_; // even frame
            int cond1 = missile_result_list_[i+MISSILE_MAX].cond_; // odd frame
            if (cond0 != 0 || cond1 != 0) { // ミサイル消滅信号
                if (missile_status_list_[i] > 0f &&
                    missile_status_list_[i] <= dt) { // ミサイル消滅の瞬間
                    missile_status_list_[i] = -TRAIL_REMAIN_PERIOD_AFTER_MISSILE_DEATH; // 煙のぶん残す
                    if (missile_result_list_[i].dist_ > max_vol) {
                        max_vol = missile_result_list_[i].dist_;
                    }
                    if ((cond0 & 2) != 0 ||
                        (cond1 & 2) != 0) {            // 命中
                        target_hit_list_[missile_result_list_[i].target_id_] = 1; // ヒット通知
                    }
                }
            }
        }
        PerformanceMeter.Instance.setFrameDiff((frame_count_&0xff) - missile_result_list_[0].frame_count_);
        UnityEngine.Profiling.Profiler.EndSample();
        return max_vol;
    }

    private int update_status_list(float dt)
    {
        UnityEngine.Profiling.Profiler.BeginSample("<set>alive_list");
        int missile_alive_count = 0;
        for (var i = 0; i < missile_status_list_.Length; ++i) {
            float missile_status = missile_status_list_[i];
            if (missile_status > 0f) {
                if (missile_status > dt) { // must live for a few frames.
                    missile_status_list_[i] -= dt;      // countdown.
                }
            } else if (missile_status < 0f) {
                missile_status_list_[i] += dt;
                if (missile_status_list_[i] > 0f) {
                    missile_status_list_[i] = 0f;
                }
            }
            if (missile_status != 0f) {
                ++missile_alive_count;
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
        return missile_alive_count;
    }

    public void update(float dt, double update_time)
    {
        ++frame_count_;

        setup_target((float)update_time);

#if ENABLE_GPUREADBACK
# if GPUREADBACK_COULD_BE_QUEUED
        for (var k = 0; k < MAX_RQUESTS; ++k) {
            if (requests_[k].done && !requests_[k].hasError) {
                Unity.Collections.NativeArray<ResultData> buffer = requests_[k].GetData<ResultData>();
                for (var i = 0; i < missile_result_list_.Length; ++i) {
                    missile_result_list_[i] = buffer[i];
                }
                request_index_ = k;
                break;
            }
        }
        for (var k = 0; k < MAX_RQUESTS; ++k) {
            if (requests_[request_index_].done) {
                requests_[request_index_] = UnityEngine.Experimental.Rendering.AsyncGPUReadback.Request(cbuffer_missile_result_);
                break;
            }
            ++request_index_;
            request_index_ %= MAX_RQUESTS;
            Debug.Assert(k < MAX_RQUESTS);
        }
# else
        if (requests_.done) {
            if (!requests_.hasError) {
                Unity.Collections.NativeArray<ResultData> buffer = requests_.GetData<ResultData>();
                for (var i = 0; i < missile_result_list_.Length; ++i) {
                    missile_result_list_[i] = buffer[i];
                }
            } else {
                Debug.Log("hasError");
            }
            requests_ = UnityEngine.Experimental.Rendering.AsyncGPUReadback.Request(cbuffer_missile_result_);
        }
# endif
#endif

        // check dead missiles with ComputeBuffer
        if (frame_count_ % 2 != 0) {
            int max_vol = check_missile_result(dt);
            if (max_vol > 240) {
                SystemManager.Instance.registSound(DrawBuffer.SE.Explosion); // 爆発音
            }
        }

        // collect active missiles and get the count.
        int missile_alive_count = update_status_list(dt);

        // gpu execution
        dispatch_compute(dt, (float)update_time);
        
        // draw
        draw(camera_, (float)update_time, missile_alive_count);
        drawn_update_time_ = (float)update_time;
        drawn_missile_alive_count_ = missile_alive_count;

        // cleanup
        spawn_index_ = 0;
        for (var i = 0; i < spawn_data_.Length; ++i) {
            spawn_data_[i].valid_ = 0;
        }
    }

    public void onSceneGUI(Camera camera)
    {
        draw(camera, drawn_update_time_, drawn_missile_alive_count_);
    }    

    public bool checkHitAndClear(int target_id)
    {
        int hit = target_hit_list_[target_id];
        target_hit_list_[target_id] = 0; // チェックしたらクリアしてしまう
        return hit != 0;
    }
    
    void set_spawn(ref SpawnData spawn, ref Vector3 pos, ref Quaternion rot, int missile_id, int target_id)
    {
        Debug.Assert(target_id >= 0);
        spawn.position_ = pos;
        spawn.rotation_ = rot * Quaternion.Euler(MyRandom.Range(-3f, 3f), MyRandom.Range(-10, 10f), 0f);
        spawn.missile_id_ = missile_id;
        spawn.target_id_ = target_id;
        spawn.valid_ = 1 /* true */;
        spawn.random_value_ = MyRandom.Range(0f, 1f);
        spawn.random_value_second_ = MyRandom.Range(0f, 1f);
    }

    public void spawn(ref Vector3 pos, ref Quaternion rot, int target_id, double update_time)
    {
        if (((float)update_time - target_data_[target_id].dead_time_) >= 0f) {
            Debug.Log("no target exists for spawn.");
            return;                // no target exists.
        }

        Debug.Assert(missile_status_list_.Length == MISSILE_MAX);
        int idx = -1;
        for (var i = 0; i < MISSILE_MAX; ++i) {
            var j = (i + next_spawn_missile_idx_) % MISSILE_MAX;
            if (missile_status_list_[j] == 0f) {
                missile_status_list_[j] = 0.1f; // live 6 frames at least.
                idx = j;
                break;
            }
        }
        if (idx < 0) {
            /* Debug.LogError("exceed missiles.."); */
            return;
        }
        if (spawn_index_ >= spawn_data_.Length) {
            Debug.LogError("exceed spawn..");
            return;
        }
        set_spawn(ref spawn_data_[spawn_index_], ref pos, ref rot, idx, target_id);
        ++spawn_index_;
        next_spawn_missile_idx_ = idx + 1;
    }


    public int registTarget(double update_time)
    {
        var id = -1;
        for (var i = 0; i < target_data_.Length; ++i) {
            if (update_time - target_data_[i].spawn_time_ < 0) {
                id = i;
                target_data_[i].position_ = Vector3.zero;
                target_data_[i].sqr_radius_ = 0.16f; // 0.4*0.4
                target_data_[i].dead_time_ = FLOAT_MAX;
                target_data_[i].spawn_time_ = (float)update_time;
                break;
            }
        }
        Debug.Assert(id >= 0);
        return id;
    }
                
    public void setTargetRadius(int target_id, float radius)
    {
        target_data_[target_id].sqr_radius_ = radius * radius;
    }

    public void updateTarget(int target_id, ref Vector3 pos)
    {
        target_data_[target_id].position_ = pos;
    }

    public void killTarget(int target_id, double update_time)
    {
        target_data_[target_id].dead_time_ = (float)update_time;
    }

    public int missileDrawMax { get { return missile_draw_max_; } }
    public void changeMissileDrawMax()
    {
        missile_draw_max_ += MISSILE_MAX/16;
        if (missile_draw_max_ > MISSILE_MAX) {
            missile_draw_max_ = MISSILE_MAX/16;
        }
    }

    private void setup_target(float update_time)
    {
        for (var i = 0; i < target_data_.Length; ++i) {
            if (update_time - target_data_[i].spawn_time_ > 0) {
                if ((update_time - target_data_[i].dead_time_) > (MISSILE_ALIVE_PERIOD_AFTER_TARGET_DEATH + 
                                                                   TRAIL_REMAIN_PERIOD_AFTER_MISSILE_DEATH)) {
                    // can be reused
                    target_data_[i].dead_time_ = FLOAT_MAX;
                    target_data_[i].spawn_time_ = FLOAT_MAX;
                }
            }
        }
    }

}

} // namespace UTJ {

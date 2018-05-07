using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

// #pragma warning disable 0414

public class ComputeBillboardRenderer : MonoBehaviour {

    public Material material_;
    private Mesh mesh_;
    const int BILLBOARD_MAX = 4096; // be careful about the minimum for the sorting algorithm.
    const int BILLBOARD_NUM = 1024;

    struct SortElement {
        public uint id;
        public float key;
    }
    struct Billboard {
        public Vector4 world_position;
        public Vector4 color;
    }

    public ComputeShader compute_shader_sort_;
    private ComputeBuffer compute_buffer_elements_;
    private ComputeBuffer compute_buffer_billboards_;
    private int kernel_sort_;
    // private uint kernel_sort_thread_x_;
    // private uint kernel_sort_thread_y_;
    // private uint kernel_sort_thread_z_;
    private int kernel_billboard_;
    private uint kernel_billboard_thread_x_;
    private uint kernel_billboard_thread_y_;
    private uint kernel_billboard_thread_z_;
    private Billboard[] billboards_;
    private SortElement[] elements_;

    void Start()
    {
        kernel_sort_ = compute_shader_sort_.FindKernel("sort");
        // compute_shader_sort_.GetKernelThreadGroupSizes(kernel_sort_,
        //                                                out kernel_sort_thread_x_,
        //                                                out kernel_sort_thread_y_,
        //                                                out kernel_sort_thread_z_);

        // setup sort elements
        compute_buffer_elements_ = new ComputeBuffer(BILLBOARD_MAX, Marshal.SizeOf(typeof(SortElement)));
        // setup positions buffer for sorting
        compute_shader_sort_.SetBuffer(kernel_sort_, "buffer_elements", compute_buffer_elements_);

        // setup billboard buffer
        compute_buffer_billboards_ = new ComputeBuffer(BILLBOARD_MAX, Marshal.SizeOf(typeof(Billboard)));

        // create triangles
        // const int MAX = UNIT_MAX <= 65000/4 ? UNIT_MAX : 65000/4;
        var vertices = new Vector3[BILLBOARD_NUM*4];
        int[] triangles = new int[BILLBOARD_NUM*6];
        for (var i = 0; i < BILLBOARD_NUM; ++i) {
            triangles[i*6+0] = i*4+0;
            triangles[i*6+1] = i*4+1;
            triangles[i*6+2] = i*4+2;
            triangles[i*6+3] = i*4+2;
            triangles[i*6+4] = i*4+1;
            triangles[i*6+5] = i*4+3;
        }
        var uvs = new Vector2[BILLBOARD_NUM*4];
        for (var i = 0; i < BILLBOARD_NUM; ++i) {
            uvs[i*4+0] = new Vector2(0f, 0f);
            uvs[i*4+1] = new Vector2(1f, 0f);
            uvs[i*4+2] = new Vector2(0f, 1f);
            uvs[i*4+3] = new Vector2(1f, 1f);
        }
        var mesh = new Mesh();
        mesh.name = "billbaord";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);
        mesh_ = mesh;

        //
        billboards_ = new Billboard[BILLBOARD_MAX];
        for (var i = 0; i < BILLBOARD_MAX; ++i) {
            billboards_[i].world_position = Random.insideUnitSphere*4f;
            billboards_[i].world_position.w = 1f;
            var col = Color.HSVToRGB(Random.Range(0f, 1f), 1f, 1f);
            billboards_[i].color = new Vector4(col.r, col.g, col.b, col.a);

        }
        elements_ = new SortElement[BILLBOARD_MAX];
    }

    void OnDestroy()
    {
        compute_buffer_billboards_.Release();
        compute_buffer_elements_.Release();
    }

    void OnRenderObject()
    {
        var camera = Camera.current;
        var view_matrix = camera.worldToCameraMatrix;
        for (uint i = 0; i < BILLBOARD_MAX; ++i) {
            var vpos = view_matrix * billboards_[i].world_position;
            elements_[i].id = i;
            elements_[i].key = vpos.z;
        }
        compute_buffer_elements_.SetData(elements_);
        // compute_shader_sort_.Dispatch(kernel_sort_, (int)(BILLBOARD_MAX/(kernel_sort_thread_x_ *
        //                                                                  kernel_sort_thread_y_ *
        //                                                                  kernel_sort_thread_z_)), 1, 1);
        compute_shader_sort_.Dispatch(kernel_sort_, 1, 1, 1);
        compute_buffer_billboards_.SetData(billboards_);
        material_.SetBuffer("buffer_elements", compute_buffer_elements_);
        material_.SetBuffer("buffer_billboards", compute_buffer_billboards_);
        material_.SetPass(0);
        Graphics.DrawMeshNow(mesh_, Vector3.zero, Quaternion.identity);
    }
}

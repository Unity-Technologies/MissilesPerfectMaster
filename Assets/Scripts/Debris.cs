/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

public class Debris
{
    // singleton
    static Debris instance_;
    public static Debris Instance { get { return instance_ ?? (instance_ = new Debris()); } }

    const int POINT_MAX = 512;
    private float range_;
    private float rangeR_;
    private Matrix4x4 prev_view_matrix_;
    private int delay_start_count_ = 2;
    private Mesh mesh_;
    private Material material_;
    static readonly int material_TargetPosition = Shader.PropertyToID("_TargetPosition");
    static readonly int material_PrevInvMatrix = Shader.PropertyToID("_PrevInvMatrix");
    static readonly int material_Color = Shader.PropertyToID("_Color");

    public Mesh getMesh() { return mesh_; }
    public Material getMaterial() { return material_; }

    public void init(Material material)
    {
        range_ = 16f;
        rangeR_ = 1f/range_;
        var vertices = new Vector3[POINT_MAX*2];
        for (var i = 0; i < POINT_MAX; ++i) {
            float x = Random.Range(-range_, range_);
            float y = Random.Range(-range_, range_);
            float z = Random.Range(-range_, range_);
            var point = new Vector3(x, y, z);
            vertices[i*2+0] = point;
            vertices[i*2+1] = point;
        }
        var indices = new int[POINT_MAX*2];
        for (var i = 0; i < POINT_MAX*2; ++i) {
            indices[i] = i;
        }
        var colors = new Color[POINT_MAX*2];
        for (var i = 0; i < POINT_MAX; ++i) {
            colors[i*2+0] = new Color(0f, 0f, 0f /* not used */, 1f);
            colors[i*2+1] = new Color(0f, 0f, 0f /* not used */, 0f);
        }
        mesh_ = new Mesh();
        mesh_.name = "debris";
        mesh_.vertices = vertices;
        mesh_.colors = colors;
        mesh_.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);
        mesh_.SetIndices(indices, MeshTopology.Lines, 0);
        mesh_.UploadMeshData(true /* markNoLogerReadable */);
        material_ = material;
        material_.SetFloat("_Range", range_);
        material_.SetFloat("_RangeR", rangeR_);
        material_.SetColor(material_Color, new Color(1f, 1f, 1f));
    }
    
    public void render(Camera camera, double render_time)
    {
        if (material_ == null) {
            return;
        }

        if (delay_start_count_ > 0) {
            prev_view_matrix_ = camera.worldToCameraMatrix;
            --delay_start_count_;
            return;
        }
        var target_position = camera.transform.TransformPoint(new Vector3(0f, 0f, range_*0.5f));
        var matrix = prev_view_matrix_ * camera.cameraToWorldMatrix; // prev-view * inverted-cur-view
        material_.SetVector(material_TargetPosition, target_position);
        material_.SetMatrix(material_PrevInvMatrix, matrix);
        prev_view_matrix_ = camera.worldToCameraMatrix;
    }
}

} // namespace UTJ {

/*
 * End of Debris.cs
 */

/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

public class MySprite {

    public enum Kind {
        Square,
        Max,
    }

    public enum Type {
        None,
        Full,
        Half,
        Black,
        Red,
        Blue,
        Magenta,
        Green,
        Yellow,
        Cyan,
    }

    // singleton
    static MySprite instance_;
    public static MySprite Instance { get { return instance_ ?? (instance_ = new MySprite()); } }

    private Vector2[][] uv_list_;
    private Vector2[] size_list_;

    // UI
    const int RECT_MAX = 32;
    private Vector3[] vertices_;
    private Vector2[] uvs_;
    private Material material_;
    private MaterialPropertyBlock material_property_block_;
    private Mesh mesh_;
    private int index_;
    
    public Mesh getMesh() { return mesh_; }
    public Material getMaterial() { return material_; }
    public MaterialPropertyBlock getMaterialPropertyBlock() { return material_property_block_; }

    public void init(Sprite[] sprites, Material material)
    {
        uv_list_ = new Vector2[sprites.Length][];
        float atlas_width = sprites[0].texture.width;
        for (var i = 0; i < sprites.Length; ++i) {
            float x0 = sprites[i].textureRect.xMin / atlas_width;
            float x1 = sprites[i].textureRect.xMax / atlas_width;
            float y0 = sprites[i].textureRect.yMin / atlas_width;
            float y1 = sprites[i].textureRect.yMax / atlas_width;
            uv_list_[i] = new Vector2[4];
            uv_list_[i][0] = new Vector2(x0, y0);
            uv_list_[i][1] = new Vector2(x1, y0);
            uv_list_[i][2] = new Vector2(x0, y1);
            uv_list_[i][3] = new Vector2(x1, y1);
        }
        size_list_ = new Vector2[sprites.Length];
        for (var i = 0; i < sprites.Length; ++i) {
            size_list_[i].x = (sprites[i].textureRect.xMax - sprites[i].textureRect.xMin)*0.5f;
            size_list_[i].y = (sprites[i].textureRect.yMax - sprites[i].textureRect.yMin)*0.5f;
        }

        vertices_ = new Vector3[RECT_MAX*4];
        uvs_ = new Vector2[RECT_MAX*4];
        var triangles = new int[RECT_MAX*6];
        for (var i = 0; i < RECT_MAX; ++i) {
            triangles[i*6+0] = i*4+0;
            triangles[i*6+1] = i*4+1;
            triangles[i*6+2] = i*4+2;
            triangles[i*6+3] = i*4+2;
            triangles[i*6+4] = i*4+1;
            triangles[i*6+5] = i*4+3;
        }
        mesh_ = new Mesh();
        mesh_.name = "Sprite";
        mesh_.MarkDynamic();
        mesh_.vertices = vertices_;
        mesh_.uv = uvs_;
        mesh_.triangles = triangles;
        mesh_.bounds = new Bounds(Vector3.zero, Vector3.one * 99999999);

        material_ = material;
        material_property_block_ = new MaterialPropertyBlock();
        var col_list = new Vector4[] {
            new Color(0f, 0f, 0f, 0f), // None
            new Color(1f, 1f, 1f, 1f), // Full
            new Color(1f, 1f, 1f, 0.5f), // Half
            new Color(0f, 0f, 0f), // Black
            new Color(1f, 0f, 0f), // Red
            new Color(0f, 0f, 1f), // Blue
            new Color(1f, 0f, 1f), // Magenta
            new Color(0f, 1f, 0f), // Green
            new Color(1f, 1f, 0f), // Yellow
            new Color(0f, 1f, 1f), // Cyan
        };
        material_property_block_.SetVectorArray("_Colors", col_list);
    }

    public void begin()
    {
        index_ = 0;
    }

    public void end()
    {
        var cv = new Vector3(0f, 0f, -1f);
        for (var i = index_*4; i < vertices_.Length; ++i) {
            vertices_[i] = cv;
        }
    }

    public void put(float x, float y, float ratio, Kind kind, Type type, bool reverse = false)
    {
        int idx = (int)kind;
        float w = size_list_[idx].x;
        float h = size_list_[idx].y;
        var rect = new Rect(x, y, w*ratio, h*ratio);
        Debug.Log("here");
        put(ref rect, kind, type, reverse);
        Debug.Log("here");
    }
    public void put(ref Rect rect, Kind kind, Type type, bool reverse = false)
    {
        if (index_ >= RECT_MAX) {
            Debug.Log("EXCEED sprite POOL!");
            return;
        }
        int sprite_id = (int)kind;
        if (sprite_id >= (int)Kind.Max) {
            Debug.LogFormat("no such kind:{0}", sprite_id);
            return;
        }
        int idx = index_*4;
        float x0 = rect.xMin - rect.width*0.5f;
        float y0 = rect.yMin - rect.height*0.5f;
        float x1 = x0 + rect.width;
        float y1 = y0 + rect.height;
        vertices_[idx+0] = new Vector3(x0, y0, (float)type);
        vertices_[idx+1] = new Vector3(x1, y0, (float)type);
        vertices_[idx+2] = new Vector3(x0, y1, (float)type);
        vertices_[idx+3] = new Vector3(x1, y1, (float)type);
        if (!reverse) {
            uvs_[idx+0] = uv_list_[sprite_id][0];
            uvs_[idx+1] = uv_list_[sprite_id][1];
            uvs_[idx+2] = uv_list_[sprite_id][2];
            uvs_[idx+3] = uv_list_[sprite_id][3];
        } else {
            uvs_[idx+0] = uv_list_[sprite_id][1];
            uvs_[idx+1] = uv_list_[sprite_id][0];
            uvs_[idx+2] = uv_list_[sprite_id][3];
            uvs_[idx+3] = uv_list_[sprite_id][2];
        }
        ++index_;
    }
    public void put(float x, float y, float w, float h, Kind kind, Type type, bool reverse = false)
    {
        var rect = new Rect(x+w*0.5f, y+h*0.5f, w, h);
        put(ref rect, kind, type, reverse);
    }

    public void render()
    {
        mesh_.vertices = vertices_;
        mesh_.uv = uvs_;
    }
}

} // namespace UTJ {

/*
 * End of MySprite.cs
 */

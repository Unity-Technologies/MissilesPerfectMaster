/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class MyFontRenderer : MonoBehaviour {
    // singleton
    static MyFontRenderer instance_;
    public static MyFontRenderer Instance { get { return instance_; } }

    private MeshFilter mf_;
    private MeshRenderer mr_;

    void Awake()
    {
        instance_ = this;
    }

    public void init(/* Camera camera */)
    {
        mf_ = GetComponent<MeshFilter>();
        mr_ = GetComponent<MeshRenderer>();
        mf_.sharedMesh = MyFont.Instance.getMesh();
        mr_.sharedMaterial = MyFont.Instance.getMaterial();
        mr_.SetPropertyBlock(MyFont.Instance.getMaterialPropertyBlock());
    }
}

} // namespace UTJ {

/*
 * End of MyFontRenderer.cs
 */

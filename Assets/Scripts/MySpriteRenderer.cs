/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class MySpriteRenderer : MonoBehaviour {
    // singleton
    static MySpriteRenderer instance_;
    public static MySpriteRenderer Instance { get { return instance_; } }

    private MeshFilter mf_;
    private MeshRenderer mr_;

    void Awake()
    {
        instance_ = this;
    }

    public void init(Camera camera)
    {
        mf_ = GetComponent<MeshFilter>();
        mr_ = GetComponent<MeshRenderer>();
        mf_.sharedMesh = MySprite.Instance.getMesh();
        mr_.sharedMaterial = MySprite.Instance.getMaterial();
        mr_.SetPropertyBlock(MySprite.Instance.getMaterialPropertyBlock());
    }
}

} // namespace UTJ {

/*
 * End of MySpriteRenderer.cs
 */

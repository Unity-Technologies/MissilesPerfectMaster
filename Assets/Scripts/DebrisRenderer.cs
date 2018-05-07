/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class DebrisRenderer : MonoBehaviour {

    private MeshFilter mf_;
    private MeshRenderer mr_;

    void Start()
    {
        mf_ = GetComponent<MeshFilter>();
        mr_ = GetComponent<MeshRenderer>();
        mf_.sharedMesh = Debris.Instance.getMesh();
        mr_.sharedMaterial = Debris.Instance.getMaterial();
    }
}

} // namespace UTJ {

/*
 * End of DebrisRenderer.cs
 */

/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

public class PerformanceFetcher : MonoBehaviour {

    void OnPreRender()
    {
        PerformanceMeter.Instance.beginConsoleRender();
        MissileManager.Instance.SyncComputeBuffer();
    }

    void OnPreCull()
    {
        PerformanceMeter.Instance.endConsoleRender();
        // MissileManager.Instance.SyncComputeBuffer();
    }

    void OnPostRender()
    {
        // MissileManager.Instance.SyncComputeBuffer();
    }
}

} // namespace UTJ {

/*
 * End of PerformanceFetcher.cs
 */

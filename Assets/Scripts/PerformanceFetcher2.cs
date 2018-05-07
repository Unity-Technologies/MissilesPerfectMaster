/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

public class PerformanceFetcher2 : MonoBehaviour {

    // void OnPreRender()
    // {
    // }

    void OnPostRender()
    {
        PerformanceMeter.Instance.endRender();
    }

}

} // namespace UTJ {

/*
 * End of PerformanceFetcher2.cs
 */

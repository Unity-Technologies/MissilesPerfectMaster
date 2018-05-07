using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTest : MonoBehaviour {

    UTJ.RigidbodyTransform rb;

    // Use this for initialization
    void Start () {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        var q = Quaternion.identity;
        for (var i = 0; i < 1000000; ++i) {
            rb.addSpringTorque(ref q, 10f /* torque_level */);
        }
        sw.Stop();
        Debug.Log(sw.ElapsedTicks);
    }
    
    // Update is called once per frame
    void Update () {
        
    }
}

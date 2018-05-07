/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System;

namespace UTJ {

public class MyRandom
{
    public class Xorshift {
        private UInt32 x;
        private UInt32 y;
        private UInt32 z;
        private UInt32 w;
        
        public Xorshift () : this ((UInt64)DateTime.Now.Ticks) {}
        
        public Xorshift (UInt64 seed)
        {
            setSeed(seed);
        }
 
        public void setSeed(UInt64 seed)
        {
            x = 521288629u;
            y = (UInt32)(seed >> 32) & 0xFFFFFFFF;
            z = (UInt32)(seed & 0xFFFFFFFF);
            w = x ^ z;
        }
 
        public UInt32 getNext()
        {
            UInt32 t = x ^ (x << 11);
            x = y;
            y = z;
            z = w;
            w = (w ^ (w >> 19)) ^ (t ^ (t >> 8));
            return w;
        }
    }

    static Xorshift rand_ = new Xorshift();
    
    public static void setSeed(UInt64 seed)
    {
        rand_.setSeed(seed);
    }

    public static UInt32 get()
    {
        return rand_.getNext();
    }

    public static float Range(float min, float max)
    {
        int val = (int)(get() & 0xffff);
        float range = max - min;
        return (((float)val*range) / (float)(0xffff)) + min;
    }

    public static int Range(int min, int max)
    {
        long val = get();
        return (int)((val%(max-min))) + min;
    }

    public static bool ProbabilitySlow(float ratio)
    {
        float v = Range(0f, 1f);
        return (v < ratio);
    }

    public static bool Probability(float ratio) // optimized
    {
        UInt32 v = (rand_.getNext()) & 0xffff;
        UInt32 p = (UInt32)(((float)(1<<16)) * ratio);
        return (v < p);
    }

    public static bool Probability(float happen_times_per_second, float dt)
    {
        float v = Range(0f, 1f);
        return (v < happen_times_per_second * dt);
    }

    // 秒間何回発生してほしいか
    public static bool ProbabilityForSecond(float times_for_a_second, float dt)
    {
        float v = Range(0f, 1f);
        return (v < times_for_a_second * dt);
    }

    public static Vector3 onSphere(float radius)
    {
        float x = Range(-1f, 1f);
        float y = Range(-1f, 1f);
        float z = Range(-1f, 1f);
        float len2 = x*x + y*y + z*z;
        float len = Mathf.Sqrt(len2);
        float rlen = 1.0f/len;
        float v = rlen * radius;
        var point = new Vector3(x*v, y*v, z*v);
        return point;
    }

}

} // namespace UTJ {

/*
 * End of MyRandom.cs
 */

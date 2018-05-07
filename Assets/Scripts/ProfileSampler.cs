namespace UTJ {

using UnityEngine;
using UnityEngine.Profiling;

public struct ProfileSampler : System.IDisposable
{
    public ProfileSampler(string name)
    {
        Profiler.BeginSample(name);
    }

    public void Dispose()
    {
        Profiler.EndSample();
    }
}

} // namespace UTJ {

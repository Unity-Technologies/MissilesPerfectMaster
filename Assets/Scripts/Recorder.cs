/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

// e.g. ffmpeg -r 60 -i "./movie_work/20180502_%04d.png" -vcodec mpeg4 -b:v 18000k out.mov
//      ffmpeg -r 60 -i "./movie_work/20180502_%04d.png" -pix_fmt yuv420p out.mp4

using UnityEngine;
using System.Collections;

namespace UTJ {

public class Recorder : MonoBehaviour
{
    public KeyCode[] screenCaptureKeys;
    public KeyCode[] keyModifiers;

    private int minimumWidth = 1278;
    private int minimumHeight = 720;
    private string directory = "movie_work/";
    private string baseFilename;
    private int framerate = 60;
    public bool isRecording = false;
    public int endFrameno = 60;

    private int frameno = -1;

    void Reset ()
    {
        screenCaptureKeys = new KeyCode[]{ KeyCode.R };
        keyModifiers = new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift };
    
        baseFilename = System.DateTime.Now.ToString("yyyyMMdd");
    }

    void Start()
    {
        Reset();
        Time.captureFramerate = framerate;
    }

    void Update ()
    {
        PerformanceMeter.Instance.setRecording();
        checkRecodingKey();

        if (isRecording == true)
        {
            TakeScreenShot();
        }
    }

    bool checkRecodingKey()
    {
        bool isModifierPressed = false;
        bool ret = false;
        if (keyModifiers.Length > 0)
        {
            foreach (KeyCode keyCode in keyModifiers)
            {
                if (Input.GetKey(keyCode))
                {
                    isModifierPressed = true;
                    break;
                }
            }
        }

        if (isModifierPressed)
        {
            foreach (KeyCode keyCode in screenCaptureKeys)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    isRecording = !isRecording;
                }
            }
        }
        return ret;
    }

    public void TakeScreenShot ()
    {
        float rw = (float)minimumWidth / Screen.width;
        float rh = (float)minimumHeight / Screen.height;
        int scale = (int)Mathf.Ceil(Mathf.Max(rw, rh));

        ++frameno;
        string path = string.Format("{0}/../{1}{2}_{3:D4}.png",
                                    Application.dataPath, directory, baseFilename, frameno);
        ScreenCapture.CaptureScreenshot(path, scale);
        Debug.Log(string.Format("screen shot : path = {0}, scale = {1} (screen = {2}, {3})",
            path, scale, Screen.width, Screen.height), this);

        if (endFrameno > 0 && frameno >= endFrameno)
        {
            isRecording = false;
        }
    }
}

} // namespace UTJ {

/*
 * End of Recorder.cs
 */

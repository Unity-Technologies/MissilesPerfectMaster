/* -*- mode:CSharp; coding:utf-8-with-signature -*-
 */

using UnityEngine;
using System.Collections;

namespace UTJ {

class CV {
    public static Vector3 Vector3Zero = Vector3.zero;
    public static Vector3 Vector3One = Vector3.one;
    public static Vector3 Vector3Forward = Vector3.forward;
    public static Vector3 Vector3Back = Vector3.back;
    public static Vector3 Vector3Left = Vector3.left;
    public static Vector3 Vector3Right = Vector3.right;
    public static Vector3 Vector3Up = Vector3.up;
    public static Vector3 Vector3Down = Vector3.down;
    public static Quaternion QuaternionIdentity = Quaternion.identity;
    public static Quaternion Quaternion180Y = Quaternion.Euler(0f, 180f, 0f);

    public static Vector3[] Vector3ArrayEmpty = new Vector3[0];
    public static Vector2[] Vector2ArrayEmpty = new Vector2[0];
    public static int[] IntArrayEmpty = new int[0];
}

} // namespace UTJ {

/*
 * End of CV.cs
 */

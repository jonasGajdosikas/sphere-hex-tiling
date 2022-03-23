using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Metrics
{
    public static readonly float phi = (Mathf.Sqrt(5) - 1) / 2f;
    public static Vector3[] IcoVertices =
    {
        new Vector3(-1, 0, +phi),
        new Vector3(0, -phi, +1),
        new Vector3(+1, 0, +phi),
        new Vector3(+phi, +1, 0),
        new Vector3(+phi, -1, 0),
        new Vector3(+1, 0, -phi),
        new Vector3(0, +phi, +1),
        new Vector3(0, +phi, -1),
        new Vector3(-phi, +1, 0),
        new Vector3(-1, 0, -phi),
        new Vector3(-phi, -1, 0),
        new Vector3(0, -phi, -1)
    };
    public static Vector3[] IcoVerticesRotated
    {
        get
        {
            Vector3[] result = new Vector3[20];
            Quaternion rotation = Quaternion.FromToRotation(IcoVertices[0], Vector3.up);
            for (int i = 0; i < 20; i++) { result[i] = Rotated(IcoVertices[i], rotation); }
            return result;
        }
    }
    static Vector3 Rotated(Vector3 initial, Quaternion rotation)
    {
        return 2f * new Vector3(
            (.5f - rotation.y * rotation.y - rotation.z * rotation.z) * initial.x +
            (rotation.x * rotation.y - rotation.z * rotation.w) * initial.y +
            (rotation.z * rotation.x + rotation.y * rotation.w) * initial.z,
            (rotation.x * rotation.y + rotation.z * rotation.w) * initial.x +
            (.5f - rotation.x * rotation.x - rotation.z * rotation.z) * initial.y +
            (rotation.y * rotation.z - rotation.x * rotation.w) * initial.z,
            (rotation.z * rotation.x - rotation.y * rotation.w) * initial.x +
            (rotation.y * rotation.z + rotation.x * rotation.w) * initial.y +
            (-5f - rotation.x * rotation.x - rotation.y * rotation.y) * initial.z);
    }
    public static int[][] IcoTriangles = new int[20][]
    {
        new int[] { 0, 1, 2 },
        new int[] { 3, 2, 1 },
        new int[] { 2, 3, 4 },
        new int[] { 5, 4, 3 },

        new int[] { 0, 2, 6 },
        new int[] { 4, 6, 2 },
        new int[] { 6, 4, 7 },
        new int[] { 5, 7, 4 },

        new int[] { 0, 6, 8 },
        new int[] { 7, 8, 6 },
        new int[] { 8, 7, 9 },
        new int[] { 5, 9, 7 },

        new int[] { 0, 8, 10 },
        new int[] { 9, 10, 8 },
        new int[] { 10, 9, 11 },
        new int[] { 5, 11, 9},

        new int[] { 0, 10, 1 },
        new int[] { 11, 1, 10 },
        new int[] { 1, 11, 3 },
        new int[] { 5, 3, 11 }
    };
}

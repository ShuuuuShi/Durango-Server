using System;

namespace UnityEngine;

public struct Vector2
{
    public float x;
    public float y;

    public Vector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2 zero => new Vector2(0f, 0f);
    public static Vector2 one => new Vector2(1f, 1f);

    public float magnitude => (float)Math.Sqrt(x * x + y * y);
    public float sqrMagnitude => x * x + y * y;
    public static float Distance(Vector2 a, Vector2 b) => (a - b).magnitude;
    public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
    public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);
    public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.x * d, a.y * d);
    public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.x / d, a.y / d);
}

public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2Int zero => new Vector2Int(0, 0);
    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.x + b.x, a.y + b.y);
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.x - b.x, a.y - b.y);
}

public struct Vector3
{
    public float x;
    public float y;
    public float z;

    public Vector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static Vector3 zero => new Vector3(0f, 0f, 0f);
    public static Vector3 one => new Vector3(1f, 1f, 1f);
    public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
    public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
    public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.x * d, a.y * d, a.z * d);
}

public class TextAsset
{
    public string text = "";
    public byte[] bytes = new byte[0];
}

public struct Keyframe
{
    public float time;
    public float value;
    public float inTangent;
    public float outTangent;

    public Keyframe(float time, float value)
    {
        this.time = time;
        this.value = value;
        inTangent = 0f;
        outTangent = 0f;
    }
}

public class AnimationCurve
{
    public AnimationCurve()
    {
    }

    public AnimationCurve(params Keyframe[] keys)
    {
    }

    public float Evaluate(float t) => 0f;
}

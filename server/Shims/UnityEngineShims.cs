namespace UnityEngine;

public static class Mathf
{
    public const float PI = 3.14159274f;

    public static int RoundToInt(float f) => (int)System.Math.Round(f);
    public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    public static float Clamp(float value, float min, float max) => value < min ? min : value > max ? max : value;
    public static int Max(int a, int b) => a > b ? a : b;
    public static int Min(int a, int b) => a < b ? a : b;
    public static float Max(float a, float b) => a > b ? a : b;
    public static float Min(float a, float b) => a < b ? a : b;
    public static float Abs(float a) => System.Math.Abs(a);
    public static int Abs(int a) => System.Math.Abs(a);
    public static float Sqrt(float a) => (float)System.Math.Sqrt(a);
    public static float Pow(float a, float b) => (float)System.Math.Pow(a, b);
    public static float Floor(float a) => (float)System.Math.Floor(a);
    public static float Ceil(float a) => (float)System.Math.Ceiling(a);
    public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp(t, 0f, 1f);
    public static float Sign(float f) => f >= 0f ? 1f : -1f;
    public static float Round(float f) => (float)System.Math.Round(f);
    public static bool Approximately(float a, float b) => System.Math.Abs(a - b) < 0.00001f;
    public static float Atan2(float y, float x) => (float)System.Math.Atan2(y, x);
}

public static class Random
{
    private static readonly System.Random rng = new System.Random();

    public static int Range(int minInclusive, int maxExclusive) => rng.Next(minInclusive, maxExclusive);
    public static float Range(float minInclusive, float maxInclusive) => (float)(rng.NextDouble() * (maxInclusive - minInclusive) + minInclusive);
    public static float value => (float)rng.NextDouble();
}

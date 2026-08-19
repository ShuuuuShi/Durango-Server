using System.Collections.Generic;

public static class KUtility
{
    public static int GetSize<T>(T[] array) => array?.Length ?? 0;
    public static int GetSize<T>(List<T> list) => list?.Count ?? 0;
    public static int GetSize<T>(IList<T> list) => list?.Count ?? 0;
    public static int GetSize<TKey, TValue>(Dictionary<TKey, TValue> dict) => dict?.Count ?? 0;
}

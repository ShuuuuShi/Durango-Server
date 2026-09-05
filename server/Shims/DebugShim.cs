public static class Debug
{
    public static bool isDebugBuild = false;

    public static void Log(object message) => System.Console.WriteLine("[LOG] " + message);
    public static void LogWarning(object message) => System.Console.WriteLine("[WARN] " + message);
    public static void LogError(object message) => System.Console.WriteLine("[ERROR] " + message);
    public static void LogException(System.Exception e) => System.Console.WriteLine("[EXC] " + e);
}

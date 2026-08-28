using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using DurangoServer.Modding;

namespace DurangoServer.Core;

/// <summary>
/// Runtime method override pipeline. Harmony is used only as the small detour layer;
/// all mod callbacks remain behind the SDK contract and are isolated per callback.
/// </summary>
internal sealed class MethodOverrideManager
{
    private sealed class Registration
    {
        public string ModId = "";
        public string MethodId = "";
        public ModMethodOverrideKind Kind;
        public ModMethodOverrideHandler Handler = null!;
        public int Priority;
        public long Sequence;
    }

    private sealed class Target
    {
        public MethodBase Method = null!;
        public string MethodId = "";
        public readonly List<Registration> Registrations = new();
    }

    private readonly object _gate = new();
    private readonly Dictionary<MethodBase, Target> _targets = new();
    private readonly Harmony _harmony = new("durango.server.method-overrides");
    private long _sequence;
    [ThreadStatic] private static HashSet<MethodBase>? _activeMethods;

    public bool Register(
        string modId,
        string requestedMethodId,
        ModMethodOverrideKind kind,
        ModMethodOverrideHandler handler,
        int priority,
        out string error,
        out string resolvedMethodId)
    {
        error = "";
        resolvedMethodId = "";
        if (string.IsNullOrWhiteSpace(modId) || handler == null)
        {
            error = "mod id and handler are required";
            return false;
        }
        if (!TryResolveTarget(requestedMethodId, out MethodBase? method, out resolvedMethodId, out error))
        {
            return false;
        }

        lock (_gate)
        {
            if (!_targets.TryGetValue(method, out Target? target))
            {
                target = new Target { Method = method, MethodId = resolvedMethodId };
                try
                {
                    InstallDetour(method);
                }
                catch (Exception e)
                {
                    error = "detour install failed: " + e.Message;
                    return false;
                }
                _targets.Add(method, target);
            }

            if (kind == ModMethodOverrideKind.Replace &&
                target.Registrations.Any(x => x.Kind == ModMethodOverrideKind.Replace &&
                    !string.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase)))
            {
                error = "another mod already owns the Replace slot for this method";
                return false;
            }

            target.Registrations.Add(new Registration
            {
                ModId = modId,
                MethodId = target.MethodId,
                Kind = kind,
                Handler = handler,
                Priority = priority,
                Sequence = ++_sequence
            });
            target.Registrations.Sort(CompareRegistration);
            return true;
        }
    }

    public IReadOnlyList<string> GetRegisteredForMod(string modId)
    {
        lock (_gate)
        {
            return _targets.Values
                .SelectMany(x => x.Registrations)
                .Where(x => string.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase))
                .Select(x => $"{x.Kind}:{x.MethodId}")
                .ToArray();
        }
    }

    public int RemoveForMod(string modId)
    {
        int removed = 0;
        lock (_gate)
        {
            foreach (Target target in _targets.Values.ToArray())
            {
                removed += target.Registrations.RemoveAll(x => string.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase));
                if (target.Registrations.Count == 0)
                {
                    _harmony.Unpatch(target.Method, HarmonyPatchType.All, _harmony.Id);
                    _targets.Remove(target.Method);
                }
                else
                {
                    target.Registrations.Sort(CompareRegistration);
                }
            }
        }
        return removed;
    }

    public void RemoveAll()
    {
        lock (_gate)
        {
            _harmony.UnpatchAll(_harmony.Id);
            _targets.Clear();
        }
    }

    internal bool InvokePrefix(
        MethodBase method,
        object? instance,
        object?[] arguments,
        Action<object?> setResult,
        out bool skipOriginal)
    {
        skipOriginal = false;
        Registration[] registrations = Snapshot(method, ModMethodOverrideKind.Prefix, ModMethodOverrideKind.Replace);
        foreach (Registration registration in registrations)
        {
            ModMethodOverrideContext context = new(
                registration.ModId, registration.MethodId, registration.Kind,
                method, instance, arguments);
            if (registration.Kind == ModMethodOverrideKind.Replace)
            {
                context.SkipOriginal = true;
            }
            InvokeSafely(registration, context);
            if (context.HasResult)
            {
                setResult(context.Result);
            }
            if (context.SkipOriginal)
            {
                skipOriginal = true;
            }
        }
        return !skipOriginal;
    }

    internal bool TryEnter(MethodBase method)
    {
        _activeMethods ??= new HashSet<MethodBase>();
        return _activeMethods.Add(method);
    }

    internal void Exit(MethodBase method)
    {
        _activeMethods?.Remove(method);
    }

    internal void InvokePostfix(
        MethodBase method,
        object? instance,
        object?[] arguments,
        object? currentResult,
        Action<object?> setResult)
    {
        Registration[] registrations = Snapshot(method, ModMethodOverrideKind.Postfix);
        foreach (Registration registration in registrations)
        {
            ModMethodOverrideContext context = new(
                registration.ModId, registration.MethodId, registration.Kind,
                method, instance, arguments, currentResult);
            InvokeSafely(registration, context);
            if (context.HasResult)
            {
                currentResult = context.Result;
                setResult(currentResult);
            }
        }
    }

    private Registration[] Snapshot(MethodBase method, params ModMethodOverrideKind[] kinds)
    {
        lock (_gate)
        {
            if (!_targets.TryGetValue(method, out Target? target)) return Array.Empty<Registration>();
            return target.Registrations.Where(x => kinds.Contains(x.Kind)).ToArray();
        }
    }

    private static int CompareRegistration(Registration left, Registration right)
    {
        int priority = right.Priority.CompareTo(left.Priority);
        if (priority != 0) return priority;
        int mod = string.Compare(left.ModId, right.ModId, StringComparison.OrdinalIgnoreCase);
        return mod != 0 ? mod : left.Sequence.CompareTo(right.Sequence);
    }

    private static void InvokeSafely(Registration registration, ModMethodOverrideContext context)
    {
        Stopwatch sw = Stopwatch.StartNew();
        bool failed = false;
        try
        {
            registration.Handler(context);
        }
        catch (Exception e)
        {
            failed = true;
            context.Exception = e;
            // Replace failures must fail open to the original game method.
            if (registration.Kind == ModMethodOverrideKind.Replace)
            {
                context.SkipOriginal = false;
            }
            Console.WriteLine($"[mods] method {registration.Kind} {registration.MethodId} from '{registration.ModId}' failed after {sw.Elapsed.TotalMilliseconds:F1}ms: {e.Message}");
        }
        finally
        {
            PluginManager.Instance?.RecordMethodOverrideCall(registration.ModId, sw.Elapsed.TotalMilliseconds, failed);
        }
    }

    private void InstallDetour(MethodBase method)
    {
        if (method.IsAbstract || method.ContainsGenericParameters)
            throw new InvalidOperationException("abstract/generic methods are not supported");
        if (method.IsConstructor)
            throw new InvalidOperationException("constructors are not supported; use lifecycle hooks");
        if (method is MethodInfo info && (info.ReturnType.IsByRef || info.ReturnType.IsPointer))
            throw new InvalidOperationException("by-ref/pointer return methods are not supported");

        string prefixName = method.IsStatic ? nameof(MethodOverrideBridge.PrefixWithResultStatic) : nameof(MethodOverrideBridge.PrefixWithResult);
        string postfixName = method.IsStatic ? nameof(MethodOverrideBridge.PostfixWithResultStatic) : nameof(MethodOverrideBridge.PostfixWithResult);
        MethodInfo prefix = CreatePatchMethod(prefixName, method, hasResult: true);
        MethodInfo postfix = CreatePatchMethod(postfixName, method, hasResult: true);
        if (method is MethodInfo returnInfo && returnInfo.ReturnType == typeof(void))
        {
            prefix = typeof(MethodOverrideBridge).GetMethod(
                method.IsStatic ? nameof(MethodOverrideBridge.PrefixVoidStatic) : nameof(MethodOverrideBridge.PrefixVoid),
                BindingFlags.Public | BindingFlags.Static)!;
            postfix = typeof(MethodOverrideBridge).GetMethod(
                method.IsStatic ? nameof(MethodOverrideBridge.PostfixVoidStatic) : nameof(MethodOverrideBridge.PostfixVoid),
                BindingFlags.Public | BindingFlags.Static)!;
        }
        MethodInfo finalizer = typeof(MethodOverrideBridge).GetMethod(nameof(MethodOverrideBridge.Finalizer), BindingFlags.Public | BindingFlags.Static)!;
        _harmony.Patch(method,
            prefix: new HarmonyMethod(prefix),
            postfix: new HarmonyMethod(postfix),
            finalizer: new HarmonyMethod(finalizer));
    }

    private static MethodInfo CreatePatchMethod(string name, MethodBase original, bool hasResult)
    {
        MethodInfo method = typeof(MethodOverrideBridge).GetMethod(name, BindingFlags.Public | BindingFlags.Static)!;
        if (!hasResult || original is not MethodInfo info || info.ReturnType == typeof(void)) return method;
        return method.MakeGenericMethod(info.ReturnType);
    }

    private static bool TryResolveTarget(string requested, out MethodBase? method, out string resolved, out string error)
    {
        method = null;
        resolved = "";
        error = "";
        if (string.IsNullOrWhiteSpace(requested) || !requested.Contains("::", StringComparison.Ordinal))
        {
            error = "method id must be Namespace.Type::Method[(ParameterType,...)]";
            return false;
        }
        string[] parts = requested.Split(new[] { "::" }, 2, StringSplitOptions.None);
        Type? type = FindServerType(parts[0].Trim());
        if (type == null)
        {
            error = "target type not found in server assembly";
            return false;
        }

        string methodName = parts[1].Trim();
        string[]? requestedParameters = null;
        int open = methodName.IndexOf('(');
        if (open >= 0)
        {
            if (!methodName.EndsWith(")", StringComparison.Ordinal))
            {
                error = "method parameter list is malformed";
                return false;
            }
            string parameterText = methodName.Substring(open + 1, methodName.Length - open - 2).Trim();
            requestedParameters = parameterText.Length == 0
                ? Array.Empty<string>()
                : parameterText.Split(',').Select(x => x.Trim()).ToArray();
            methodName = methodName.Substring(0, open).Trim();
        }
        if (methodName.Length == 0)
        {
            error = "method name is empty";
            return false;
        }

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        MethodInfo[] candidates = type.GetMethods(flags)
            .Where(x => string.Equals(x.Name, methodName, StringComparison.Ordinal))
            .Where(x => requestedParameters == null || ParametersMatch(x.GetParameters(), requestedParameters))
            .ToArray();
        if (candidates.Length != 1)
        {
            error = candidates.Length == 0 ? "target method not found" : "target method is ambiguous; include parameter types";
            return false;
        }
        method = candidates[0];
        resolved = $"{type.FullName}::{method.Name}({string.Join(",", method.GetParameters().Select(x => NormalizeTypeName(x.ParameterType)))}):{NormalizeTypeName((method as MethodInfo)?.ReturnType ?? typeof(void))}";
        return true;
    }

    private static bool ParametersMatch(ParameterInfo[] actual, string[] requested)
    {
        return actual.Length == requested.Length && actual.Zip(requested, (a, r) => TypeNameMatches(a.ParameterType, r)).All(x => x);
    }

    private static bool TypeNameMatches(Type type, string requested)
    {
        string normalized = requested.Trim().Replace("global::", "", StringComparison.Ordinal);
        if (type.IsByRef) type = type.GetElementType()!;
        string full = NormalizeTypeName(type);
        if (string.Equals(full, normalized, StringComparison.Ordinal) || string.Equals(type.Name, normalized, StringComparison.Ordinal)) return true;
        return normalized switch
        {
            "bool" => type == typeof(bool),
            "byte" => type == typeof(byte),
            "short" => type == typeof(short),
            "int" => type == typeof(int),
            "long" => type == typeof(long),
            "float" => type == typeof(float),
            "double" => type == typeof(double),
            "string" => type == typeof(string),
            "object" => type == typeof(object),
            _ => false
        };
    }

    private static string NormalizeTypeName(Type type)
    {
        if (type.IsByRef) type = type.GetElementType()!;
        return type.FullName?.Replace('+', '.') ?? type.Name;
    }

    private static Type? FindServerType(string name)
    {
        Assembly server = typeof(PluginManager).Assembly;
        return server.GetType(name, throwOnError: false, ignoreCase: false)
            ?? server.GetTypes().FirstOrDefault(x => string.Equals(x.FullName?.Replace('+', '.'), name, StringComparison.Ordinal));
    }

    // Harmony calls these public static methods after it has generated the detour.
    public static class MethodOverrideBridge
    {
        public static bool PrefixVoid(MethodBase __originalMethod, object __instance, object[] __args, out bool __state)
        {
            MethodOverrideManager? manager = PluginManager.Instance?.MethodOverrides;
            __state = manager?.TryEnter(__originalMethod) == true;
            return !__state || manager!.InvokePrefix(__originalMethod, __instance, __args, _ => { }, out _);
        }

        public static void PostfixVoid(MethodBase __originalMethod, object __instance, object[] __args, bool __state)
        {
            if (__state)
                PluginManager.Instance?.MethodOverrides.InvokePostfix(__originalMethod, __instance, __args, null, _ => { });
        }

        public static bool PrefixVoidStatic(MethodBase __originalMethod, object[] __args, out bool __state)
        {
            MethodOverrideManager? manager = PluginManager.Instance?.MethodOverrides;
            __state = manager?.TryEnter(__originalMethod) == true;
            return !__state || manager!.InvokePrefix(__originalMethod, null, __args, _ => { }, out _);
        }

        public static void PostfixVoidStatic(MethodBase __originalMethod, object[] __args, bool __state)
        {
            if (__state)
                PluginManager.Instance?.MethodOverrides.InvokePostfix(__originalMethod, null, __args, null, _ => { });
        }

        public static bool PrefixWithResult<T>(MethodBase __originalMethod, object __instance, object[] __args, ref T __result, out bool __state)
        {
            MethodOverrideManager? manager = PluginManager.Instance?.MethodOverrides;
            __state = manager?.TryEnter(__originalMethod) == true;
            if (!__state) return true;
            object? result = __result;
            bool runOriginal = manager!.InvokePrefix(
                __originalMethod, __instance, __args,
                value => result = value, out _);
            if (!TryCoerce(result, out T converted)) return true;
            __result = converted;
            return runOriginal;
        }

        public static void PostfixWithResult<T>(MethodBase __originalMethod, object __instance, object[] __args, ref T __result, bool __state)
        {
            if (!__state) return;
            object? result = __result;
            PluginManager.Instance?.MethodOverrides.InvokePostfix(
                __originalMethod, __instance, __args, result,
                value => result = value);
            if (TryCoerce(result, out T converted)) __result = converted;
        }

        public static bool PrefixWithResultStatic<T>(MethodBase __originalMethod, object[] __args, ref T __result, out bool __state)
        {
            MethodOverrideManager? manager = PluginManager.Instance?.MethodOverrides;
            __state = manager?.TryEnter(__originalMethod) == true;
            if (!__state) return true;
            object? result = __result;
            bool runOriginal = manager!.InvokePrefix(
                __originalMethod, null, __args,
                value => result = value, out _);
            if (!TryCoerce(result, out T converted)) return true;
            __result = converted;
            return runOriginal;
        }

        public static void PostfixWithResultStatic<T>(MethodBase __originalMethod, object[] __args, ref T __result, bool __state)
        {
            if (!__state) return;
            object? result = __result;
            PluginManager.Instance?.MethodOverrides.InvokePostfix(
                __originalMethod, null, __args, result,
                value => result = value);
            if (TryCoerce(result, out T converted)) __result = converted;
        }

        public static Exception? Finalizer(MethodBase __originalMethod, bool __state, Exception? __exception)
        {
            if (__state) PluginManager.Instance?.MethodOverrides.Exit(__originalMethod);
            return __exception;
        }

        private static bool TryCoerce<T>(object? value, out T result)
        {
            try
            {
                if (value == null) { result = default!; return true; }
                if (value is T typed) { result = typed; return true; }
                Type target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                result = (T)Convert.ChangeType(value, target);
                return true;
            }
            catch (Exception e)
            {
                result = default!;
                Console.WriteLine($"[mods] method override result rejected for {typeof(T).FullName}: {e.Message}");
                return false;
            }
        }
    }
}

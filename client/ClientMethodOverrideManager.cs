using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Durango.Modding;
using HarmonyLib;

internal sealed class ClientMethodOverrideManager
{
    private sealed class Registration
    {
        public string ModId;
        public string MethodId;
        public ClientMethodOverrideKind Kind;
        public ClientMethodOverrideHandler Handler;
        public int Priority;
        public long Sequence;
    }

    private sealed class Target
    {
        public MethodBase Method;
        public string MethodId;
        public readonly List<Registration> Registrations = new List<Registration>();
    }

    private readonly object _gate = new object();
    private readonly Dictionary<MethodBase, Target> _targets = new Dictionary<MethodBase, Target>();
    private readonly Harmony _harmony = new Harmony("durango.client.method-overrides");
    private long _sequence;
    [ThreadStatic] private static HashSet<MethodBase> _activeMethods;

    public bool Register(string modId, string requestedMethodId, ClientMethodOverrideKind kind,
        ClientMethodOverrideHandler handler, int priority, out string error, out string resolvedMethodId)
    {
        error = "";
        resolvedMethodId = "";
        MethodBase method;
        if (String.IsNullOrEmpty(modId) || handler == null)
        {
            error = "mod id and handler are required";
            return false;
        }
        if (!TryResolveTarget(requestedMethodId, out method, out resolvedMethodId, out error)) return false;

        lock (_gate)
        {
            Target target;
            if (!_targets.TryGetValue(method, out target))
            {
                target = new Target { Method = method, MethodId = resolvedMethodId };
                try { InstallDetour(method); }
                catch (Exception e) { error = "detour install failed: " + e.Message; return false; }
                _targets.Add(method, target);
            }
            if (kind == ClientMethodOverrideKind.Replace && target.Registrations.Any(delegate(Registration x)
                { return x.Kind == ClientMethodOverrideKind.Replace && !String.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase); }))
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

    public string[] GetRegisteredForMod(string modId)
    {
        lock (_gate)
        {
            return _targets.Values.SelectMany(delegate(Target x) { return x.Registrations; })
                .Where(delegate(Registration x) { return String.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase); })
                .Select(delegate(Registration x) { return x.Kind + ":" + x.MethodId; }).ToArray();
        }
    }

    public int RemoveForMod(string modId)
    {
        int removed = 0;
        lock (_gate)
        {
            Target[] targets = _targets.Values.ToArray();
            for (int i = 0; i < targets.Length; i++)
            {
                Target target = targets[i];
                removed += target.Registrations.RemoveAll(delegate(Registration x)
                    { return String.Equals(x.ModId, modId, StringComparison.OrdinalIgnoreCase); });
                if (target.Registrations.Count == 0)
                {
                    _harmony.Unpatch(target.Method, HarmonyPatchType.All, _harmony.Id);
                    _targets.Remove(target.Method);
                }
                else target.Registrations.Sort(CompareRegistration);
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

    internal bool TryEnter(MethodBase method)
    {
        if (_activeMethods == null) _activeMethods = new HashSet<MethodBase>();
        return _activeMethods.Add(method);
    }

    internal void Exit(MethodBase method)
    {
        if (_activeMethods != null) _activeMethods.Remove(method);
    }

    internal bool InvokePrefix(MethodBase method, object instance, object[] arguments,
        Action<object> setResult, out bool skipOriginal)
    {
        skipOriginal = false;
        Registration[] registrations = Snapshot(method, ClientMethodOverrideKind.Prefix, ClientMethodOverrideKind.Replace);
        for (int i = 0; i < registrations.Length; i++)
        {
            Registration registration = registrations[i];
            ClientMethodOverrideContext context = new ClientMethodOverrideContext(registration.ModId,
                registration.MethodId, registration.Kind, method, instance, arguments, null);
            if (registration.Kind == ClientMethodOverrideKind.Replace) context.SkipOriginal = true;
            InvokeSafely(registration, context);
            if (context.HasResult) setResult(context.Result);
            if (context.SkipOriginal) skipOriginal = true;
        }
        return !skipOriginal;
    }

    internal void InvokePostfix(MethodBase method, object instance, object[] arguments,
        object currentResult, Action<object> setResult)
    {
        Registration[] registrations = Snapshot(method, ClientMethodOverrideKind.Postfix);
        for (int i = 0; i < registrations.Length; i++)
        {
            Registration registration = registrations[i];
            ClientMethodOverrideContext context = new ClientMethodOverrideContext(registration.ModId,
                registration.MethodId, registration.Kind, method, instance, arguments, currentResult);
            InvokeSafely(registration, context);
            if (context.HasResult) { currentResult = context.Result; setResult(currentResult); }
        }
    }

    private Registration[] Snapshot(MethodBase method, params ClientMethodOverrideKind[] kinds)
    {
        lock (_gate)
        {
            Target target;
            if (!_targets.TryGetValue(method, out target)) return new Registration[0];
            return target.Registrations.Where(delegate(Registration x) { return kinds.Contains(x.Kind); }).ToArray();
        }
    }

    private static int CompareRegistration(Registration left, Registration right)
    {
        int priority = right.Priority.CompareTo(left.Priority);
        if (priority != 0) return priority;
        int mod = String.Compare(left.ModId, right.ModId, StringComparison.OrdinalIgnoreCase);
        return mod != 0 ? mod : left.Sequence.CompareTo(right.Sequence);
    }

    private static void InvokeSafely(Registration registration, ClientMethodOverrideContext context)
    {
        Stopwatch sw = Stopwatch.StartNew();
        try { registration.Handler(context); }
        catch (Exception e)
        {
            context.Exception = e;
            if (registration.Kind == ClientMethodOverrideKind.Replace) context.SkipOriginal = false;
            ClientModLoader.Trace("[clientmods] method " + registration.Kind + " " + registration.MethodId +
                " from '" + registration.ModId + "' failed after " + sw.Elapsed.TotalMilliseconds.ToString("F1") + "ms: " + e.Message);
        }
    }

    private void InstallDetour(MethodBase method)
    {
        if (method.IsAbstract || method.ContainsGenericParameters) throw new InvalidOperationException("abstract/generic methods are not supported");
        if (method.IsConstructor) throw new InvalidOperationException("constructors are not supported");
        MethodInfo methodInfo = method as MethodInfo;
        if (methodInfo != null && (methodInfo.ReturnType.IsByRef || methodInfo.ReturnType.IsPointer))
            throw new InvalidOperationException("by-ref/pointer return methods are not supported");

        string prefixName = method.IsStatic ? "PrefixWithResultStatic" : "PrefixWithResult";
        string postfixName = method.IsStatic ? "PostfixWithResultStatic" : "PostfixWithResult";
        MethodInfo prefix = CreatePatchMethod(prefixName, method);
        MethodInfo postfix = CreatePatchMethod(postfixName, method);
        if (methodInfo != null && methodInfo.ReturnType == typeof(void))
        {
            prefix = typeof(Bridge).GetMethod(method.IsStatic ? "PrefixVoidStatic" : "PrefixVoid");
            postfix = typeof(Bridge).GetMethod(method.IsStatic ? "PostfixVoidStatic" : "PostfixVoid");
        }
        MethodInfo finalizer = typeof(Bridge).GetMethod("Finalizer");
        _harmony.Patch(method, new HarmonyMethod(prefix), new HarmonyMethod(postfix), null,
            new HarmonyMethod(finalizer));
    }

    private static MethodInfo CreatePatchMethod(string name, MethodBase original)
    {
        MethodInfo patch = typeof(Bridge).GetMethod(name);
        MethodInfo info = original as MethodInfo;
        return info == null || info.ReturnType == typeof(void) ? patch : patch.MakeGenericMethod(info.ReturnType);
    }

    private static bool TryResolveTarget(string requested, out MethodBase method, out string resolved, out string error)
    {
        method = null;
        resolved = "";
        error = "";
        if (String.IsNullOrEmpty(requested) || requested.IndexOf("::", StringComparison.Ordinal) < 0)
        {
            error = "method id must be Namespace.Type::Method[(ParameterType,...)]";
            return false;
        }
        string[] parts = requested.Split(new[] { "::" }, StringSplitOptions.None);
        if (parts.Length != 2) { error = "method id is malformed"; return false; }
        Type type = FindClientType(parts[0].Trim());
        if (type == null) { error = "target type not found in Assembly-CSharp"; return false; }

        string methodName = parts[1].Trim();
        string[] requestedParameters = null;
        int open = methodName.IndexOf('(');
        if (open >= 0)
        {
            if (!methodName.EndsWith(")")) { error = "method parameter list is malformed"; return false; }
            string text = methodName.Substring(open + 1, methodName.Length - open - 2).Trim();
            requestedParameters = text.Length == 0 ? new string[0] : text.Split(',').Select(delegate(string x) { return x.Trim(); }).ToArray();
            methodName = methodName.Substring(0, open).Trim();
        }
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        MethodInfo[] candidates = type.GetMethods(flags).Where(delegate(MethodInfo x)
        {
            return x.Name == methodName && (requestedParameters == null || ParametersMatch(x.GetParameters(), requestedParameters));
        }).ToArray();
        if (candidates.Length != 1)
        {
            error = candidates.Length == 0 ? "target method not found" : "target method is ambiguous; include parameter types";
            return false;
        }
        method = candidates[0];
        resolved = type.FullName + "::" + method.Name + "(" + String.Join(",", method.GetParameters().Select(delegate(ParameterInfo x)
            { return NormalizeTypeName(x.ParameterType); }).ToArray()) + "):" + NormalizeTypeName(candidates[0].ReturnType);
        return true;
    }

    private static bool ParametersMatch(ParameterInfo[] actual, string[] requested)
    {
        if (actual.Length != requested.Length) return false;
        for (int i = 0; i < actual.Length; i++) if (!TypeNameMatches(actual[i].ParameterType, requested[i])) return false;
        return true;
    }

    private static bool TypeNameMatches(Type type, string requested)
    {
        string normalized = requested.Trim().Replace("global::", "");
        if (type.IsByRef) type = type.GetElementType();
        string full = NormalizeTypeName(type);
        if (full == normalized || type.Name == normalized) return true;
        if (normalized == "bool") return type == typeof(bool);
        if (normalized == "int") return type == typeof(int);
        if (normalized == "float") return type == typeof(float);
        if (normalized == "double") return type == typeof(double);
        if (normalized == "string") return type == typeof(string);
        if (normalized == "object") return type == typeof(object);
        return false;
    }

    private static string NormalizeTypeName(Type type)
    {
        if (type.IsByRef) type = type.GetElementType();
        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    private static Type FindClientType(string name)
    {
        Assembly assembly = typeof(ClientModLoader).Assembly;
        Type type = assembly.GetType(name, false, false);
        if (type != null) return type;
        return assembly.GetTypes().FirstOrDefault(delegate(Type x) { return (x.FullName ?? "").Replace('+', '.') == name; });
    }

    public static class Bridge
    {
        public static bool PrefixVoid(MethodBase __originalMethod, object __instance, object[] __args, out bool __state)
        {
            __state = ClientModLoader.MethodOverrides.TryEnter(__originalMethod);
            return !__state || ClientModLoader.MethodOverrides.InvokePrefix(__originalMethod, __instance, __args, delegate { }, out _);
        }
        public static void PostfixVoid(MethodBase __originalMethod, object __instance, object[] __args, bool __state)
        { if (__state) ClientModLoader.MethodOverrides.InvokePostfix(__originalMethod, __instance, __args, null, delegate { }); }
        public static bool PrefixVoidStatic(MethodBase __originalMethod, object[] __args, out bool __state)
        {
            __state = ClientModLoader.MethodOverrides.TryEnter(__originalMethod);
            return !__state || ClientModLoader.MethodOverrides.InvokePrefix(__originalMethod, null, __args, delegate { }, out _);
        }
        public static void PostfixVoidStatic(MethodBase __originalMethod, object[] __args, bool __state)
        { if (__state) ClientModLoader.MethodOverrides.InvokePostfix(__originalMethod, null, __args, null, delegate { }); }
        public static bool PrefixWithResult<T>(MethodBase __originalMethod, object __instance, object[] __args, ref T __result, out bool __state)
        {
            __state = ClientModLoader.MethodOverrides.TryEnter(__originalMethod);
            if (!__state) return true;
            object result = __result;
            bool run = ClientModLoader.MethodOverrides.InvokePrefix(__originalMethod, __instance, __args, delegate(object value) { result = value; }, out _);
            T converted; if (!TryCoerce(result, out converted)) return true; __result = converted; return run;
        }
        public static void PostfixWithResult<T>(MethodBase __originalMethod, object __instance, object[] __args, ref T __result, bool __state)
        {
            if (!__state) return; object result = __result;
            ClientModLoader.MethodOverrides.InvokePostfix(__originalMethod, __instance, __args, result, delegate(object value) { result = value; });
            T converted; if (TryCoerce(result, out converted)) __result = converted;
        }
        public static bool PrefixWithResultStatic<T>(MethodBase __originalMethod, object[] __args, ref T __result, out bool __state)
        {
            __state = ClientModLoader.MethodOverrides.TryEnter(__originalMethod);
            if (!__state) return true; object result = __result;
            bool run = ClientModLoader.MethodOverrides.InvokePrefix(__originalMethod, null, __args, delegate(object value) { result = value; }, out _);
            T converted; if (!TryCoerce(result, out converted)) return true; __result = converted; return run;
        }
        public static void PostfixWithResultStatic<T>(MethodBase __originalMethod, object[] __args, ref T __result, bool __state)
        {
            if (!__state) return; object result = __result;
            ClientModLoader.MethodOverrides.InvokePostfix(__originalMethod, null, __args, result, delegate(object value) { result = value; });
            T converted; if (TryCoerce(result, out converted)) __result = converted;
        }
        public static Exception Finalizer(MethodBase __originalMethod, bool __state, Exception __exception)
        { if (__state) ClientModLoader.MethodOverrides.Exit(__originalMethod); return __exception; }

        private static bool TryCoerce<T>(object value, out T result)
        {
            try
            {
                if (value == null) { result = default(T); return true; }
                if (value is T) { result = (T)value; return true; }
                Type target = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                result = (T)Convert.ChangeType(value, target); return true;
            }
            catch (Exception e)
            {
                result = default(T);
                ClientModLoader.Trace("[clientmods] method override result rejected for " + typeof(T).FullName + ": " + e.Message);
                return false;
            }
        }
    }
}

using System;
using System.Reflection;

namespace DurangoServer.Modding;

/// <summary>How a mod participates in a game method call.</summary>
public enum ModMethodOverrideKind
{
    Prefix,
    Postfix,
    Replace
}

/// <summary>Callback invoked around an approved game method.</summary>
public delegate void ModMethodOverrideHandler(ModMethodOverrideContext context);

/// <summary>Mutable call data exposed to a method override handler.</summary>
public sealed class ModMethodOverrideContext
{
    public ModMethodOverrideContext(
        string modId,
        string methodId,
        ModMethodOverrideKind kind,
        MethodBase originalMethod,
        object? instance,
        object?[] arguments,
        object? result = null)
    {
        ModId = modId;
        MethodId = methodId;
        Kind = kind;
        OriginalMethod = originalMethod;
        Instance = instance;
        Arguments = arguments;
        Result = result;
    }

    public string ModId { get; }
    public string MethodId { get; }
    public ModMethodOverrideKind Kind { get; }
    public MethodBase OriginalMethod { get; }
    public object? Instance { get; }
    public object?[] Arguments { get; }
    public object? Result { get; private set; }
    public bool HasResult { get; private set; }
    public bool SkipOriginal { get; set; }
    public Exception? Exception { get; set; }

    public void SetResult(object? result)
    {
        Result = result;
        HasResult = true;
    }
}

/// <summary>Optional capability for mods that need to patch approved game methods.</summary>
public interface IModMethodOverridesApi
{
    /// <summary>
    /// Registers a Prefix, Postfix or Replace callback.
    /// methodId format: Namespace.Type::Method(Type1,Type2)
    /// Omitting the parameter list is allowed only when the method name is unambiguous.
    /// </summary>
    bool RegisterMethodOverride(
        string methodId,
        ModMethodOverrideKind kind,
        ModMethodOverrideHandler handler,
        int priority = 0);

    IReadOnlyList<string> GetRegisteredMethodOverrides();

    /// <summary>Removes every method override owned by the calling mod.</summary>
    int UnregisterMethodOverrides();
}

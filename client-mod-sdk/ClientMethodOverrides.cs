using System;
using System.Reflection;

namespace Durango.Modding
{
    public enum ClientMethodOverrideKind
    {
        Prefix,
        Postfix,
        Replace
    }

    public delegate void ClientMethodOverrideHandler(ClientMethodOverrideContext context);

    public sealed class ClientMethodOverrideContext
    {
        private object _result;

        public ClientMethodOverrideContext(string modId, string methodId, ClientMethodOverrideKind kind,
            MethodBase originalMethod, object instance, object[] arguments, object result)
        {
            ModId = modId;
            MethodId = methodId;
            Kind = kind;
            OriginalMethod = originalMethod;
            Instance = instance;
            Arguments = arguments;
            _result = result;
        }

        public string ModId { get; private set; }
        public string MethodId { get; private set; }
        public ClientMethodOverrideKind Kind { get; private set; }
        public MethodBase OriginalMethod { get; private set; }
        public object Instance { get; private set; }
        public object[] Arguments { get; private set; }
        public object Result { get { return _result; } }
        public bool HasResult { get; private set; }
        public bool SkipOriginal { get; set; }
        public Exception Exception { get; set; }

        public void SetResult(object result)
        {
            _result = result;
            HasResult = true;
        }
    }

    /// <summary>Optional client capability for patching methods in Assembly-CSharp.</summary>
    public interface IClientMethodOverridesApi
    {
        bool RegisterMethodOverride(string methodId, ClientMethodOverrideKind kind,
            ClientMethodOverrideHandler handler, int priority);
        string[] GetRegisteredMethodOverrides();
        int UnregisterMethodOverrides();
    }

    public interface IClientModLifecycle
    {
        void OnDisable(IClientModApi api);
    }
}

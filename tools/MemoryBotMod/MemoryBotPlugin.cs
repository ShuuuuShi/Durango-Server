using System;
using Durango.Modding;
using UnityEngine;

namespace DurangoMemoryBot
{

public sealed class MemoryBotPlugin : IClientPlugin
{
    public string Name { get { return "DurangoMemoryBot"; } }
    public string Version { get { return "0.1.0"; } }

    public void OnPreLoad(IClientModApi api)
    {
        api.Log("MVP preloaded (managed-state bridge; no raw memory access)");
    }

    public void OnLoad(IClientModApi api)
    {
        if (string.Equals(Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT"), "0", StringComparison.Ordinal))
        {
            api.Log("disabled by DURANGO_MEMORYBOT=0");
            return;
        }
        MemoryBotRuntime.Start(api);
    }

    public void OnPostLoad(IClientModApi api)
    {
        api.Log("MVP postload complete");
    }
}
}

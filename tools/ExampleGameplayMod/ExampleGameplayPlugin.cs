using System;
using System.Collections.Generic;
using DurangoServer.Modding;

namespace ExampleGameplayMod;

/// <summary>Reference/test mod for the optional event bus. Set DURANGO_CANCEL_CRAFT=1 or
/// DURANGO_CANCEL_GATHER=1 to exercise cancellable before hooks.</summary>
public sealed class ExampleGameplayPlugin : IGamePlugin, IModIdentity
{
    private IModStorage? _storage;
    private int _events;

    public string Name => "ExampleGameplayMod";
    public string Version => "0.1.0";
    public string Id => "examplegameplay";
    public string ApiVersion => "1.1";
    public IReadOnlyList<string> Dependencies => Array.Empty<string>();

    public void OnPreLoad(IModApi api) { api.Log("event test mod preloaded"); }

    public void OnLoad(IModApi api)
    {
        if (api is not IModEventsApi events)
        {
            api.Log("event bus unavailable; running compatibility mode");
            return;
        }
        _storage = events.Storage;
        if (api is IModMethodOverridesApi overrides)
        {
            overrides.RegisterMethodOverride(
                "DurangoServer.Core.ServerPlayer::StatusStaminaCostDelta()",
                ModMethodOverrideKind.Prefix,
                context =>
                {
                    if (Environment.GetEnvironmentVariable("DURANGO_OVERRIDE_STAMINA") == "1")
                    {
                        context.SetResult(0.25f);
                        context.SkipOriginal = true;
                    }
                },
                priority: 100);
            overrides.RegisterMethodOverride(
                "DurangoServer.Core.QuestData::ValidateAndReport()",
                ModMethodOverrideKind.Prefix,
                context =>
                {
                    if (Environment.GetEnvironmentVariable("DURANGO_OVERRIDE_PROBE") == "1")
                    {
                        api.Log("method override probe hit: QuestData.ValidateAndReport");
                        context.SetResult(false);
                        context.SkipOriginal = true;
                    }
                },
                priority: 100);
        }
        events.Subscribe("craft.before", e =>
        {
            if (Environment.GetEnvironmentVariable("DURANGO_CANCEL_CRAFT") == "1")
            {
                e.Cancel("ExampleGameplayMod: craft disabled for test");
                return;
            }
            api.Log("before craft recipe=" + Read(e, "recipe_id"));
        }, EventPriority.High);
        events.Subscribe("gather.before", e =>
        {
            if (Environment.GetEnvironmentVariable("DURANGO_CANCEL_GATHER") == "1")
            {
                e.Cancel("ExampleGameplayMod: gather disabled for test");
            }
        }, EventPriority.High);
        events.Subscribe("craft.completed", e => Count(api, "craft.completed"), EventPriority.Monitor);
        events.Subscribe("gather.completed", e => Count(api, "gather.completed"), EventPriority.Monitor);
        events.Subscribe("player.died", e => Count(api, "player.died"), EventPriority.Monitor);
        api.RegisterCommand("eventstatus", (player, _) =>
            $"ExampleGameplayMod events={_events}, storage={(_storage?.Exists("events") == true ? "ok" : "new")}");
    }

    public void OnPostLoad(IModApi api) { api.Log("event test mod ready"); }

    private void Count(IModApi api, string name)
    {
        _events++;
        api.Log("event " + name);
        _storage?.SaveJson("events", "{\"count\":" + _events + "}");
    }

    private static string Read(IModEventContext context, string key) =>
        context.Data.TryGetValue(key, out string? value) ? value : "";
}

using System;
using System.Collections.Generic;

namespace DurangoServer.Modding;

/// <summary>Typed payloads for the gameplay event bus. Context is read-only except cancellation.</summary>
public interface IGameplayEvent : IModEventContext
{
    string ActionId { get; }
    IReadOnlyDictionary<string, string> Values { get; }
}

public sealed class GameplayEventContext : IGameplayEvent
{
    public string EventName { get; init; } = "";
    public string EventId { get; init; } = Guid.NewGuid().ToString("N");
    public double OccurredAt { get; init; }
    public IModPlayer? Player { get; init; }
    public bool IsBefore { get; init; }
    public bool IsCommitted { get; init; }
    public bool IsCancelled { get; private set; }
    public string? CancelReason { get; private set; }
    public IReadOnlyDictionary<string, string> Data { get; init; } = new Dictionary<string, string>();
    public string ActionId { get; init; } = "";
    public IReadOnlyDictionary<string, string> Values => Data;
    public void Cancel(string reason)
    {
        if (!IsBefore || IsCommitted || IsCancelled) return;
        IsCancelled = true;
        CancelReason = string.IsNullOrWhiteSpace(reason) ? "cancelled by mod" : reason;
    }
}

public static class GameplayEventNames
{
    public const string CraftBefore = "craft.before";
    public const string CraftCompleted = "craft.completed";
    public const string GatherBefore = "gather.before";
    public const string GatherCompleted = "gather.completed";
    public const string BuildingBeforePlace = "building.before_place";
    public const string BuildingPlaced = "building.placed";
    public const string CombatAttack = "combat.attack";
    public const string CombatDamage = "combat.damage";
    public const string InventoryAdded = "inventory.added";
    public const string InventoryRemoved = "inventory.removed";
}

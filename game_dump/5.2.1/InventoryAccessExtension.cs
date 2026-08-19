using System.Collections.Generic;
using Messages;
using Shared.Player;

public static class InventoryAccessExtension
{
	public static int GetTakenCount(this InventoryAccess access, string id, double at)
	{
		double? takenCountsValidUntil = access.TakenCountsValidUntil;
		if (!takenCountsValidUntil.HasValue || access.TakenCountsValidUntil.Value < at)
		{
			return 0;
		}
		if (access.TakenCounts == null)
		{
			return 0;
		}
		return access.TakenCounts.Get(id, 0);
	}

	public static InventoryAccess SetFriendAccessCount(this InventoryAccess access, Shared.Player.FriendType type, int value)
	{
		Dictionary<Shared.Player.FriendType, int> dictionary = access.Friends;
		if (dictionary == null)
		{
			dictionary = (access.Friends = new Dictionary<Shared.Player.FriendType, int>(default(FriendTypeComparer)));
		}
		dictionary[type] = value;
		return access;
	}

	public static InventoryAccess SetClanRoleAccessCount(this InventoryAccess access, int roleId, int value)
	{
		Dictionary<int, int> dictionary = access.ClanMembers;
		if (dictionary == null)
		{
			dictionary = (access.ClanMembers = new Dictionary<int, int>());
		}
		dictionary[roleId] = value;
		return access;
	}

	public static InventoryAccess SetOtherAccessCount(this InventoryAccess access, int value)
	{
		access.Others = value;
		return access;
	}
}

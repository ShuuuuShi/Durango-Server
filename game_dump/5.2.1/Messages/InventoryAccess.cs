using System.Collections.Generic;
using MsgPack;
using Shared.Player;

namespace Messages;

public struct InventoryAccess
{
	public int Others;

	public Dictionary<Shared.Player.FriendType, int> Friends;

	public Dictionary<int, int> ClanMembers;

	public Dictionary<string, int> TakenCounts;

	public double? TakenCountsValidUntil;

	public static void Pack(Packer packer, InventoryAccess val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		packer.Pack(val.Others);
		if (val.Friends == null)
		{
			packer.PackNull();
		}
		else if (val.Friends == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Friends.Count);
			foreach (KeyValuePair<Shared.Player.FriendType, int> friend in val.Friends)
			{
				packer.Pack((int)friend.Key);
				packer.Pack(friend.Value);
			}
		}
		if (val.ClanMembers == null)
		{
			packer.PackNull();
		}
		else if (val.ClanMembers == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ClanMembers.Count);
			foreach (KeyValuePair<int, int> clanMember in val.ClanMembers)
			{
				packer.Pack(clanMember.Key);
				packer.Pack(clanMember.Value);
			}
		}
		if (val.TakenCounts == null)
		{
			packer.PackNull();
		}
		else if (val.TakenCounts == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.TakenCounts.Count);
			foreach (KeyValuePair<string, int> takenCount in val.TakenCounts)
			{
				if (takenCount.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(takenCount.Key);
				}
				packer.Pack(takenCount.Value);
			}
		}
		if (!val.TakenCountsValidUntil.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.TakenCountsValidUntil.Value);
		}
	}

	public static InventoryAccess Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InventoryAccess result = default(InventoryAccess);
		result.Others = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Friends = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Dictionary<Shared.Player.FriendType, int> dictionary = new Dictionary<Shared.Player.FriendType, int>(num, default(FriendTypeComparer));
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				int num2 = unpacker.LastReadData.AsInt32();
				Shared.Player.FriendType key = ((num2 >= 0 && 1 >= num2) ? ((Shared.Player.FriendType)num2) : Shared.Player.FriendType.Invalid);
				unpacker.Read();
				int value = unpacker.LastReadData.AsInt32();
				dictionary.Add(key, value);
			}
			result.Friends = dictionary;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ClanMembers = null;
		}
		else
		{
			int num3 = unpacker.LastReadData.AsInt32();
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>(num3);
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				int key2 = unpacker.LastReadData.AsInt32();
				unpacker.Read();
				int value2 = unpacker.LastReadData.AsInt32();
				dictionary2.Add(key2, value2);
			}
			result.ClanMembers = dictionary2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TakenCounts = null;
		}
		else
		{
			int num4 = unpacker.LastReadData.AsInt32();
			Dictionary<string, int> dictionary3 = new Dictionary<string, int>(num4);
			for (int k = 0; k < num4; k++)
			{
				unpacker.Read();
				string key3 = unpacker.LastReadData.AsString();
				unpacker.Read();
				int value3 = unpacker.LastReadData.AsInt32();
				dictionary3.Add(key3, value3);
			}
			result.TakenCounts = dictionary3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TakenCountsValidUntil = null;
		}
		else
		{
			double value4 = unpacker.LastReadData.AsDouble();
			result.TakenCountsValidUntil = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<InventoryAccess Others={Others} Friends={Friends} ClanMembers={ClanMembers} TakenCounts={TakenCounts} TakenCountsValidUntil={TakenCountsValidUntil}>";
	}
}

using System.Collections.Generic;
using MsgPack;
using Shared.Player;

namespace Messages;

public struct ArtifactAccess
{
	public bool Others;

	public Dictionary<Shared.Player.FriendType, bool> Friends;

	public Dictionary<int, bool> ClanMembers;

	public InventoryAccess? InventoryAccess;

	public static void Pack(Packer packer, ArtifactAccess val, bool hint = false)
	{
		packer.PackArrayHeader(4);
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
			foreach (KeyValuePair<Shared.Player.FriendType, bool> friend in val.Friends)
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
			foreach (KeyValuePair<int, bool> clanMember in val.ClanMembers)
			{
				packer.Pack(clanMember.Key);
				packer.Pack(clanMember.Value);
			}
		}
		if (!val.InventoryAccess.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.InventoryAccess.Pack(packer, val.InventoryAccess.Value);
		}
	}

	public static ArtifactAccess Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactAccess result = default(ArtifactAccess);
		result.Others = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Friends = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Dictionary<Shared.Player.FriendType, bool> dictionary = new Dictionary<Shared.Player.FriendType, bool>(num, default(FriendTypeComparer));
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				int num2 = unpacker.LastReadData.AsInt32();
				Shared.Player.FriendType key = ((num2 >= 0 && 1 >= num2) ? ((Shared.Player.FriendType)num2) : Shared.Player.FriendType.Invalid);
				unpacker.Read();
				bool value = unpacker.LastReadData.AsBoolean();
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
			Dictionary<int, bool> dictionary2 = new Dictionary<int, bool>(num3);
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				int key2 = unpacker.LastReadData.AsInt32();
				unpacker.Read();
				bool value2 = unpacker.LastReadData.AsBoolean();
				dictionary2.Add(key2, value2);
			}
			result.ClanMembers = dictionary2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.InventoryAccess = null;
		}
		else
		{
			InventoryAccess value3 = Messages.InventoryAccess.Unpack(unpacker);
			result.InventoryAccess = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactAccess Others={Others} Friends={Friends} ClanMembers={ClanMembers} InventoryAccess={InventoryAccess}>";
	}
}

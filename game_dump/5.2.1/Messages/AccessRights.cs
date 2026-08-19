using System.Collections.Generic;
using MsgPack;
using Shared.Estate;
using Shared.Player;

namespace Messages;

public struct AccessRights
{
	public Shared.Estate.AccessRights ForOthers;

	public Dictionary<Shared.Player.FriendType, Shared.Estate.AccessRights> ForFriends;

	public Dictionary<int, Shared.Estate.AccessRights> ForClanMembers;

	public static void Pack(Packer packer, AccessRights val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack((int)val.ForOthers);
		if (val.ForFriends == null)
		{
			packer.PackNull();
		}
		else if (val.ForFriends == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ForFriends.Count);
			foreach (KeyValuePair<Shared.Player.FriendType, Shared.Estate.AccessRights> forFriend in val.ForFriends)
			{
				packer.Pack((int)forFriend.Key);
				packer.Pack((int)forFriend.Value);
			}
		}
		if (val.ForClanMembers == null)
		{
			packer.PackNull();
			return;
		}
		if (val.ForClanMembers == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.ForClanMembers.Count);
		foreach (KeyValuePair<int, Shared.Estate.AccessRights> forClanMember in val.ForClanMembers)
		{
			packer.Pack(forClanMember.Key);
			packer.Pack((int)forClanMember.Value);
		}
	}

	public static AccessRights Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AccessRights result = default(AccessRights);
		result.ForOthers = (Shared.Estate.AccessRights)(num & 0x3F);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ForFriends = null;
		}
		else
		{
			int num2 = unpacker.LastReadData.AsInt32();
			Dictionary<Shared.Player.FriendType, Shared.Estate.AccessRights> dictionary = new Dictionary<Shared.Player.FriendType, Shared.Estate.AccessRights>(num2, default(FriendTypeComparer));
			for (int i = 0; i < num2; i++)
			{
				unpacker.Read();
				int num3 = unpacker.LastReadData.AsInt32();
				Shared.Player.FriendType key = ((num3 >= 0 && 1 >= num3) ? ((Shared.Player.FriendType)num3) : Shared.Player.FriendType.Invalid);
				unpacker.Read();
				Shared.Estate.AccessRights value = (Shared.Estate.AccessRights)(unpacker.LastReadData.AsInt32() & 0x3F);
				dictionary.Add(key, value);
			}
			result.ForFriends = dictionary;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ForClanMembers = null;
		}
		else
		{
			int num4 = unpacker.LastReadData.AsInt32();
			Dictionary<int, Shared.Estate.AccessRights> dictionary2 = new Dictionary<int, Shared.Estate.AccessRights>(num4);
			for (int j = 0; j < num4; j++)
			{
				unpacker.Read();
				int key2 = unpacker.LastReadData.AsInt32();
				unpacker.Read();
				Shared.Estate.AccessRights value2 = (Shared.Estate.AccessRights)(unpacker.LastReadData.AsInt32() & 0x3F);
				dictionary2.Add(key2, value2);
			}
			result.ForClanMembers = dictionary2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AccessRights ForOthers={ForOthers} ForFriends={ForFriends} ForClanMembers={ForClanMembers}>";
	}
}

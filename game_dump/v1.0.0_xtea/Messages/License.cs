using System.Collections.Generic;
using MsgPack;
using Shared.Estate;

namespace Messages;

public struct License
{
	public AccessRights ForOthers;

	public AccessRights? ForFriends;

	public Dictionary<int, AccessRights> ForClanMembers;

	public static void Pack(Packer packer, License val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack((int)val.ForOthers);
		if (!val.ForFriends.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.ForFriends.Value);
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
		foreach (KeyValuePair<int, AccessRights> forClanMember in val.ForClanMembers)
		{
			packer.Pack(forClanMember.Key);
			packer.Pack((int)forClanMember.Value);
		}
	}

	public static License Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		License result = default(License);
		result.ForOthers = (AccessRights)(num & 0x3F);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.ForFriends = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			AccessRights value = (AccessRights)(num2 & 0x3F);
			result.ForFriends = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.ForClanMembers = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
			Dictionary<int, AccessRights> dictionary = new Dictionary<int, AccessRights>(num3);
			for (int i = 0; i < num3; i++)
			{
				unpacker.Read();
				MessagePackObject lastReadData6 = unpacker.LastReadData;
				int key = ((MessagePackObject)(ref lastReadData6)).AsInt32();
				unpacker.Read();
				MessagePackObject lastReadData7 = unpacker.LastReadData;
				int num4 = ((MessagePackObject)(ref lastReadData7)).AsInt32();
				AccessRights value2 = (AccessRights)(num4 & 0x3F);
				dictionary.Add(key, value2);
			}
			result.ForClanMembers = dictionary;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<License ForOthers={ForOthers} ForFriends={ForFriends} ForClanMembers={ForClanMembers}>";
	}
}

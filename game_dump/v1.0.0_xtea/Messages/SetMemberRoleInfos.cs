using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct SetMemberRoleInfos
{
	public const uint TypeCode = 3680u;

	public Dictionary<int, MemberRole> Infos;

	public static void Pack(Packer packer, SetMemberRoleInfos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3680u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Infos == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Infos.Count);
		foreach (KeyValuePair<int, MemberRole> info in val.Infos)
		{
			packer.Pack(info.Key);
			MemberRole.Pack(packer, info.Value);
		}
	}

	public static SetMemberRoleInfos Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SetMemberRoleInfos result = default(SetMemberRoleInfos);
		result.Infos = new Dictionary<int, MemberRole>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int key = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			unpacker.Read();
			MemberRole value = MemberRole.Unpack(unpacker);
			result.Infos.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetMemberRoleInfos Infos={Infos}>";
	}
}

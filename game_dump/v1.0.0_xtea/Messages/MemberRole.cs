using MsgPack;
using Shared.Clan;

namespace Messages;

public struct MemberRole
{
	public int Id;

	public string Name;

	public int Grade;

	public Permissions Permissions;

	public static void Pack(Packer packer, MemberRole val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.Id);
		if (val.Name == null)
		{
			packer.PackNull();
		}
		else if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
		packer.Pack(val.Grade);
		packer.Pack((int)val.Permissions);
	}

	public static MemberRole Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		MemberRole result = default(MemberRole);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Name = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string name = ((MessagePackObject)(ref lastReadData3)).AsString();
			result.Name = name;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Grade = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.Permissions = (Permissions)(num & 0xF);
		return result;
	}

	public override string ToString()
	{
		return $"<MemberRole Id={Id} Name={Name} Grade={Grade} Permissions={Permissions}>";
	}
}

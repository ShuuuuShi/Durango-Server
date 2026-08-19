using MsgPack;
using Shared.Clan;

namespace Messages;

public struct MemberRole
{
	public int Id;

	public string Name;

	public int Grade;

	public Permissions Permissions;

	public UserType UserType;

	public static void Pack(Packer packer, MemberRole val, bool hint = false)
	{
		packer.PackArrayHeader(5);
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
		packer.Pack((int)val.UserType);
	}

	public static MemberRole Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MemberRole result = default(MemberRole);
		result.Id = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Name = null;
		}
		else
		{
			string name = unpacker.LastReadData.AsString();
			result.Name = name;
		}
		unpacker.Read();
		result.Grade = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Permissions = (Permissions)(num & 0x1F);
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 1 < num2)
		{
			result.UserType = UserType.Invalid;
		}
		else
		{
			result.UserType = (UserType)num2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<MemberRole Id={Id} Name={Name} Grade={Grade} Permissions={Permissions} UserType={UserType}>";
	}
}

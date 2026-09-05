using MsgPack;

namespace Messages;

public struct RoleOrder
{
	public int RoleId;

	public int Grade;

	public static void Pack(Packer packer, RoleOrder val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.RoleId);
		packer.Pack(val.Grade);
	}

	public static RoleOrder Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RoleOrder result = default(RoleOrder);
		result.RoleId = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Grade = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<RoleOrder RoleId={RoleId} Grade={Grade}>";
	}
}

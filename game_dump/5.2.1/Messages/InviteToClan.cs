using MsgPack;

namespace Messages;

public struct InviteToClan
{
	public const uint TypeCode = 3660u;

	public string EntityId;

	public static void Pack(Packer packer, InviteToClan val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3660u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static InviteToClan Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InviteToClan result = default(InviteToClan);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<InviteToClan EntityId=" + EntityId + ">";
	}
}

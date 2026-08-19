using MsgPack;

namespace Messages;

public struct Secured
{
	public string EntityId;

	public string OwnerId;

	public static void Pack(Packer packer, Secured val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.OwnerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.OwnerId);
		}
	}

	public static Secured Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Secured result = default(Secured);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.OwnerId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<Secured EntityId=" + EntityId + " OwnerId=" + OwnerId + ">";
	}
}

using MsgPack;

namespace Messages;

public struct DropClanApplier
{
	public const uint TypeCode = 3659u;

	public string EntityId;

	public static void Pack(Packer packer, DropClanApplier val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3659u);
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

	public static DropClanApplier Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DropClanApplier result = default(DropClanApplier);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<DropClanApplier EntityId=" + EntityId + ">";
	}
}

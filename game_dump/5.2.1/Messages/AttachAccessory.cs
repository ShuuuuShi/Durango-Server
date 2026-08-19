using MsgPack;

namespace Messages;

public struct AttachAccessory
{
	public const uint TypeCode = 9823459u;

	public string AccessoryId;

	public static void Pack(Packer packer, AttachAccessory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9823459u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.AccessoryId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.AccessoryId);
		}
	}

	public static AttachAccessory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AttachAccessory result = default(AttachAccessory);
		result.AccessoryId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<AttachAccessory AccessoryId=" + AccessoryId + ">";
	}
}

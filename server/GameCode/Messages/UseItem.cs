using MsgPack;

namespace Messages;

public struct UseItem
{
	public const uint TypeCode = 17u;

	public string ItemId;

	public bool Accept;

	public static void Pack(Packer packer, UseItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(17u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		packer.Pack(val.Accept);
	}

	public static UseItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UseItem result = default(UseItem);
		result.ItemId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Accept = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<UseItem ItemId={ItemId} Accept={Accept}>";
	}
}

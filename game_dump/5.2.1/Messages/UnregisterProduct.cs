using MsgPack;

namespace Messages;

public struct UnregisterProduct
{
	public const uint TypeCode = 2070u;

	public string ProductId;

	public static void Pack(Packer packer, UnregisterProduct val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2070u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ProductId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ProductId);
		}
	}

	public static UnregisterProduct Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UnregisterProduct result = default(UnregisterProduct);
		result.ProductId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<UnregisterProduct ProductId=" + ProductId + ">";
	}
}

using MsgPack;

namespace Messages;

public struct WithdrawProduct
{
	public const uint TypeCode = 2085u;

	public string ProductId;

	public static void Pack(Packer packer, WithdrawProduct val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2085u);
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

	public static WithdrawProduct Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WithdrawProduct result = default(WithdrawProduct);
		result.ProductId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<WithdrawProduct ProductId={ProductId}>";
	}
}

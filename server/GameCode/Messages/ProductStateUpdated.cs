using MsgPack;
using Shared.Market;

namespace Messages;

public struct ProductStateUpdated
{
	public const uint TypeCode = 5120u;

	public string ProductId;

	public ProductState State;

	public static void Pack(Packer packer, ProductStateUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5120u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ProductId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ProductId);
		}
		packer.Pack((int)val.State);
	}

	public static ProductStateUpdated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ProductStateUpdated result = default(ProductStateUpdated);
		result.ProductId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 1 || 11 < num)
		{
			result.State = ProductState.Invalid;
		}
		else
		{
			result.State = (ProductState)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ProductStateUpdated ProductId={ProductId} State={State}>";
	}
}

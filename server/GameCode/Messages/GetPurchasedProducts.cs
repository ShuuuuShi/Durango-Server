using MsgPack;

namespace Messages;

public struct GetPurchasedProducts
{
	public const uint TypeCode = 5013u;

	public SortCondition? Sort;

	public int Skip;

	public static void Pack(Packer packer, GetPurchasedProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5013u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (!val.Sort.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			SortCondition.Pack(packer, val.Sort.Value);
		}
		packer.Pack(val.Skip);
	}

	public static GetPurchasedProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetPurchasedProducts result = default(GetPurchasedProducts);
		if (unpacker.LastReadData.IsNil)
		{
			result.Sort = null;
		}
		else
		{
			SortCondition value = SortCondition.Unpack(unpacker);
			result.Sort = value;
		}
		unpacker.Read();
		result.Skip = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<GetPurchasedProducts Sort={Sort} Skip={Skip}>";
	}
}

using MsgPack;

namespace Messages;

public struct GetRegisteredProducts
{
	public const uint TypeCode = 5011u;

	public SortCondition? Sort;

	public int Skip;

	public static void Pack(Packer packer, GetRegisteredProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5011u);
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

	public static GetRegisteredProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetRegisteredProducts result = default(GetRegisteredProducts);
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
		return $"<GetRegisteredProducts Sort={Sort} Skip={Skip}>";
	}
}

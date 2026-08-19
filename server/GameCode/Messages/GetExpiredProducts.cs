using MsgPack;

namespace Messages;

public struct GetExpiredProducts
{
	public const uint TypeCode = 5015u;

	public SortCondition? Sort;

	public int Skip;

	public static void Pack(Packer packer, GetExpiredProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5015u);
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

	public static GetExpiredProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetExpiredProducts result = default(GetExpiredProducts);
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
		return $"<GetExpiredProducts Sort={Sort} Skip={Skip}>";
	}
}

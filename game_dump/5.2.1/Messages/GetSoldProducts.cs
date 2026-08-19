using MsgPack;

namespace Messages;

public struct GetSoldProducts
{
	public const uint TypeCode = 5012u;

	public SortCondition? Sort;

	public int Skip;

	public static void Pack(Packer packer, GetSoldProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5012u);
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

	public static GetSoldProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetSoldProducts result = default(GetSoldProducts);
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
		return $"<GetSoldProducts Sort={Sort} Skip={Skip}>";
	}
}

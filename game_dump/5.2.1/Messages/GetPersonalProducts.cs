using MsgPack;

namespace Messages;

public struct GetPersonalProducts
{
	public SortCondition? Sort;

	public int Skip;

	public static void Pack(Packer packer, GetPersonalProducts val, bool hint = false)
	{
		packer.PackArrayHeader(2);
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

	public static GetPersonalProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetPersonalProducts result = default(GetPersonalProducts);
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
		return $"<GetPersonalProducts Sort={Sort} Skip={Skip}>";
	}
}

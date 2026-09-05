using MsgPack;
using Shared.Market;

namespace Messages;

public struct SortCondition
{
	public ProductSortField Field;

	public bool Ascending;

	public static void Pack(Packer packer, SortCondition val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack((int)val.Field);
		packer.Pack(val.Ascending);
	}

	public static SortCondition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SortCondition result = default(SortCondition);
		if (num < 0 || 6 < num)
		{
			result.Field = ProductSortField.Invalid;
		}
		else
		{
			result.Field = (ProductSortField)num;
		}
		unpacker.Read();
		result.Ascending = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<SortCondition Field={Field} Ascending={Ascending}>";
	}
}

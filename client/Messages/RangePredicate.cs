using MsgPack;

namespace Messages;

public struct RangePredicate
{
	public int? Min;

	public int? Max;

	public static void Pack(Packer packer, RangePredicate val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (!val.Min.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Min.Value);
		}
		if (!val.Max.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Max.Value);
		}
	}

	public static RangePredicate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RangePredicate result = default(RangePredicate);
		if (unpacker.LastReadData.IsNil)
		{
			result.Min = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Min = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Max = null;
		}
		else
		{
			int value2 = unpacker.LastReadData.AsInt32();
			result.Max = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RangePredicate Min={Min} Max={Max}>";
	}
}

using MsgPack;

namespace Messages;

public struct TimeRange
{
	public double? Since;

	public double? Until;

	public static void Pack(Packer packer, TimeRange val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (!val.Since.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Since.Value);
		}
		if (!val.Until.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Until.Value);
		}
	}

	public static TimeRange Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TimeRange result = default(TimeRange);
		if (unpacker.LastReadData.IsNil)
		{
			result.Since = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.Since = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Until = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.Until = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TimeRange Since={Since} Until={Until}>";
	}
}

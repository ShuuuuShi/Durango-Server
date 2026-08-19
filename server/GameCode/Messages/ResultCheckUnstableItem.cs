using MsgPack;

namespace Messages;

public struct ResultCheckUnstableItem
{
	public const uint TypeCode = 5197836u;

	public bool Result;

	public int TotalUnstableCount;

	public static void Pack(Packer packer, ResultCheckUnstableItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5197836u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Result);
		packer.Pack(val.TotalUnstableCount);
	}

	public static ResultCheckUnstableItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ResultCheckUnstableItem result = default(ResultCheckUnstableItem);
		result.Result = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.TotalUnstableCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ResultCheckUnstableItem Result={Result} TotalUnstableCount={TotalUnstableCount}>";
	}
}

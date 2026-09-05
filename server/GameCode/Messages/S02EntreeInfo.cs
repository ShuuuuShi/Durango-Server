using MsgPack;

namespace Messages;

public struct S02EntreeInfo
{
	public const uint TypeCode = 222203u;

	public int QueueCount;

	public double DepartureAt;

	public static void Pack(Packer packer, S02EntreeInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(222203u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.QueueCount);
		packer.Pack(val.DepartureAt);
	}

	public static S02EntreeInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02EntreeInfo result = default(S02EntreeInfo);
		result.QueueCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.DepartureAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<S02EntreeInfo QueueCount={QueueCount} DepartureAt={DepartureAt}>";
	}
}

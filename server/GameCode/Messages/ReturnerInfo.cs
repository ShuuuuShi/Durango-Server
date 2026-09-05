using MsgPack;

namespace Messages;

public struct ReturnerInfo
{
	public const uint TypeCode = 9439833u;

	public bool IsReturner;

	public double Since;

	public double Until;

	public int ReturnerCount;

	public static void Pack(Packer packer, ReturnerInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(9439833u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.IsReturner);
		packer.Pack(val.Since);
		packer.Pack(val.Until);
		packer.Pack(val.ReturnerCount);
	}

	public static ReturnerInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReturnerInfo result = default(ReturnerInfo);
		result.IsReturner = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.Since = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Until = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.ReturnerCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ReturnerInfo IsReturner={IsReturner} Since={Since} Until={Until} ReturnerCount={ReturnerCount}>";
	}
}

using MsgPack;

namespace Messages;

public struct Dead
{
	public const uint TypeCode = 130u;

	public double DeadAt;

	public static void Pack(Packer packer, Dead val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(130u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.DeadAt);
	}

	public static Dead Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Dead result = default(Dead);
		result.DeadAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Dead DeadAt={DeadAt}>";
	}
}

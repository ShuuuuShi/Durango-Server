using MsgPack;

namespace Messages;

public struct LastSearchedTime
{
	public const uint TypeCode = 907u;

	public double SearchedAt;

	public static void Pack(Packer packer, LastSearchedTime val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(907u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.SearchedAt);
	}

	public static LastSearchedTime Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		LastSearchedTime result = default(LastSearchedTime);
		result.SearchedAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<LastSearchedTime SearchedAt={SearchedAt}>";
	}
}

using MsgPack;

namespace Messages;

public struct S02PVPStatus
{
	public const uint TypeCode = 222206u;

	public int RemainSurvivorCount;

	public static void Pack(Packer packer, S02PVPStatus val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(222206u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.RemainSurvivorCount);
	}

	public static S02PVPStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02PVPStatus result = default(S02PVPStatus);
		result.RemainSurvivorCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<S02PVPStatus RemainSurvivorCount={RemainSurvivorCount}>";
	}
}

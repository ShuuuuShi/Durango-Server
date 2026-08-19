using MsgPack;
using Shared.System;

namespace Messages;

public struct Evicted
{
	public const uint TypeCode = 2052u;

	public EvictionReason Reason;

	public static void Pack(Packer packer, Evicted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2052u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Reason);
	}

	public static Evicted Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Evicted result = default(Evicted);
		if (num < 0 || 3 < num)
		{
			result.Reason = EvictionReason.Invalid;
		}
		else
		{
			result.Reason = (EvictionReason)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Evicted Reason={Reason}>";
	}
}

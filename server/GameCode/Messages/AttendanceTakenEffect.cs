using MsgPack;
using Shared.System;

namespace Messages;

public struct AttendanceTakenEffect
{
	public const uint TypeCode = 2084u;

	public Shared.System.RewardEffect Type;

	public static void Pack(Packer packer, AttendanceTakenEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2084u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Type);
	}

	public static AttendanceTakenEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AttendanceTakenEffect result = default(AttendanceTakenEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AttendanceTakenEffect Type={Type}>";
	}
}

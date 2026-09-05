using MsgPack;
using Shared.System;

namespace Messages;

public struct PioneerGradeUpEffect
{
	public const uint TypeCode = 20621u;

	public Shared.System.RewardEffect Type;

	public int Grade;

	public int? EstateSize;

	public static void Pack(Packer packer, PioneerGradeUpEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(20621u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		packer.Pack(val.Grade);
		if (!val.EstateSize.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.EstateSize.Value);
		}
	}

	public static PioneerGradeUpEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		PioneerGradeUpEffect result = default(PioneerGradeUpEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.Grade = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EstateSize = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.EstateSize = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PioneerGradeUpEffect Type={Type} Grade={Grade} EstateSize={EstateSize}>";
	}
}

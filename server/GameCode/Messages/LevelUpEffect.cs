using MsgPack;
using Shared.System;

namespace Messages;

public struct LevelUpEffect
{
	public const uint TypeCode = 2062u;

	public Shared.System.RewardEffect Type;

	public int Level;

	public static void Pack(Packer packer, LevelUpEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2062u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		packer.Pack(val.Level);
	}

	public static LevelUpEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		LevelUpEffect result = default(LevelUpEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<LevelUpEffect Type={Type} Level={Level}>";
	}
}

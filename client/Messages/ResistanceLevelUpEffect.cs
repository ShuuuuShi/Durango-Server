using MsgPack;
using Shared.Ability;
using Shared.System;

namespace Messages;

public struct ResistanceLevelUpEffect
{
	public const uint TypeCode = 20620u;

	public Shared.System.RewardEffect Type;

	public Derived ResistanceType;

	public int Level;

	public static void Pack(Packer packer, ResistanceLevelUpEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(20620u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		packer.Pack((int)val.ResistanceType);
		packer.Pack(val.Level);
	}

	public static ResistanceLevelUpEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ResistanceLevelUpEffect result = default(ResistanceLevelUpEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 322 < num2)
		{
			result.ResistanceType = Derived.Invalid;
		}
		else
		{
			result.ResistanceType = (Derived)num2;
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ResistanceLevelUpEffect Type={Type} ResistanceType={ResistanceType} Level={Level}>";
	}
}

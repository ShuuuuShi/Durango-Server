using MsgPack;
using Shared.Faction;
using Shared.System;

namespace Messages;

public struct FactionLevelUpEffect
{
	public const uint TypeCode = 2068u;

	public Shared.System.RewardEffect Type;

	public FactionType FactionType;

	public string LevelName;

	public static void Pack(Packer packer, FactionLevelUpEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2068u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		packer.Pack((int)val.FactionType);
		packer.PackString(val.LevelName);
	}

	public static FactionLevelUpEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		FactionLevelUpEffect result = default(FactionLevelUpEffect);
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
		if (num2 < 0 || 101 < num2)
		{
			result.FactionType = FactionType.Invalid;
		}
		else
		{
			result.FactionType = (FactionType)num2;
		}
		unpacker.Read();
		result.LevelName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<FactionLevelUpEffect Type={Type} FactionType={FactionType} LevelName={LevelName}>";
	}
}

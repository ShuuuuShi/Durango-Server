using MsgPack;
using Shared.System;

namespace Messages;

public struct FactionLevelUpEffect
{
	public const uint TypeCode = 2068u;

	public Shared.System.RewardEffect Type;

	public string FactionName;

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
		packer.PackString(val.FactionName);
		packer.PackString(val.LevelName);
	}

	public static FactionLevelUpEffect Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		FactionLevelUpEffect result = default(FactionLevelUpEffect);
		if (num < 0 || 9 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.FactionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.LevelName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<FactionLevelUpEffect Type={Type} FactionName={FactionName} LevelName={LevelName}>";
	}
}

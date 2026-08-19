using MsgPack;
using Shared.Faction;
using Shared.System;

namespace Messages;

public struct MissionCompletedEffect
{
	public const uint TypeCode = 2078u;

	public Shared.System.RewardEffect Type;

	public string MissionId;

	public FactionType FactionType;

	public static void Pack(Packer packer, MissionCompletedEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2078u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		if (val.MissionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MissionId);
		}
		packer.Pack((int)val.FactionType);
	}

	public static MissionCompletedEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		MissionCompletedEffect result = default(MissionCompletedEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.MissionId = unpacker.LastReadData.AsString();
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
		return result;
	}

	public override string ToString()
	{
		return $"<MissionCompletedEffect Type={Type} MissionId={MissionId} FactionType={FactionType}>";
	}
}

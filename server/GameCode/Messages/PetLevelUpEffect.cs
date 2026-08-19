using MsgPack;
using Shared.System;

namespace Messages;

public struct PetLevelUpEffect
{
	public const uint TypeCode = 2086u;

	public Shared.System.RewardEffect Type;

	public string PetName;

	public int Level;

	public bool MilestoneAvailable;

	public bool ActiveskillAvailable;

	public static void Pack(Packer packer, PetLevelUpEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2086u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack((int)val.Type);
		packer.PackString(val.PetName);
		packer.Pack(val.Level);
		packer.Pack(val.MilestoneAvailable);
		packer.Pack(val.ActiveskillAvailable);
	}

	public static PetLevelUpEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		PetLevelUpEffect result = default(PetLevelUpEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.PetName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MilestoneAvailable = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.ActiveskillAvailable = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<PetLevelUpEffect Type={Type} PetName={PetName} Level={Level} MilestoneAvailable={MilestoneAvailable} ActiveskillAvailable={ActiveskillAvailable}>";
	}
}

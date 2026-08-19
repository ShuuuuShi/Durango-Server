using MsgPack;
using Shared.System;

namespace Messages;

public struct SkillRewardEffect
{
	public const uint TypeCode = 2063u;

	public Shared.System.RewardEffect Type;

	public Skill LearnedSkill;

	public static void Pack(Packer packer, SkillRewardEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2063u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		Skill.Pack(packer, val.LearnedSkill);
	}

	public static SkillRewardEffect Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SkillRewardEffect result = default(SkillRewardEffect);
		if (num < 0 || 9 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.LearnedSkill = Skill.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SkillRewardEffect Type={Type} LearnedSkill={LearnedSkill}>";
	}
}

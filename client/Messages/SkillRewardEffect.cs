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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SkillRewardEffect result = default(SkillRewardEffect);
		if (num < 0 || 23 < num)
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

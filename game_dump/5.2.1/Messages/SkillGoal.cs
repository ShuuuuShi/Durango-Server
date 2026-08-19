using MsgPack;

namespace Messages;

public struct SkillGoal
{
	public const uint TypeCode = 3512u;

	public Skill Skill;

	public float Progress;

	public static void Pack(Packer packer, SkillGoal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3512u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		Skill.Pack(packer, val.Skill);
		packer.Pack(val.Progress);
	}

	public static SkillGoal Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkillGoal result = default(SkillGoal);
		result.Skill = Skill.Unpack(unpacker);
		unpacker.Read();
		result.Progress = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<SkillGoal Skill={Skill} Progress={Progress}>";
	}
}

using MsgPack;

namespace Messages;

public struct GetSkillAdvisorPoint
{
	public const uint TypeCode = 3903u;

	public Skill Skill;

	public static void Pack(Packer packer, GetSkillAdvisorPoint val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3903u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		Skill.Pack(packer, val.Skill);
	}

	public static GetSkillAdvisorPoint Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetSkillAdvisorPoint result = default(GetSkillAdvisorPoint);
		result.Skill = Skill.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<GetSkillAdvisorPoint Skill={Skill}>";
	}
}

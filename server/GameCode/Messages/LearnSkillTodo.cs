using MsgPack;

namespace Messages;

public struct LearnSkillTodo
{
	public const uint TypeCode = 3522u;

	public Skill Skill;

	public static void Pack(Packer packer, LearnSkillTodo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3522u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		Skill.Pack(packer, val.Skill);
	}

	public static LearnSkillTodo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		LearnSkillTodo result = default(LearnSkillTodo);
		result.Skill = Skill.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<LearnSkillTodo Skill={Skill}>";
	}
}

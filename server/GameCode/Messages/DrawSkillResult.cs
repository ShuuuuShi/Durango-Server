using MsgPack;
using Shared.Economy;

namespace Messages;

public struct DrawSkillResult
{
	public const uint TypeCode = 800103u;

	public PetActiveSkill Skill;

	public Money RetryCost;

	public static void Pack(Packer packer, DrawSkillResult val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(800103u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		PetActiveSkill.Pack(packer, val.Skill);
		packer.PackArrayHeader(2);
		packer.Pack(val.RetryCost.Amount);
		packer.Pack((int)val.RetryCost.Currency);
	}

	public static DrawSkillResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DrawSkillResult result = default(DrawSkillResult);
		result.Skill = PetActiveSkill.Unpack(unpacker);
		unpacker.Read();
		unpacker.ReadInt32(out var result2);
		unpacker.ReadInt32(out var result3);
		result.RetryCost = new Money(result2, (Currency)result3);
		return result;
	}

	public override string ToString()
	{
		return $"<DrawSkillResult Skill={Skill} RetryCost={RetryCost}>";
	}
}

using MsgPack;
using Shared.Skill;

namespace Messages;

public struct SkillCategoryExperienced
{
	public const uint TypeCode = 3644u;

	public Category Category;

	public int Exp;

	public double ResearchReducedTime;

	public static void Pack(Packer packer, SkillCategoryExperienced val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3644u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Category);
		packer.Pack(val.Exp);
		packer.Pack(val.ResearchReducedTime);
	}

	public static SkillCategoryExperienced Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SkillCategoryExperienced result = default(SkillCategoryExperienced);
		if (num < 0 || 15 < num)
		{
			result.Category = Category.Invalid;
		}
		else
		{
			result.Category = (Category)num;
		}
		unpacker.Read();
		result.Exp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ResearchReducedTime = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<SkillCategoryExperienced Category={Category} Exp={Exp} ResearchReducedTime={ResearchReducedTime}>";
	}
}

using MsgPack;
using Shared.Skill;

namespace Messages;

public struct SkipSkillCategoryResearch
{
	public const uint TypeCode = 3643u;

	public Category SkillCategory;

	public static void Pack(Packer packer, SkipSkillCategoryResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3643u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.SkillCategory);
	}

	public static SkipSkillCategoryResearch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SkipSkillCategoryResearch result = default(SkipSkillCategoryResearch);
		if (num < 0 || 15 < num)
		{
			result.SkillCategory = Category.Invalid;
		}
		else
		{
			result.SkillCategory = (Category)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SkipSkillCategoryResearch SkillCategory={SkillCategory}>";
	}
}

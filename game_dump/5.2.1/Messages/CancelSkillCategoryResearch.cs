using MsgPack;
using Shared.Skill;

namespace Messages;

public struct CancelSkillCategoryResearch
{
	public const uint TypeCode = 36431u;

	public Category SkillCategory;

	public static void Pack(Packer packer, CancelSkillCategoryResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(36431u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.SkillCategory);
	}

	public static CancelSkillCategoryResearch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		CancelSkillCategoryResearch result = default(CancelSkillCategoryResearch);
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
		return $"<CancelSkillCategoryResearch SkillCategory={SkillCategory}>";
	}
}

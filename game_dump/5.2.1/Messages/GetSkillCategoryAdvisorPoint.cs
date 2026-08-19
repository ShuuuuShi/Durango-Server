using MsgPack;
using Shared.Skill;

namespace Messages;

public struct GetSkillCategoryAdvisorPoint
{
	public const uint TypeCode = 3904u;

	public Category SkillCategory;

	public static void Pack(Packer packer, GetSkillCategoryAdvisorPoint val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3904u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.SkillCategory);
	}

	public static GetSkillCategoryAdvisorPoint Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetSkillCategoryAdvisorPoint result = default(GetSkillCategoryAdvisorPoint);
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
		return $"<GetSkillCategoryAdvisorPoint SkillCategory={SkillCategory}>";
	}
}

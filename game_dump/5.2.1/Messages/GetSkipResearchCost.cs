using MsgPack;
using Shared.Skill;

namespace Messages;

public struct GetSkipResearchCost
{
	public const uint TypeCode = 3642u;

	public Category SkillCategory;

	public static void Pack(Packer packer, GetSkipResearchCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3642u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.SkillCategory);
	}

	public static GetSkipResearchCost Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetSkipResearchCost result = default(GetSkipResearchCost);
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
		return $"<GetSkipResearchCost SkillCategory={SkillCategory}>";
	}
}

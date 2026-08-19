using MsgPack;
using Shared.Skill;

namespace Messages;

public struct ResearchSkillCategory
{
	public const uint TypeCode = 2446u;

	public Category Category;

	public Category? SkipCategory;

	public static void Pack(Packer packer, ResearchSkillCategory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2446u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Category);
		if (!val.SkipCategory.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.SkipCategory.Value);
		}
	}

	public static ResearchSkillCategory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ResearchSkillCategory result = default(ResearchSkillCategory);
		if (num < 0 || 15 < num)
		{
			result.Category = Category.Invalid;
		}
		else
		{
			result.Category = (Category)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SkipCategory = null;
		}
		else
		{
			int num2 = unpacker.LastReadData.AsInt32();
			Category value = ((num2 >= 0 && 15 >= num2) ? ((Category)num2) : Category.Invalid);
			result.SkipCategory = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ResearchSkillCategory Category={Category} SkipCategory={SkipCategory}>";
	}
}

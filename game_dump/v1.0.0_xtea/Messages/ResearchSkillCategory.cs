using MsgPack;
using Shared.Skill;

namespace Messages;

public struct ResearchSkillCategory
{
	public const uint TypeCode = 2446u;

	public Category Category;

	public Category? SkipCategory;

	public int SkipCost;

	public static void Pack(Packer packer, ResearchSkillCategory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2446u);
		}
		else
		{
			packer.PackArrayHeader(3);
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
		packer.Pack(val.SkipCost);
	}

	public static ResearchSkillCategory Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ResearchSkillCategory result = default(ResearchSkillCategory);
		if (num < 0 || 13 < num)
		{
			result.Category = Category.Invalid;
		}
		else
		{
			result.Category = (Category)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.SkipCategory = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			Category value = ((num2 >= 0 && 13 >= num2) ? ((Category)num2) : Category.Invalid);
			result.SkipCategory = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.SkipCost = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ResearchSkillCategory Category={Category} SkipCategory={SkipCategory} SkipCost={SkipCost}>";
	}
}

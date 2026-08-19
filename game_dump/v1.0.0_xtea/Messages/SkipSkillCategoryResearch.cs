using MsgPack;
using Shared.Skill;

namespace Messages;

public struct SkipSkillCategoryResearch
{
	public const uint TypeCode = 3643u;

	public Category SkillCategory;

	public int Cost;

	public static void Pack(Packer packer, SkipSkillCategoryResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3643u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.SkillCategory);
		packer.Pack(val.Cost);
	}

	public static SkipSkillCategoryResearch Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SkipSkillCategoryResearch result = default(SkipSkillCategoryResearch);
		if (num < 0 || 13 < num)
		{
			result.SkillCategory = Category.Invalid;
		}
		else
		{
			result.SkillCategory = (Category)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Cost = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SkipSkillCategoryResearch SkillCategory={SkillCategory} Cost={Cost}>";
	}
}

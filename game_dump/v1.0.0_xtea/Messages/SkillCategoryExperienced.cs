using MsgPack;
using Shared.Skill;

namespace Messages;

public struct SkillCategoryExperienced
{
	public const uint TypeCode = 3644u;

	public Category Category;

	public int Exp;

	public static void Pack(Packer packer, SkillCategoryExperienced val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3644u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Category);
		packer.Pack(val.Exp);
	}

	public static SkillCategoryExperienced Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SkillCategoryExperienced result = default(SkillCategoryExperienced);
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
		result.Exp = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SkillCategoryExperienced Category={Category} Exp={Exp}>";
	}
}

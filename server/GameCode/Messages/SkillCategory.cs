using MsgPack;

namespace Messages;

public struct SkillCategory
{
	public int Level;

	public int Exp;

	public SkillCategoryResearchTime? ResearchTime;

	public SkillCategoryResearching? Researching;

	public static void Pack(Packer packer, SkillCategory val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.Level);
		packer.Pack(val.Exp);
		if (!val.ResearchTime.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			SkillCategoryResearchTime.Pack(packer, val.ResearchTime.Value);
		}
		if (!val.Researching.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			SkillCategoryResearching.Pack(packer, val.Researching.Value);
		}
	}

	public static SkillCategory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkillCategory result = default(SkillCategory);
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Exp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ResearchTime = null;
		}
		else
		{
			SkillCategoryResearchTime value = SkillCategoryResearchTime.Unpack(unpacker);
			result.ResearchTime = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Researching = null;
		}
		else
		{
			SkillCategoryResearching value2 = SkillCategoryResearching.Unpack(unpacker);
			result.Researching = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SkillCategory Level={Level} Exp={Exp} ResearchTime={ResearchTime} Researching={Researching}>";
	}
}

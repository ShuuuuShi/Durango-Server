using System.Collections.Generic;
using MsgPack;
using Shared.Skill;

namespace Messages;

public struct Skills
{
	public const uint TypeCode = 123u;

	public SkillBundle[] SkillList;

	public int SkillPoint;

	public Dictionary<Category, SkillCategory> Categories;

	public int UntrainedCount;

	public Skill[] AdvisedSkills;

	public Dictionary<Category, int> AdvisedSkillCategories;

	public static void Pack(Packer packer, Skills val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(123u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.SkillList == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.SkillList.Length);
			for (int i = 0; i < val.SkillList.Length; i++)
			{
				SkillBundle.Pack(packer, val.SkillList[i]);
			}
		}
		packer.Pack(val.SkillPoint);
		if (val.Categories == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Categories.Count);
			foreach (KeyValuePair<Category, SkillCategory> category in val.Categories)
			{
				packer.Pack((int)category.Key);
				SkillCategory.Pack(packer, category.Value);
			}
		}
		packer.Pack(val.UntrainedCount);
		if (val.AdvisedSkills == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.AdvisedSkills.Length);
			for (int j = 0; j < val.AdvisedSkills.Length; j++)
			{
				Skill.Pack(packer, val.AdvisedSkills[j]);
			}
		}
		if (val.AdvisedSkillCategories == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.AdvisedSkillCategories.Count);
		foreach (KeyValuePair<Category, int> advisedSkillCategory in val.AdvisedSkillCategories)
		{
			packer.Pack((int)advisedSkillCategory.Key);
			packer.Pack(advisedSkillCategory.Value);
		}
	}

	public static Skills Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Skills result = default(Skills);
		result.SkillList = new SkillBundle[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref SkillBundle reference = ref result.SkillList[i];
			reference = SkillBundle.Unpack(unpacker);
		}
		unpacker.Read();
		result.SkillPoint = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Categories = new Dictionary<Category, SkillCategory>(num2, default(CategoryComparer));
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			Category key = ((num3 >= 0 && 15 >= num3) ? ((Category)num3) : Category.Invalid);
			unpacker.Read();
			SkillCategory value = SkillCategory.Unpack(unpacker);
			result.Categories.Add(key, value);
		}
		unpacker.Read();
		result.UntrainedCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		result.AdvisedSkills = new Skill[num4];
		for (int k = 0; k < num4; k++)
		{
			unpacker.Read();
			ref Skill reference2 = ref result.AdvisedSkills[k];
			reference2 = Skill.Unpack(unpacker);
		}
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.AdvisedSkillCategories = new Dictionary<Category, int>(num5, default(CategoryComparer));
		for (int l = 0; l < num5; l++)
		{
			unpacker.Read();
			int num6 = unpacker.LastReadData.AsInt32();
			Category key2 = ((num6 >= 0 && 15 >= num6) ? ((Category)num6) : Category.Invalid);
			unpacker.Read();
			int value2 = unpacker.LastReadData.AsInt32();
			result.AdvisedSkillCategories.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Skills SkillList={SkillList} SkillPoint={SkillPoint} Categories={Categories} UntrainedCount={UntrainedCount} AdvisedSkills={AdvisedSkills} AdvisedSkillCategories={AdvisedSkillCategories}>";
	}
}

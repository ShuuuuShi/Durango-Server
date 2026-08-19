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

	public bool Untrainable;

	public static void Pack(Packer packer, Skills val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(123u);
		}
		else
		{
			packer.PackArrayHeader(4);
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
		packer.Pack(val.Untrainable);
	}

	public static Skills Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Skills result = default(Skills);
		result.SkillList = new SkillBundle[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref SkillBundle reference = ref result.SkillList[i];
			reference = SkillBundle.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.SkillPoint = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Categories = new Dictionary<Category, SkillCategory>(num2, default(CategoryComparer));
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			Category key = ((num3 >= 0 && 13 >= num3) ? ((Category)num3) : Category.Invalid);
			unpacker.Read();
			SkillCategory value = SkillCategory.Unpack(unpacker);
			result.Categories.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Untrainable = ((MessagePackObject)(ref lastReadData5)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<Skills SkillList={SkillList} SkillPoint={SkillPoint} Categories={Categories} Untrainable={Untrainable}>";
	}
}

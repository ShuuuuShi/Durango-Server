using System.Collections.Generic;
using MsgPack;
using Shared.Skill;

namespace Messages;

public struct SkillBundle
{
	public Category Category;

	public string SkillId;

	public Dictionary<string, int> Levels;

	public static void Pack(Packer packer, SkillBundle val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack((int)val.Category);
		if (val.SkillId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillId);
		}
		if (val.Levels == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Levels.Count);
		foreach (KeyValuePair<string, int> level in val.Levels)
		{
			if (level.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(level.Key);
			}
			packer.Pack(level.Value);
		}
	}

	public static SkillBundle Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SkillBundle result = default(SkillBundle);
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
		result.SkillId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Levels = new Dictionary<string, int>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData4)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData5)).AsInt32();
			result.Levels.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SkillBundle Category={Category} SkillId={SkillId} Levels={Levels}>";
	}
}

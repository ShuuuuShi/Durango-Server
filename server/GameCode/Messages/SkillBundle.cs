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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SkillBundle result = default(SkillBundle);
		if (num < 0 || 15 < num)
		{
			result.Category = Category.Invalid;
		}
		else
		{
			result.Category = (Category)num;
		}
		unpacker.Read();
		result.SkillId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Levels = new Dictionary<string, int>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.Levels.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SkillBundle Category={Category} SkillId={SkillId} Levels={Levels}>";
	}
}

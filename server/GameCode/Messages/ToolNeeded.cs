using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ToolNeeded
{
	public const uint TypeCode = 3647u;

	public string[] RecipeIds;

	public Dictionary<string, Skill> Skills;

	public string TagNames;

	public Dictionary<string, int> Tags;

	public static void Pack(Packer packer, ToolNeeded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3647u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.RecipeIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.RecipeIds.Length);
			for (int i = 0; i < val.RecipeIds.Length; i++)
			{
				if (val.RecipeIds[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.RecipeIds[i]);
				}
			}
		}
		if (val.Skills == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Skills.Count);
			foreach (KeyValuePair<string, Skill> skill in val.Skills)
			{
				if (skill.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(skill.Key);
				}
				Skill.Pack(packer, skill.Value);
			}
		}
		packer.PackString(val.TagNames);
		if (val.Tags == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Tags.Count);
		foreach (KeyValuePair<string, int> tag in val.Tags)
		{
			if (tag.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(tag.Key);
			}
			packer.Pack(tag.Value);
		}
	}

	public static ToolNeeded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ToolNeeded result = default(ToolNeeded);
		result.RecipeIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.RecipeIds[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Skills = new Dictionary<string, Skill>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			Skill value = Skill.Unpack(unpacker);
			result.Skills.Add(key, value);
		}
		unpacker.Read();
		result.TagNames = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.Tags = new Dictionary<string, int>(num3);
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			string key2 = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value2 = unpacker.LastReadData.AsInt32();
			result.Tags.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ToolNeeded RecipeIds={RecipeIds} Skills={Skills} TagNames={TagNames} Tags={Tags}>";
	}
}

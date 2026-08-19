using System.Collections.Generic;
using MsgPack;
using Shared.Item;

namespace Messages;

public struct ReformSlot
{
	public int Index;

	public string RecipeId;

	public Tag[] Tags;

	public string Decorator;

	public Dictionary<string, TagLevelRareness> TagRareness;

	public static void Pack(Packer packer, ReformSlot val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		packer.Pack(val.Index);
		if (val.RecipeId == null)
		{
			packer.PackNull();
		}
		else if (val.RecipeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipeId);
		}
		if (val.Tags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Tags.Length);
			for (int i = 0; i < val.Tags.Length; i++)
			{
				Tag.Pack(packer, val.Tags[i]);
			}
		}
		if (val.Decorator == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.Decorator);
		}
		if (val.TagRareness == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.TagRareness.Count);
		foreach (KeyValuePair<string, TagLevelRareness> item in val.TagRareness)
		{
			if (item.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(item.Key);
			}
			packer.Pack((int)item.Value);
		}
	}

	public static ReformSlot Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReformSlot result = default(ReformSlot);
		result.Index = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RecipeId = null;
		}
		else
		{
			string recipeId = unpacker.LastReadData.AsString();
			result.RecipeId = recipeId;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Tags = new Tag[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Tag reference = ref result.Tags[i];
			reference = Tag.Unpack(unpacker);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Decorator = null;
		}
		else
		{
			string decorator = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.Decorator = decorator;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.TagRareness = new Dictionary<string, TagLevelRareness>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			TagLevelRareness value = ((num3 >= 9 && 12 >= num3) ? ((TagLevelRareness)num3) : TagLevelRareness.Invalid);
			result.TagRareness.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ReformSlot Index={Index} RecipeId={RecipeId} Tags={Tags} Decorator={Decorator} TagRareness={TagRareness}>";
	}
}

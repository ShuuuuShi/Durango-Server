using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct RecommendRecipes
{
	public const uint TypeCode = 3645u;

	public Dictionary<string, int> Tags;

	public static void Pack(Packer packer, RecommendRecipes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3645u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
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

	public static RecommendRecipes Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RecommendRecipes result = default(RecommendRecipes);
		result.Tags = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.Tags.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RecommendRecipes Tags={Tags}>";
	}
}

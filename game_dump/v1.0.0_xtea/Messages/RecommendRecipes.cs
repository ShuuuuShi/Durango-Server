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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		RecommendRecipes result = default(RecommendRecipes);
		result.Tags = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData2)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			result.Tags.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RecommendRecipes Tags={Tags}>";
	}
}

using System.Collections.Generic;
using MsgPack;
using Shared.Rank;

namespace Messages;

public struct S02RewardedRanking
{
	public const uint TypeCode = 222232u;

	public Dictionary<Category, string[]> Rewarded;

	public static void Pack(Packer packer, S02RewardedRanking val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(222232u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Rewarded == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Rewarded.Count);
		foreach (KeyValuePair<Category, string[]> item in val.Rewarded)
		{
			packer.Pack((int)item.Key);
			if (item.Value == null)
			{
				packer.PackArrayHeader(0);
				continue;
			}
			packer.PackArrayHeader(item.Value.Length);
			for (int i = 0; i < item.Value.Length; i++)
			{
				if (item.Value[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(item.Value[i]);
				}
			}
		}
	}

	public static S02RewardedRanking Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		S02RewardedRanking result = default(S02RewardedRanking);
		result.Rewarded = new Dictionary<Category, string[]>(num, default(CategoryComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Category key = ((num2 >= 10 && 78 >= num2) ? ((Category)num2) : Category.Invalid);
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num3];
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				array[j] = unpacker.LastReadData.AsString();
			}
			result.Rewarded.Add(key, array);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<S02RewardedRanking Rewarded={Rewarded}>";
	}
}

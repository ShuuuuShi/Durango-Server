using System.Collections.Generic;
using MsgPack;
using Shared.Skill;
using Shared.System;

namespace Messages;

public struct CategoryLevelUpRewardEffect
{
	public const uint TypeCode = 2064u;

	public Shared.System.RewardEffect Type;

	public Dictionary<Category, int> ChangedLevels;

	public static void Pack(Packer packer, CategoryLevelUpRewardEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2064u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		if (val.ChangedLevels == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.ChangedLevels.Count);
		foreach (KeyValuePair<Category, int> changedLevel in val.ChangedLevels)
		{
			packer.Pack((int)changedLevel.Key);
			packer.Pack(changedLevel.Value);
		}
	}

	public static CategoryLevelUpRewardEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		CategoryLevelUpRewardEffect result = default(CategoryLevelUpRewardEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.ChangedLevels = new Dictionary<Category, int>(num2, default(CategoryComparer));
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			Category key = ((num3 >= 0 && 15 >= num3) ? ((Category)num3) : Category.Invalid);
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.ChangedLevels.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CategoryLevelUpRewardEffect Type={Type} ChangedLevels={ChangedLevels}>";
	}
}

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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		CategoryLevelUpRewardEffect result = default(CategoryLevelUpRewardEffect);
		if (num < 0 || 9 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.ChangedLevels = new Dictionary<Category, int>(num2, default(CategoryComparer));
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			Category key = ((num3 >= 0 && 13 >= num3) ? ((Category)num3) : Category.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			result.ChangedLevels.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CategoryLevelUpRewardEffect Type={Type} ChangedLevels={ChangedLevels}>";
	}
}

using MsgPack;
using Shared.Season2;
using Shared.System;

namespace Messages;

public struct S02SupplyRewardsEffect
{
	public const uint TypeCode = 222212u;

	public Shared.System.RewardEffect Type;

	public ResourceType ResourceType;

	public int Level;

	public bool IsLevelUpReward;

	public int? RewardIndex;

	public static void Pack(Packer packer, S02SupplyRewardsEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(222212u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack((int)val.Type);
		packer.Pack((int)val.ResourceType);
		packer.Pack(val.Level);
		packer.Pack(val.IsLevelUpReward);
		if (!val.RewardIndex.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.RewardIndex.Value);
		}
	}

	public static S02SupplyRewardsEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		S02SupplyRewardsEffect result = default(S02SupplyRewardsEffect);
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
		if (num2 < 0 || 2 < num2)
		{
			result.ResourceType = ResourceType.Invalid;
		}
		else
		{
			result.ResourceType = (ResourceType)num2;
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.IsLevelUpReward = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RewardIndex = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.RewardIndex = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<S02SupplyRewardsEffect Type={Type} ResourceType={ResourceType} Level={Level} IsLevelUpReward={IsLevelUpReward} RewardIndex={RewardIndex}>";
	}
}

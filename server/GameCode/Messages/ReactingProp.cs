using MsgPack;
using Shared.System;

namespace Messages;

public struct ReactingProp
{
	public string EntityId;

	public Interaction Interaction;

	public RequiredItems? RequiredItems;

	public Cost? RequiredMoney;

	public RewardItem[] GivingItems;

	public RewardStatusEffect? RewardStatusEffect;

	public Cooltime? Cooltime;

	public string[] Motions;

	public static void Pack(Packer packer, ReactingProp val, bool hint = false)
	{
		packer.PackArrayHeader(8);
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack((int)val.Interaction);
		if (!val.RequiredItems.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.RequiredItems.Pack(packer, val.RequiredItems.Value);
		}
		if (!val.RequiredMoney.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Cost.Pack(packer, val.RequiredMoney.Value);
		}
		if (val.GivingItems == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.GivingItems.Length);
			for (int i = 0; i < val.GivingItems.Length; i++)
			{
				RewardItem.Pack(packer, val.GivingItems[i]);
			}
		}
		if (!val.RewardStatusEffect.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.RewardStatusEffect.Pack(packer, val.RewardStatusEffect.Value);
		}
		if (!val.Cooltime.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Cooltime.Pack(packer, val.Cooltime.Value);
		}
		if (val.Motions == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Motions.Length);
		for (int j = 0; j < val.Motions.Length; j++)
		{
			if (val.Motions[j] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Motions[j]);
			}
		}
	}

	public static ReactingProp Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReactingProp result = default(ReactingProp);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 10244 < num)
		{
			result.Interaction = Interaction.Invalid;
		}
		else
		{
			result.Interaction = (Interaction)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RequiredItems = null;
		}
		else
		{
			RequiredItems value = Messages.RequiredItems.Unpack(unpacker);
			result.RequiredItems = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RequiredMoney = null;
		}
		else
		{
			Cost value2 = Cost.Unpack(unpacker);
			result.RequiredMoney = value2;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.GivingItems = new RewardItem[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref RewardItem reference = ref result.GivingItems[i];
			reference = RewardItem.Unpack(unpacker);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RewardStatusEffect = null;
		}
		else
		{
			RewardStatusEffect value3 = Messages.RewardStatusEffect.Unpack(unpacker);
			result.RewardStatusEffect = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Cooltime = null;
		}
		else
		{
			Cooltime value4 = Messages.Cooltime.Unpack(unpacker);
			result.Cooltime = value4;
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.Motions = new string[num3];
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			result.Motions[j] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ReactingProp EntityId={EntityId} Interaction={Interaction} RequiredItems={RequiredItems} RequiredMoney={RequiredMoney} GivingItems={GivingItems} RewardStatusEffect={RewardStatusEffect} Cooltime={Cooltime} Motions={Motions}>";
	}
}

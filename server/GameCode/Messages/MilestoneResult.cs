using System.Collections.Generic;
using MsgPack;
using Shared.Ability;
using Shared.Economy;

namespace Messages;

public struct MilestoneResult
{
	public const uint TypeCode = 800013u;

	public string SelectedTagId;

	public Dictionary<Derived, float> OriginalStat;

	public Dictionary<Derived, float> NewStat;

	public string RewardItemId;

	public Money RetryCost;

	public static void Pack(Packer packer, MilestoneResult val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(800013u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.SelectedTagId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SelectedTagId);
		}
		if (val.OriginalStat == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.OriginalStat.Count);
			foreach (KeyValuePair<Derived, float> item in val.OriginalStat)
			{
				packer.Pack((int)item.Key);
				packer.Pack(item.Value);
			}
		}
		if (val.NewStat == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.NewStat.Count);
			foreach (KeyValuePair<Derived, float> item2 in val.NewStat)
			{
				packer.Pack((int)item2.Key);
				packer.Pack(item2.Value);
			}
		}
		if (val.RewardItemId == null)
		{
			packer.PackNull();
		}
		else if (val.RewardItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RewardItemId);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.RetryCost.Amount);
		packer.Pack((int)val.RetryCost.Currency);
	}

	public static MilestoneResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MilestoneResult result = default(MilestoneResult);
		result.SelectedTagId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.OriginalStat = new Dictionary<Derived, float>(num, default(DerivedComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Derived key = ((num2 >= 0 && 322 >= num2) ? ((Derived)num2) : Derived.Invalid);
			unpacker.Read();
			float value = unpacker.LastReadData.AsSingle();
			result.OriginalStat.Add(key, value);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.NewStat = new Dictionary<Derived, float>(num3, default(DerivedComparer));
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			int num4 = unpacker.LastReadData.AsInt32();
			Derived key2 = ((num4 >= 0 && 322 >= num4) ? ((Derived)num4) : Derived.Invalid);
			unpacker.Read();
			float value2 = unpacker.LastReadData.AsSingle();
			result.NewStat.Add(key2, value2);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RewardItemId = null;
		}
		else
		{
			string rewardItemId = unpacker.LastReadData.AsString();
			result.RewardItemId = rewardItemId;
		}
		unpacker.Read();
		unpacker.ReadInt32(out var result2);
		unpacker.ReadInt32(out var result3);
		result.RetryCost = new Money(result2, (Currency)result3);
		return result;
	}

	public override string ToString()
	{
		return $"<MilestoneResult SelectedTagId={SelectedTagId} OriginalStat={OriginalStat} NewStat={NewStat} RewardItemId={RewardItemId} RetryCost={RetryCost}>";
	}
}

using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct PetStats
{
	public const uint TypeCode = 4097198u;

	public float PlaybackRate;

	public int Size;

	public string Taste;

	public int InventoryUsage;

	public Gauge Life;

	public Gauge Hungry;

	public string[] EatableTags;

	public Dictionary<string, int> Tags;

	public bool LastMilestoneAccepted;

	public Money? RetryCost;

	public bool IsOld;

	public double AgingSince;

	public double AgingUntil;

	public double? GrazedAt;

	public static void Pack(Packer packer, PetStats val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(15);
			packer.Pack(4097198u);
		}
		else
		{
			packer.PackArrayHeader(14);
		}
		packer.Pack(val.PlaybackRate);
		packer.Pack(val.Size);
		if (val.Taste == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Taste);
		}
		packer.Pack(val.InventoryUsage);
		Gauge.PackTo(val.Life, packer);
		Gauge.PackTo(val.Hungry, packer);
		if (val.EatableTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.EatableTags.Length);
			for (int i = 0; i < val.EatableTags.Length; i++)
			{
				if (val.EatableTags[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.EatableTags[i]);
				}
			}
		}
		if (val.Tags == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
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
		packer.Pack(val.LastMilestoneAccepted);
		if (!val.RetryCost.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.RetryCost.Value.Amount);
			packer.Pack((int)val.RetryCost.Value.Currency);
		}
		packer.Pack(val.IsOld);
		packer.Pack(val.AgingSince);
		packer.Pack(val.AgingUntil);
		if (!val.GrazedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.GrazedAt.Value);
		}
	}

	public static PetStats Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PetStats result = default(PetStats);
		result.PlaybackRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Size = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Taste = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.InventoryUsage = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Life = Gauge.UnpackFrom(unpacker);
		unpacker.Read();
		result.Hungry = Gauge.UnpackFrom(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.EatableTags = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.EatableTags[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Tags = new Dictionary<string, int>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.Tags.Add(key, value);
		}
		unpacker.Read();
		result.LastMilestoneAccepted = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RetryCost = null;
		}
		else
		{
			unpacker.ReadInt32(out var result2);
			unpacker.ReadInt32(out var result3);
			Money value2 = new Money(result2, (Currency)result3);
			result.RetryCost = value2;
		}
		unpacker.Read();
		result.IsOld = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.AgingSince = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.AgingUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.GrazedAt = null;
		}
		else
		{
			double value3 = unpacker.LastReadData.AsDouble();
			result.GrazedAt = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PetStats PlaybackRate={PlaybackRate} Size={Size} Taste={Taste} InventoryUsage={InventoryUsage} Life={Life} Hungry={Hungry} EatableTags={EatableTags} Tags={Tags} LastMilestoneAccepted={LastMilestoneAccepted} RetryCost={RetryCost} IsOld={IsOld} AgingSince={AgingSince} AgingUntil={AgingUntil} GrazedAt={GrazedAt}>";
	}
}

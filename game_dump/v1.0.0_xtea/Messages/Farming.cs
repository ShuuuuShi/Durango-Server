using System.Collections.Generic;
using MsgPack;
using Shared.Etc;

namespace Messages;

public struct Farming
{
	public string PlantName;

	public double PlantedAt;

	public double GrowsUntil;

	public KeyValuePair<int, int> Water;

	public Fitness BiomeFitness;

	public float Fertilized;

	public int Fertilizer;

	public int SurplusFertilizer;

	public int RequiredFertilizer;

	public Gauge RapidGrowthCost;

	public static void Pack(Packer packer, Farming val, bool hint = false)
	{
		packer.PackArrayHeader(10);
		packer.PackString(val.PlantName);
		packer.Pack(val.PlantedAt);
		packer.Pack(val.GrowsUntil);
		packer.PackArrayHeader(2);
		packer.Pack(val.Water.Key);
		packer.Pack(val.Water.Value);
		packer.Pack((int)val.BiomeFitness);
		packer.Pack(val.Fertilized);
		packer.Pack(val.Fertilizer);
		packer.Pack(val.SurplusFertilizer);
		packer.Pack(val.RequiredFertilizer);
		if (val.RapidGrowthCost == null)
		{
			packer.PackNull();
		}
		else
		{
			Gauge.PackTo(val.RapidGrowthCost, packer);
		}
	}

	public static Farming Unpack(Unpacker unpacker)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		Farming result = default(Farming);
		result.PlantName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.PlantedAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.GrowsUntil = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Water = new KeyValuePair<int, int>(key, value);
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		if (num < 0 || 3 < num)
		{
			result.BiomeFitness = Fitness.Invalid;
		}
		else
		{
			result.BiomeFitness = (Fitness)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Fertilized = ((MessagePackObject)(ref lastReadData6)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.Fertilizer = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		result.SurplusFertilizer = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		result.RequiredFertilizer = ((MessagePackObject)(ref lastReadData9)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData10)).IsNil)
		{
			result.RapidGrowthCost = null;
		}
		else
		{
			Gauge rapidGrowthCost = Gauge.UnpackFrom(unpacker);
			result.RapidGrowthCost = rapidGrowthCost;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Farming PlantName={PlantName} PlantedAt={PlantedAt} GrowsUntil={GrowsUntil} Water={Water} BiomeFitness={BiomeFitness} Fertilized={Fertilized} Fertilizer={Fertilizer} SurplusFertilizer={SurplusFertilizer} RequiredFertilizer={RequiredFertilizer} RapidGrowthCost={RapidGrowthCost}>";
	}
}

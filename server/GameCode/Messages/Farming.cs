using MsgPack;
using Shared.Etc;
using UnityEngine;

namespace Messages;

public struct Farming
{
	public string PlantName;

	public double PlantedAt;

	public double GrowsUntil;

	public Vector2 Water;

	public Fitness BiomeFitness;

	public float FertilizedRatio;

	public float FertilizerAmount;

	public int RequiredFertilizer;

	public string AppliedCropBooster;

	public int BoosterLevel;

	public Gauge RapidGrowthCost;

	public static void Pack(Packer packer, Farming val, bool hint = false)
	{
		packer.PackArrayHeader(11);
		packer.PackString(val.PlantName);
		packer.Pack(val.PlantedAt);
		packer.Pack(val.GrowsUntil);
		packer.PackArrayHeader(2);
		packer.Pack(val.Water.x);
		packer.Pack(val.Water.y);
		packer.Pack((int)val.BiomeFitness);
		packer.Pack(val.FertilizedRatio);
		packer.Pack(val.FertilizerAmount);
		packer.Pack(val.RequiredFertilizer);
		if (val.AppliedCropBooster == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.AppliedCropBooster);
		}
		packer.Pack(val.BoosterLevel);
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
		unpacker.Read();
		Farming result = default(Farming);
		result.PlantName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.PlantedAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.GrowsUntil = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		unpacker.ReadSingle(out result.Water.x);
		unpacker.ReadSingle(out result.Water.y);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.BiomeFitness = Fitness.Invalid;
		}
		else
		{
			result.BiomeFitness = (Fitness)num;
		}
		unpacker.Read();
		result.FertilizedRatio = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.FertilizerAmount = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.RequiredFertilizer = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.AppliedCropBooster = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.BoosterLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
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
		return $"<Farming PlantName={PlantName} PlantedAt={PlantedAt} GrowsUntil={GrowsUntil} Water={Water} BiomeFitness={BiomeFitness} FertilizedRatio={FertilizedRatio} FertilizerAmount={FertilizerAmount} RequiredFertilizer={RequiredFertilizer} AppliedCropBooster={AppliedCropBooster} BoosterLevel={BoosterLevel} RapidGrowthCost={RapidGrowthCost}>";
	}
}

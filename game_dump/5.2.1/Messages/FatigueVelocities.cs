using System.Collections.Generic;
using MsgPack;
using Shared.Survival;

namespace Messages;

public struct FatigueVelocities
{
	public const uint TypeCode = 318u;

	public Dictionary<FatigueCategory, float> Velocities;

	public string FatigueEffect;

	public Dictionary<FatigueCategory, float> Resistances;

	public BiomeFatigue? BiomeFatigue;

	public static void Pack(Packer packer, FatigueVelocities val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(318u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.Velocities == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Velocities.Count);
			foreach (KeyValuePair<FatigueCategory, float> velocity in val.Velocities)
			{
				packer.Pack((int)velocity.Key);
				packer.Pack(velocity.Value);
			}
		}
		if (val.FatigueEffect == null)
		{
			packer.PackNull();
		}
		else if (val.FatigueEffect == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.FatigueEffect);
		}
		if (val.Resistances == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Resistances.Count);
			foreach (KeyValuePair<FatigueCategory, float> resistance in val.Resistances)
			{
				packer.Pack((int)resistance.Key);
				packer.Pack(resistance.Value);
			}
		}
		if (!val.BiomeFatigue.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.BiomeFatigue.Pack(packer, val.BiomeFatigue.Value);
		}
	}

	public static FatigueVelocities Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		FatigueVelocities result = default(FatigueVelocities);
		result.Velocities = new Dictionary<FatigueCategory, float>(num, default(FatigueCategoryComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			FatigueCategory key = ((num2 >= 0 && 11 >= num2) ? ((FatigueCategory)num2) : FatigueCategory.Invalid);
			unpacker.Read();
			float value = unpacker.LastReadData.AsSingle();
			result.Velocities.Add(key, value);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.FatigueEffect = null;
		}
		else
		{
			string fatigueEffect = unpacker.LastReadData.AsString();
			result.FatigueEffect = fatigueEffect;
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.Resistances = new Dictionary<FatigueCategory, float>(num3, default(FatigueCategoryComparer));
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			int num4 = unpacker.LastReadData.AsInt32();
			FatigueCategory key2 = ((num4 >= 0 && 11 >= num4) ? ((FatigueCategory)num4) : FatigueCategory.Invalid);
			unpacker.Read();
			float value2 = unpacker.LastReadData.AsSingle();
			result.Resistances.Add(key2, value2);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BiomeFatigue = null;
		}
		else
		{
			BiomeFatigue value3 = Messages.BiomeFatigue.Unpack(unpacker);
			result.BiomeFatigue = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FatigueVelocities Velocities={Velocities} FatigueEffect={FatigueEffect} Resistances={Resistances} BiomeFatigue={BiomeFatigue}>";
	}
}

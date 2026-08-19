using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct SkillCategoryResearchTime
{
	public float DefaultNeededTime;

	public Dictionary<string, float> ReduceStatusEffects;

	public float ReduceRate;

	public double ReduceUntil;

	public static void Pack(Packer packer, SkillCategoryResearchTime val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.DefaultNeededTime);
		if (val.ReduceStatusEffects == null)
		{
			packer.PackNull();
		}
		else if (val.ReduceStatusEffects == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ReduceStatusEffects.Count);
			foreach (KeyValuePair<string, float> reduceStatusEffect in val.ReduceStatusEffects)
			{
				if (reduceStatusEffect.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(reduceStatusEffect.Key);
				}
				packer.Pack(reduceStatusEffect.Value);
			}
		}
		packer.Pack(val.ReduceRate);
		packer.Pack(val.ReduceUntil);
	}

	public static SkillCategoryResearchTime Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkillCategoryResearchTime result = default(SkillCategoryResearchTime);
		result.DefaultNeededTime = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ReduceStatusEffects = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Dictionary<string, float> dictionary = new Dictionary<string, float>(num);
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				string key = unpacker.LastReadData.AsString();
				unpacker.Read();
				float value = unpacker.LastReadData.AsSingle();
				dictionary.Add(key, value);
			}
			result.ReduceStatusEffects = dictionary;
		}
		unpacker.Read();
		result.ReduceRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.ReduceUntil = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<SkillCategoryResearchTime DefaultNeededTime={DefaultNeededTime} ReduceStatusEffects={ReduceStatusEffects} ReduceRate={ReduceRate} ReduceUntil={ReduceUntil}>";
	}
}

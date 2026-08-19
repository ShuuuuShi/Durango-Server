using System.Collections.Generic;
using MsgPack;
using Shared.Ability;
using Shared.Animal;

namespace Messages;

public struct DomesticationResult
{
	public const uint TypeCode = 694356u;

	public bool Domesticated;

	public PetRank? Rank;

	public int? MilestoneCount;

	public Dictionary<string, int> Tags;

	public Dictionary<Derived, float> Stat;

	public Dictionary<Derived, float> Original;

	public string ReinId;

	public static void Pack(Packer packer, DomesticationResult val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(8);
			packer.Pack(694356u);
		}
		else
		{
			packer.PackArrayHeader(7);
		}
		packer.Pack(val.Domesticated);
		if (!val.Rank.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.Rank.Value);
		}
		if (!val.MilestoneCount.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.MilestoneCount.Value);
		}
		if (val.Tags == null)
		{
			packer.PackNull();
		}
		else if (val.Tags == null)
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
		if (val.Stat == null)
		{
			packer.PackNull();
		}
		else if (val.Stat == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Stat.Count);
			foreach (KeyValuePair<Derived, float> item in val.Stat)
			{
				packer.Pack((int)item.Key);
				packer.Pack(item.Value);
			}
		}
		if (val.Original == null)
		{
			packer.PackNull();
		}
		else if (val.Original == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Original.Count);
			foreach (KeyValuePair<Derived, float> item2 in val.Original)
			{
				packer.Pack((int)item2.Key);
				packer.Pack(item2.Value);
			}
		}
		if (val.ReinId == null)
		{
			packer.PackNull();
		}
		else if (val.ReinId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ReinId);
		}
	}

	public static DomesticationResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DomesticationResult result = default(DomesticationResult);
		result.Domesticated = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Rank = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			PetRank value = ((num >= 10 && 14 >= num) ? ((PetRank)num) : PetRank.Invalid);
			result.Rank = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.MilestoneCount = null;
		}
		else
		{
			int value2 = unpacker.LastReadData.AsInt32();
			result.MilestoneCount = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Tags = null;
		}
		else
		{
			int num2 = unpacker.LastReadData.AsInt32();
			Dictionary<string, int> dictionary = new Dictionary<string, int>(num2);
			for (int i = 0; i < num2; i++)
			{
				unpacker.Read();
				string key = unpacker.LastReadData.AsString();
				unpacker.Read();
				int value3 = unpacker.LastReadData.AsInt32();
				dictionary.Add(key, value3);
			}
			result.Tags = dictionary;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Stat = null;
		}
		else
		{
			int num3 = unpacker.LastReadData.AsInt32();
			Dictionary<Derived, float> dictionary2 = new Dictionary<Derived, float>(num3, default(DerivedComparer));
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				int num4 = unpacker.LastReadData.AsInt32();
				Derived key2 = ((num4 >= 0 && 322 >= num4) ? ((Derived)num4) : Derived.Invalid);
				unpacker.Read();
				float value4 = unpacker.LastReadData.AsSingle();
				dictionary2.Add(key2, value4);
			}
			result.Stat = dictionary2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Original = null;
		}
		else
		{
			int num5 = unpacker.LastReadData.AsInt32();
			Dictionary<Derived, float> dictionary3 = new Dictionary<Derived, float>(num5, default(DerivedComparer));
			for (int k = 0; k < num5; k++)
			{
				unpacker.Read();
				int num6 = unpacker.LastReadData.AsInt32();
				Derived key3 = ((num6 >= 0 && 322 >= num6) ? ((Derived)num6) : Derived.Invalid);
				unpacker.Read();
				float value5 = unpacker.LastReadData.AsSingle();
				dictionary3.Add(key3, value5);
			}
			result.Original = dictionary3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ReinId = null;
		}
		else
		{
			string reinId = unpacker.LastReadData.AsString();
			result.ReinId = reinId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DomesticationResult Domesticated={Domesticated} Rank={Rank} MilestoneCount={MilestoneCount} Tags={Tags} Stat={Stat} Original={Original} ReinId={ReinId}>";
	}
}

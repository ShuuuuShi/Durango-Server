using System.Collections.Generic;
using MsgPack;
using Shared.Ability;

namespace Messages;

public struct Statistics
{
	public const uint TypeCode = 2040u;

	public Dictionary<Basic, int> BasicAbilities;

	public Dictionary<Derived, float> DerivedsAbilities;

	public int Level;

	public int Exp;

	public Dictionary<Derived, int> ResistanceLevels;

	public Dictionary<Derived, int> ResistanceExps;

	public Dictionary<string, float> Modifiers;

	public Dictionary<RepresentType, float> RepresentPowers;

	public static void Pack(Packer packer, Statistics val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(9);
			packer.Pack(2040u);
		}
		else
		{
			packer.PackArrayHeader(8);
		}
		if (val.BasicAbilities == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.BasicAbilities.Count);
			foreach (KeyValuePair<Basic, int> basicAbility in val.BasicAbilities)
			{
				packer.Pack((int)basicAbility.Key);
				packer.Pack(basicAbility.Value);
			}
		}
		if (val.DerivedsAbilities == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.DerivedsAbilities.Count);
			foreach (KeyValuePair<Derived, float> derivedsAbility in val.DerivedsAbilities)
			{
				packer.Pack((int)derivedsAbility.Key);
				packer.Pack(derivedsAbility.Value);
			}
		}
		packer.Pack(val.Level);
		packer.Pack(val.Exp);
		if (val.ResistanceLevels == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ResistanceLevels.Count);
			foreach (KeyValuePair<Derived, int> resistanceLevel in val.ResistanceLevels)
			{
				packer.Pack((int)resistanceLevel.Key);
				packer.Pack(resistanceLevel.Value);
			}
		}
		if (val.ResistanceExps == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ResistanceExps.Count);
			foreach (KeyValuePair<Derived, int> resistanceExp in val.ResistanceExps)
			{
				packer.Pack((int)resistanceExp.Key);
				packer.Pack(resistanceExp.Value);
			}
		}
		if (val.Modifiers == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Modifiers.Count);
			foreach (KeyValuePair<string, float> modifier in val.Modifiers)
			{
				if (modifier.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(modifier.Key);
				}
				packer.Pack(modifier.Value);
			}
		}
		if (val.RepresentPowers == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.RepresentPowers.Count);
		foreach (KeyValuePair<RepresentType, float> representPower in val.RepresentPowers)
		{
			packer.Pack((int)representPower.Key);
			packer.Pack(representPower.Value);
		}
	}

	public static Statistics Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Statistics result = default(Statistics);
		result.BasicAbilities = new Dictionary<Basic, int>(num, default(BasicComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Basic key = ((num2 >= 0 && 7 >= num2) ? ((Basic)num2) : Basic.Invalid);
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.BasicAbilities.Add(key, value);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.DerivedsAbilities = new Dictionary<Derived, float>(num3, default(DerivedComparer));
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			int num4 = unpacker.LastReadData.AsInt32();
			Derived key2 = ((num4 >= 0 && 322 >= num4) ? ((Derived)num4) : Derived.Invalid);
			unpacker.Read();
			float value2 = unpacker.LastReadData.AsSingle();
			result.DerivedsAbilities.Add(key2, value2);
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Exp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.ResistanceLevels = new Dictionary<Derived, int>(num5, default(DerivedComparer));
		for (int k = 0; k < num5; k++)
		{
			unpacker.Read();
			int num6 = unpacker.LastReadData.AsInt32();
			Derived key3 = ((num6 >= 0 && 322 >= num6) ? ((Derived)num6) : Derived.Invalid);
			unpacker.Read();
			int value3 = unpacker.LastReadData.AsInt32();
			result.ResistanceLevels.Add(key3, value3);
		}
		unpacker.Read();
		int num7 = unpacker.LastReadData.AsInt32();
		result.ResistanceExps = new Dictionary<Derived, int>(num7, default(DerivedComparer));
		for (int l = 0; l < num7; l++)
		{
			unpacker.Read();
			int num8 = unpacker.LastReadData.AsInt32();
			Derived key4 = ((num8 >= 0 && 322 >= num8) ? ((Derived)num8) : Derived.Invalid);
			unpacker.Read();
			int value4 = unpacker.LastReadData.AsInt32();
			result.ResistanceExps.Add(key4, value4);
		}
		unpacker.Read();
		int num9 = unpacker.LastReadData.AsInt32();
		result.Modifiers = new Dictionary<string, float>(num9);
		for (int m = 0; m < num9; m++)
		{
			unpacker.Read();
			string key5 = unpacker.LastReadData.AsString();
			unpacker.Read();
			float value5 = unpacker.LastReadData.AsSingle();
			result.Modifiers.Add(key5, value5);
		}
		unpacker.Read();
		int num10 = unpacker.LastReadData.AsInt32();
		result.RepresentPowers = new Dictionary<RepresentType, float>(num10, default(RepresentTypeComparer));
		for (int n = 0; n < num10; n++)
		{
			unpacker.Read();
			int num11 = unpacker.LastReadData.AsInt32();
			RepresentType key6 = ((num11 >= 0 && 2 >= num11) ? ((RepresentType)num11) : RepresentType.Invalid);
			unpacker.Read();
			float value6 = unpacker.LastReadData.AsSingle();
			result.RepresentPowers.Add(key6, value6);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Statistics BasicAbilities={BasicAbilities} DerivedsAbilities={DerivedsAbilities} Level={Level} Exp={Exp} ResistanceLevels={ResistanceLevels} ResistanceExps={ResistanceExps} Modifiers={Modifiers} RepresentPowers={RepresentPowers}>";
	}
}

using System.Collections.Generic;
using MsgPack;
using Shared.Ability;

namespace Messages;

public struct Statistics
{
	public const uint TypeCode = 2040u;

	public Dictionary<Basic, int> BasicAbilities;

	public Dictionary<Derived, int> DerivedsAbilities;

	public int Level;

	public int Exp;

	public Dictionary<string, float> Modifiers;

	public static void Pack(Packer packer, Statistics val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2040u);
		}
		else
		{
			packer.PackArrayHeader(5);
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
			foreach (KeyValuePair<Derived, int> derivedsAbility in val.DerivedsAbilities)
			{
				packer.Pack((int)derivedsAbility.Key);
				packer.Pack(derivedsAbility.Value);
			}
		}
		packer.Pack(val.Level);
		packer.Pack(val.Exp);
		if (val.Modifiers == null)
		{
			packer.PackMapHeader(0);
			return;
		}
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

	public static Statistics Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Statistics result = default(Statistics);
		result.BasicAbilities = new Dictionary<Basic, int>(num, default(BasicComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			Basic key = ((num2 >= 0 && 7 >= num2) ? ((Basic)num2) : Basic.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			result.BasicAbilities.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.DerivedsAbilities = new Dictionary<Derived, int>(num3, default(DerivedComparer));
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			int num4 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
			Derived key2 = ((num4 >= 0 && 301 >= num4) ? ((Derived)num4) : Derived.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int value2 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			result.DerivedsAbilities.Add(key2, value2);
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		result.Exp = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		int num5 = ((MessagePackObject)(ref lastReadData9)).AsInt32();
		result.Modifiers = new Dictionary<string, float>(num5);
		for (int k = 0; k < num5; k++)
		{
			unpacker.Read();
			MessagePackObject lastReadData10 = unpacker.LastReadData;
			string key3 = ((MessagePackObject)(ref lastReadData10)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData11 = unpacker.LastReadData;
			float value3 = ((MessagePackObject)(ref lastReadData11)).AsSingle();
			result.Modifiers.Add(key3, value3);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Statistics BasicAbilities={BasicAbilities} DerivedsAbilities={DerivedsAbilities} Level={Level} Exp={Exp} Modifiers={Modifiers}>";
	}
}

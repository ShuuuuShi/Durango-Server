using System.Collections.Generic;
using MsgPack;
using Shared.Ability;

namespace Messages;

public struct PetStatistics
{
	public const uint TypeCode = 4097197u;

	public int Level;

	public int Exp;

	public int RequiredExp;

	public Dictionary<Derived, float> DerivedAbilities;

	public MilestoneInfo[] MilestonesInformation;

	public PetActiveSkill[] AvailableActiveSkill;

	public static void Pack(Packer packer, PetStatistics val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(4097197u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.Pack(val.Level);
		packer.Pack(val.Exp);
		packer.Pack(val.RequiredExp);
		if (val.DerivedAbilities == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.DerivedAbilities.Count);
			foreach (KeyValuePair<Derived, float> derivedAbility in val.DerivedAbilities)
			{
				packer.Pack((int)derivedAbility.Key);
				packer.Pack(derivedAbility.Value);
			}
		}
		if (val.MilestonesInformation == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.MilestonesInformation.Length);
			for (int i = 0; i < val.MilestonesInformation.Length; i++)
			{
				MilestoneInfo.Pack(packer, val.MilestonesInformation[i]);
			}
		}
		if (val.AvailableActiveSkill == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.AvailableActiveSkill.Length);
		for (int j = 0; j < val.AvailableActiveSkill.Length; j++)
		{
			PetActiveSkill.Pack(packer, val.AvailableActiveSkill[j]);
		}
	}

	public static PetStatistics Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PetStatistics result = default(PetStatistics);
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Exp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.RequiredExp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.DerivedAbilities = new Dictionary<Derived, float>(num, default(DerivedComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Derived key = ((num2 >= 0 && 322 >= num2) ? ((Derived)num2) : Derived.Invalid);
			unpacker.Read();
			float value = unpacker.LastReadData.AsSingle();
			result.DerivedAbilities.Add(key, value);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.MilestonesInformation = new MilestoneInfo[num3];
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			ref MilestoneInfo reference = ref result.MilestonesInformation[j];
			reference = MilestoneInfo.Unpack(unpacker);
		}
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		result.AvailableActiveSkill = new PetActiveSkill[num4];
		for (int k = 0; k < num4; k++)
		{
			unpacker.Read();
			ref PetActiveSkill reference2 = ref result.AvailableActiveSkill[k];
			reference2 = PetActiveSkill.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PetStatistics Level={Level} Exp={Exp} RequiredExp={RequiredExp} DerivedAbilities={DerivedAbilities} MilestonesInformation={MilestonesInformation} AvailableActiveSkill={AvailableActiveSkill}>";
	}
}

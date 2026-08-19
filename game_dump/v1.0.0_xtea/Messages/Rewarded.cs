using System.Collections.Generic;
using MsgPack;
using Shared.Ability;
using Shared.Economy;
using Shared.Faction;

namespace Messages;

public struct Rewarded
{
	public const uint TypeCode = 2065u;

	public object Effect;

	public int Exp;

	public Dictionary<Currency, int> Currency;

	public int SkillPoints;

	public int UsableSkillPoints;

	public Dictionary<Basic, int> Abilities;

	public Skill[] UnlockedSkills;

	public string[] Titles;

	public Dictionary<FactionType, int> FriendshipPoint;

	public static void Pack(Packer packer, Rewarded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(2065u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		if (val.Effect == null)
		{
			packer.PackNull();
		}
		else if (val.Effect is HuntRewardEffect)
		{
			HuntRewardEffect.Pack(packer, (HuntRewardEffect)val.Effect, hint: true);
		}
		else if (val.Effect is LevelUpEffect)
		{
			LevelUpEffect.Pack(packer, (LevelUpEffect)val.Effect, hint: true);
		}
		else if (val.Effect is CategoryLevelUpRewardEffect)
		{
			CategoryLevelUpRewardEffect.Pack(packer, (CategoryLevelUpRewardEffect)val.Effect, hint: true);
		}
		else if (val.Effect is GetTargetTitleEffect)
		{
			GetTargetTitleEffect.Pack(packer, (GetTargetTitleEffect)val.Effect, hint: true);
		}
		else if (val.Effect is FactionEventCompletedEffect)
		{
			FactionEventCompletedEffect.Pack(packer, (FactionEventCompletedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is ExplorePOIEffect)
		{
			ExplorePOIEffect.Pack(packer, (ExplorePOIEffect)val.Effect, hint: true);
		}
		else if (val.Effect is OfferCompletedEffect)
		{
			OfferCompletedEffect.Pack(packer, (OfferCompletedEffect)val.Effect, hint: true);
		}
		else if (val.Effect is FactionLevelUpEffect)
		{
			FactionLevelUpEffect.Pack(packer, (FactionLevelUpEffect)val.Effect, hint: true);
		}
		else if (val.Effect is SkillRewardEffect)
		{
			SkillRewardEffect.Pack(packer, (SkillRewardEffect)val.Effect, hint: true);
		}
		else if (val.Effect is TamingCompletedEffect)
		{
			TamingCompletedEffect.Pack(packer, (TamingCompletedEffect)val.Effect, hint: true);
		}
		packer.Pack(val.Exp);
		if (val.Currency == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Currency.Count);
			foreach (KeyValuePair<Currency, int> item in val.Currency)
			{
				packer.Pack((int)item.Key);
				packer.Pack(item.Value);
			}
		}
		packer.Pack(val.SkillPoints);
		packer.Pack(val.UsableSkillPoints);
		if (val.Abilities == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Abilities.Count);
			foreach (KeyValuePair<Basic, int> ability in val.Abilities)
			{
				packer.Pack((int)ability.Key);
				packer.Pack(ability.Value);
			}
		}
		if (val.UnlockedSkills == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.UnlockedSkills.Length);
			for (int i = 0; i < val.UnlockedSkills.Length; i++)
			{
				Skill.Pack(packer, val.UnlockedSkills[i]);
			}
		}
		if (val.Titles == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Titles.Length);
			for (int j = 0; j < val.Titles.Length; j++)
			{
				if (val.Titles[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Titles[j]);
				}
			}
		}
		if (val.FriendshipPoint == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.FriendshipPoint.Count);
		foreach (KeyValuePair<FactionType, int> item2 in val.FriendshipPoint)
		{
			packer.Pack((int)item2.Key);
			packer.Pack(item2.Value);
		}
	}

	public static Rewarded Unpack(Unpacker unpacker)
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		Rewarded result = default(Rewarded);
		result.Effect = null;
		uint num = default(uint);
		if (unpacker.ReadUInt32(ref num))
		{
			switch (num)
			{
			case 2061u:
				result.Effect = HuntRewardEffect.Unpack(unpacker);
				break;
			case 2062u:
				result.Effect = LevelUpEffect.Unpack(unpacker);
				break;
			case 2064u:
				result.Effect = CategoryLevelUpRewardEffect.Unpack(unpacker);
				break;
			case 2067u:
				result.Effect = GetTargetTitleEffect.Unpack(unpacker);
				break;
			case 2078u:
				result.Effect = FactionEventCompletedEffect.Unpack(unpacker);
				break;
			case 2079u:
				result.Effect = ExplorePOIEffect.Unpack(unpacker);
				break;
			case 2066u:
				result.Effect = OfferCompletedEffect.Unpack(unpacker);
				break;
			case 2068u:
				result.Effect = FactionLevelUpEffect.Unpack(unpacker);
				break;
			case 2063u:
				result.Effect = SkillRewardEffect.Unpack(unpacker);
				break;
			case 812u:
				result.Effect = TamingCompletedEffect.Unpack(unpacker);
				break;
			default:
				Debug.LogError((object)("Unexpected type code: " + num));
				break;
			}
		}
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Exp = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Currency = new Dictionary<Currency, int>(num2, default(CurrencyComparer));
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			Currency key = ((num3 >= 0 && 1 >= num3) ? ((Currency)num3) : Shared.Economy.Currency.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			result.Currency.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.SkillPoints = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.UsableSkillPoints = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		int num4 = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		result.Abilities = new Dictionary<Basic, int>(num4, default(BasicComparer));
		for (int j = 0; j < num4; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData8 = unpacker.LastReadData;
			int num5 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
			Basic key2 = ((num5 >= 0 && 7 >= num5) ? ((Basic)num5) : Basic.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			int value2 = ((MessagePackObject)(ref lastReadData9)).AsInt32();
			result.Abilities.Add(key2, value2);
		}
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		int num6 = ((MessagePackObject)(ref lastReadData10)).AsInt32();
		result.UnlockedSkills = new Skill[num6];
		for (int k = 0; k < num6; k++)
		{
			unpacker.Read();
			ref Skill reference = ref result.UnlockedSkills[k];
			reference = Skill.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		int num7 = ((MessagePackObject)(ref lastReadData11)).AsInt32();
		result.Titles = new string[num7];
		for (int l = 0; l < num7; l++)
		{
			unpacker.Read();
			string[] titles = result.Titles;
			int num8 = l;
			MessagePackObject lastReadData12 = unpacker.LastReadData;
			titles[num8] = ((MessagePackObject)(ref lastReadData12)).AsString();
		}
		unpacker.Read();
		MessagePackObject lastReadData13 = unpacker.LastReadData;
		int num9 = ((MessagePackObject)(ref lastReadData13)).AsInt32();
		result.FriendshipPoint = new Dictionary<FactionType, int>(num9, default(FactionTypeComparer));
		for (int m = 0; m < num9; m++)
		{
			unpacker.Read();
			MessagePackObject lastReadData14 = unpacker.LastReadData;
			int num10 = ((MessagePackObject)(ref lastReadData14)).AsInt32();
			FactionType key3 = ((num10 >= 0 && 4 >= num10) ? ((FactionType)num10) : FactionType.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData15 = unpacker.LastReadData;
			int value3 = ((MessagePackObject)(ref lastReadData15)).AsInt32();
			result.FriendshipPoint.Add(key3, value3);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Rewarded Effect={Effect} Exp={Exp} Currency={Currency} SkillPoints={SkillPoints} UsableSkillPoints={UsableSkillPoints} Abilities={Abilities} UnlockedSkills={UnlockedSkills} Titles={Titles} FriendshipPoint={FriendshipPoint}>";
	}
}

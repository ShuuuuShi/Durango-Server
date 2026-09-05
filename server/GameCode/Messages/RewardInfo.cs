using System.Collections.Generic;
using MsgPack;
using Shared.Ability;
using Shared.Economy;
using Shared.Faction;
using Shared.Memo;

namespace Messages;

public struct RewardInfo
{
	public int? Exp;

	public Dictionary<Currency, long> Currency;

	public int? SkillPoints;

	public int? UsableSkillPoints;

	public Dictionary<Basic, int> Abilities;

	public Dictionary<Derived, float> DerivedAbilities;

	public Skill[] UnlockedSkills;

	public string[] Titles;

	public Dictionary<FactionType, int> FriendshipPoint;

	public RewardItem[] Items;

	public RewardItem[] RandomItems;

	public VoucherInfo[] Vouchers;

	public int? QuestScore;

	public string[] RecipeIds;

	public string[] BlueprintIds;

	public Pair<MemoType, int>[] Memos;

	public static void Pack(Packer packer, RewardInfo val, bool hint = false)
	{
		packer.PackArrayHeader(16);
		if (!val.Exp.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Exp.Value);
		}
		if (val.Currency == null)
		{
			packer.PackNull();
		}
		else if (val.Currency == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Currency.Count);
			foreach (KeyValuePair<Currency, long> item in val.Currency)
			{
				packer.Pack((int)item.Key);
				packer.Pack(item.Value);
			}
		}
		if (!val.SkillPoints.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.SkillPoints.Value);
		}
		if (!val.UsableSkillPoints.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.UsableSkillPoints.Value);
		}
		if (val.Abilities == null)
		{
			packer.PackNull();
		}
		else if (val.Abilities == null)
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
		if (val.DerivedAbilities == null)
		{
			packer.PackNull();
		}
		else if (val.DerivedAbilities == null)
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
		if (val.UnlockedSkills == null)
		{
			packer.PackNull();
		}
		else if (val.UnlockedSkills == null)
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
			packer.PackNull();
		}
		else if (val.Titles == null)
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
			packer.PackNull();
		}
		else if (val.FriendshipPoint == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.FriendshipPoint.Count);
			foreach (KeyValuePair<FactionType, int> item2 in val.FriendshipPoint)
			{
				packer.Pack((int)item2.Key);
				packer.Pack(item2.Value);
			}
		}
		if (val.Items == null)
		{
			packer.PackNull();
		}
		else if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int k = 0; k < val.Items.Length; k++)
			{
				RewardItem.Pack(packer, val.Items[k]);
			}
		}
		if (val.RandomItems == null)
		{
			packer.PackNull();
		}
		else if (val.RandomItems == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.RandomItems.Length);
			for (int l = 0; l < val.RandomItems.Length; l++)
			{
				RewardItem.Pack(packer, val.RandomItems[l]);
			}
		}
		if (val.Vouchers == null)
		{
			packer.PackNull();
		}
		else if (val.Vouchers == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Vouchers.Length);
			for (int m = 0; m < val.Vouchers.Length; m++)
			{
				VoucherInfo.Pack(packer, val.Vouchers[m]);
			}
		}
		if (!val.QuestScore.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.QuestScore.Value);
		}
		if (val.RecipeIds == null)
		{
			packer.PackNull();
		}
		else if (val.RecipeIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.RecipeIds.Length);
			for (int n = 0; n < val.RecipeIds.Length; n++)
			{
				if (val.RecipeIds[n] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.RecipeIds[n]);
				}
			}
		}
		if (val.BlueprintIds == null)
		{
			packer.PackNull();
		}
		else if (val.BlueprintIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.BlueprintIds.Length);
			for (int num = 0; num < val.BlueprintIds.Length; num++)
			{
				if (val.BlueprintIds[num] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.BlueprintIds[num]);
				}
			}
		}
		if (val.Memos == null)
		{
			packer.PackNull();
			return;
		}
		if (val.Memos == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Memos.Length);
		for (int num2 = 0; num2 < val.Memos.Length; num2++)
		{
			packer.PackArrayHeader(2);
			packer.Pack((int)val.Memos[num2].Item1);
			packer.Pack(val.Memos[num2].Item2);
		}
	}

	public static RewardInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RewardInfo result = default(RewardInfo);
		if (unpacker.LastReadData.IsNil)
		{
			result.Exp = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Exp = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Currency = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Dictionary<Currency, long> dictionary = new Dictionary<Currency, long>(num, default(CurrencyComparer));
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				int num2 = unpacker.LastReadData.AsInt32();
				Currency key = ((num2 >= 0 && 7 >= num2) ? ((Currency)num2) : Shared.Economy.Currency.Invalid);
				unpacker.Read();
				long value2 = unpacker.LastReadData.AsInt64();
				dictionary.Add(key, value2);
			}
			result.Currency = dictionary;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SkillPoints = null;
		}
		else
		{
			int value3 = unpacker.LastReadData.AsInt32();
			result.SkillPoints = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.UsableSkillPoints = null;
		}
		else
		{
			int value4 = unpacker.LastReadData.AsInt32();
			result.UsableSkillPoints = value4;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Abilities = null;
		}
		else
		{
			int num3 = unpacker.LastReadData.AsInt32();
			Dictionary<Basic, int> dictionary2 = new Dictionary<Basic, int>(num3, default(BasicComparer));
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				int num4 = unpacker.LastReadData.AsInt32();
				Basic key2 = ((num4 >= 0 && 7 >= num4) ? ((Basic)num4) : Basic.Invalid);
				unpacker.Read();
				int value5 = unpacker.LastReadData.AsInt32();
				dictionary2.Add(key2, value5);
			}
			result.Abilities = dictionary2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.DerivedAbilities = null;
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
				float value6 = unpacker.LastReadData.AsSingle();
				dictionary3.Add(key3, value6);
			}
			result.DerivedAbilities = dictionary3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.UnlockedSkills = null;
		}
		else
		{
			int num7 = unpacker.LastReadData.AsInt32();
			Skill[] array = new Skill[num7];
			for (int l = 0; l < num7; l++)
			{
				unpacker.Read();
				ref Skill reference = ref array[l];
				reference = Skill.Unpack(unpacker);
			}
			result.UnlockedSkills = array;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Titles = null;
		}
		else
		{
			int num8 = unpacker.LastReadData.AsInt32();
			string[] array2 = new string[num8];
			for (int m = 0; m < num8; m++)
			{
				unpacker.Read();
				array2[m] = unpacker.LastReadData.AsString();
			}
			result.Titles = array2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.FriendshipPoint = null;
		}
		else
		{
			int num9 = unpacker.LastReadData.AsInt32();
			Dictionary<FactionType, int> dictionary4 = new Dictionary<FactionType, int>(num9, default(FactionTypeComparer));
			for (int n = 0; n < num9; n++)
			{
				unpacker.Read();
				int num10 = unpacker.LastReadData.AsInt32();
				FactionType key4 = ((num10 >= 0 && 101 >= num10) ? ((FactionType)num10) : FactionType.Invalid);
				unpacker.Read();
				int value7 = unpacker.LastReadData.AsInt32();
				dictionary4.Add(key4, value7);
			}
			result.FriendshipPoint = dictionary4;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Items = null;
		}
		else
		{
			int num11 = unpacker.LastReadData.AsInt32();
			RewardItem[] array3 = new RewardItem[num11];
			for (int num12 = 0; num12 < num11; num12++)
			{
				unpacker.Read();
				ref RewardItem reference2 = ref array3[num12];
				reference2 = RewardItem.Unpack(unpacker);
			}
			result.Items = array3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RandomItems = null;
		}
		else
		{
			int num13 = unpacker.LastReadData.AsInt32();
			RewardItem[] array4 = new RewardItem[num13];
			for (int num14 = 0; num14 < num13; num14++)
			{
				unpacker.Read();
				ref RewardItem reference3 = ref array4[num14];
				reference3 = RewardItem.Unpack(unpacker);
			}
			result.RandomItems = array4;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Vouchers = null;
		}
		else
		{
			int num15 = unpacker.LastReadData.AsInt32();
			VoucherInfo[] array5 = new VoucherInfo[num15];
			for (int num16 = 0; num16 < num15; num16++)
			{
				unpacker.Read();
				ref VoucherInfo reference4 = ref array5[num16];
				reference4 = VoucherInfo.Unpack(unpacker);
			}
			result.Vouchers = array5;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.QuestScore = null;
		}
		else
		{
			int value8 = unpacker.LastReadData.AsInt32();
			result.QuestScore = value8;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RecipeIds = null;
		}
		else
		{
			int num17 = unpacker.LastReadData.AsInt32();
			string[] array6 = new string[num17];
			for (int num18 = 0; num18 < num17; num18++)
			{
				unpacker.Read();
				array6[num18] = unpacker.LastReadData.AsString();
			}
			result.RecipeIds = array6;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BlueprintIds = null;
		}
		else
		{
			int num19 = unpacker.LastReadData.AsInt32();
			string[] array7 = new string[num19];
			for (int num20 = 0; num20 < num19; num20++)
			{
				unpacker.Read();
				array7[num20] = unpacker.LastReadData.AsString();
			}
			result.BlueprintIds = array7;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Memos = null;
		}
		else
		{
			int num21 = unpacker.LastReadData.AsInt32();
			Pair<MemoType, int>[] array8 = new Pair<MemoType, int>[num21];
			for (int num22 = 0; num22 < num21; num22++)
			{
				unpacker.Read();
				unpacker.Read();
				int num23 = unpacker.LastReadData.AsInt32();
				MemoType item = ((num23 >= 0 && 1 >= num23) ? ((MemoType)num23) : MemoType.Invalid);
				unpacker.Read();
				int item2 = unpacker.LastReadData.AsInt32();
				ref Pair<MemoType, int> reference5 = ref array8[num22];
				reference5 = new Pair<MemoType, int>(item, item2);
			}
			result.Memos = array8;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RewardInfo Exp={Exp} Currency={Currency} SkillPoints={SkillPoints} UsableSkillPoints={UsableSkillPoints} Abilities={Abilities} DerivedAbilities={DerivedAbilities} UnlockedSkills={UnlockedSkills} Titles={Titles} FriendshipPoint={FriendshipPoint} Items={Items} RandomItems={RandomItems} Vouchers={Vouchers} QuestScore={QuestScore} RecipeIds={RecipeIds} BlueprintIds={BlueprintIds} Memos={Memos}>";
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Durango.Logic.Item;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Animal;
using Shared.Pet;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public static class PetUtil
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<TagData, string> _003C_003E9__9_2;

		public static Func<string, string> _003C_003E9__9_3;

		public static Func<TagData, string, TagData> _003C_003E9__9_4;

		public static Predicate<ItemData> _003C_003E9__9_1;

		internal string _003CGetAnimalFoodFilter_003Eb__9_2(TagData item)
		{
			return item.Id;
		}

		internal string _003CGetAnimalFoodFilter_003Eb__9_3(string eatable)
		{
			return eatable;
		}

		internal TagData _003CGetAnimalFoodFilter_003Eb__9_4(TagData item, string eatable)
		{
			return item;
		}

		internal bool _003CGetAnimalFoodFilter_003Eb__9_1(ItemData data)
		{
			return data.GetPerformanceData("pet_food").HasValue;
		}
	}

	public const string PetFoodPerformance = "pet_food";

	public static string GetRankedName(string name, PetRank rank)
	{
		return rank switch
		{
			PetRank.S => string.Format("[preset=circle_box?<em>{1}</em>] {0}", name, rank), 
			PetRank.Invalid => name, 
			_ => string.Format("[preset=circle_box?{1}] {0}", name, rank), 
		};
	}

	public static string GetAgingTooltip()
	{
		return T._("모든 동물들은 수명이 정해져있습니다. 수명 한계를 넘으면 노화되어 능력치가 현저히 낮아지며, 생산 및 훈련을 진행할 수 없습니다. 동물 영약을 통해 노화를 되돌릴 수 있습니다.");
	}

	public static string ConverStatusToSrpite(CageStatus domesticationStatus)
	{
		switch (domesticationStatus)
		{
		case CageStatus.Wild:
		case CageStatus.InProgress:
			return "animal_emoticon_angry";
		case CageStatus.Complete:
		case CageStatus.Domesticated:
			return "animal_emoticon_happy";
		default:
			return string.Empty;
		}
	}

	public static float ConvertInfoToRatio(DomesticationInfo info)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (info.Domesticated)
		{
			return 1f;
		}
		if (info.DomesticateUntil <= predictedServerTime)
		{
			return 1f;
		}
		if (info.DomesticationInProgress)
		{
			return Mathf.Clamp01((float)((info.DomesticateUntil - predictedServerTime) / info.TotalTime));
		}
		return 0f;
	}

	public static CageStatus ConverInfoToStatus(DomesticationInfo info)
	{
		if (info.Domesticated)
		{
			return CageStatus.Domesticated;
		}
		if (info.DomesticationInProgress)
		{
			if (Connections.Frontend.GetPredictedServerTime() < info.DomesticateUntil)
			{
				return CageStatus.InProgress;
			}
			return CageStatus.Complete;
		}
		return CageStatus.Wild;
	}

	public static Pair<Color, Color> ConverStatusToGradient(CageStatus domesticationStatus)
	{
		switch (domesticationStatus)
		{
		case CageStatus.Wild:
			return new Pair<Color, Color>(ConverStatusToColor(CageStatus.Wild), ConverStatusToColor(CageStatus.Wild));
		case CageStatus.InProgress:
			return new Pair<Color, Color>(ConverStatusToColor(CageStatus.Complete), ConverStatusToColor(CageStatus.InProgress));
		case CageStatus.Complete:
		case CageStatus.Domesticated:
			return new Pair<Color, Color>(ConverStatusToColor(CageStatus.Complete), ConverStatusToColor(CageStatus.Complete));
		default:
			return new Pair<Color, Color>(Color.white, Color.white);
		}
	}

	public static Color ConverStatusToColor(CageStatus domesticationStatus)
	{
		switch (domesticationStatus)
		{
		case CageStatus.Wild:
			return PresetColor.UIRed;
		case CageStatus.InProgress:
			return PresetColor.UIDarkOrange;
		case CageStatus.Complete:
		case CageStatus.Domesticated:
			return PresetColor.UIYellow;
		default:
			return Color.white;
		}
	}

	public static string ConvertInfoToRemainingTime(DomesticationInfo info, int scope = 2, string granuality = "sec")
	{
		return ConverInfoToStatus(info) switch
		{
			CageStatus.Wild => TimedeltaFormatter.Format(info.TotalTime, scope, granuality), 
			CageStatus.InProgress => TimedeltaFormatter.Format(info.DomesticateUntil - Connections.Frontend.GetPredictedServerTime(), scope, granuality), 
			_ => string.Empty, 
		};
	}

	public static Predicate<ItemData> GetAnimalFoodFilter(string[] eatableTags)
	{
		object obj;
		if (KUtility.GetSize(eatableTags) == 0)
		{
			obj = _003C_003Ec._003C_003E9__9_1;
			if (obj == null)
			{
				return _003C_003Ec._003C_003E9__9_1 = (ItemData data) => data.GetPerformanceData("pet_food").HasValue;
			}
		}
		else
		{
			obj = (Predicate<ItemData>)((ItemData data) => data.GetPerformanceData("pet_food").HasValue && (from item in data.Tags
				join eatable in eatableTags on item.Id equals eatable
				select item).Any());
		}
		return (Predicate<ItemData>)obj;
	}

	public static Predicate<ItemData> GetDomesticationFoodFilter(string[] eatableTags)
	{
		Predicate<ItemData> func = GetAnimalFoodFilter(eatableTags);
		return delegate(ItemData item)
		{
			if (!func(item))
			{
				return false;
			}
			Dictionary<string, string[]> performanceReference = Yaml.Util.Singleton<Constants>.Instance.Pet.PerformanceReference;
			if (performanceReference == null)
			{
				return false;
			}
			List<string> domesticationParameters = Yaml.Util.Singleton<Constants>.Instance.Pet.GetDomesticationParameters();
			if (domesticationParameters == null)
			{
				return false;
			}
			foreach (string item in domesticationParameters)
			{
				if (performanceReference.TryGetValue(item, out var value) && value.Any((string id) => item.GetFloatAttribute(id) > 0f))
				{
					return true;
				}
			}
			return false;
		};
	}

	public static string GetDomesticPetStatusText(CageStatus rein)
	{
		return rein switch
		{
			CageStatus.Wild => T._("야생 상태"), 
			CageStatus.InProgress => T._("길들이는 중"), 
			CageStatus.Domesticated => T._("보관 중"), 
			CageStatus.Complete => T._("길들임 완료"), 
			_ => string.Empty, 
		};
	}

	public static float GetPetFoodEnergy(ItemData item)
	{
		Performance? performanceData = item.GetPerformanceData("pet_food");
		if (performanceData.HasValue && performanceData.Value.Nums.TryGetValue("vigor", out var value))
		{
			return value;
		}
		performanceData = item.GetPerformanceData("food");
		if (performanceData.HasValue && performanceData.Value.Nums.TryGetValue("energy", out var value2))
		{
			return value2;
		}
		return Yaml.Util.Singleton<Constants>.Instance.Pet.DefaultFeedEnergy;
	}

	public static float GetPetFoodRejuvenatingDays(ItemData item)
	{
		return item.GetFloatAttribute("rejuvenating_days");
	}

	public static GrowCage? GetGrowCage(Artifact artifact)
	{
		if (artifact == null)
		{
			return null;
		}
		object cage = artifact.ArtifactState.Cage;
		if (cage is GrowCage)
		{
			return (GrowCage)cage;
		}
		return null;
	}

	public static Messages.Cage? GetCage(Artifact artifact)
	{
		if (artifact == null)
		{
			return null;
		}
		object cage = artifact.ArtifactState.Cage;
		if (cage is Messages.Cage)
		{
			return (Messages.Cage)cage;
		}
		return null;
	}

	public static int GetPetMilestoneDiffLevel(float diff)
	{
		if (diff > 0.15f)
		{
			return 3;
		}
		if (diff > 0.1f)
		{
			return 2;
		}
		if (diff > 0f)
		{
			return 1;
		}
		return 0;
	}

	public static string GetPetInfoString(Messages.Pet pet)
	{
		int exp = pet.Statistics.Exp;
		int requiredExp = pet.Statistics.RequiredExp;
		Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
		string text = ((pet2 != null) ? PetTasteToString(pet2.Type) : null);
		if (requiredExp > 0)
		{
			return $"{LocalizeUtil.FormatLevel(pet.Statistics.Level)} <weak>({exp}/{requiredExp})</weak>  <bar/>  {text}";
		}
		return LocalizeUtil.FormatLevel(pet.Statistics.Level) + "  <bar/>  " + text;
	}

	public static string PetTasteToString(string taste)
	{
		return LocalizeSystem.Get("#pet_taste_" + taste);
	}

	public static List<Pair<Messages.PetActiveSkill, float>> GetActiveSkillCandidates(Messages.Pet pet)
	{
		List<Pair<Messages.PetActiveSkill, float>> list = new List<Pair<Messages.PetActiveSkill, float>>();
		Yaml.Pet pet2 = SingletonDict<int, Yaml.Pet>.Get(pet.EntityType);
		if (pet2 == null)
		{
			return list;
		}
		foreach (KeyValuePair<string, PetActiveSkillConditionDict> item in SingletonDict<string, PetActiveSkillConditionDict>.Instance)
		{
			string key = item.Key;
			foreach (KeyValuePair<SkillRank, PetActiveSkillCondition> item2 in item.Value)
			{
				SkillRank key2 = item2.Key;
				PetActiveSkillCondition value = item2.Value;
				if ((value.ForRidable && !pet2.IsRidable) || (value.ForFightable && !pet2.IsFightable) || (KUtility.GetSize(value.EntityType) > 0 && value.EntityType.IndexOf(pet.EntityType) == -1))
				{
					continue;
				}
				if (KUtility.GetSize(value.TagCondition) > 0)
				{
					if (pet.Stat.Tags == null)
					{
						continue;
					}
					bool flag = false;
					foreach (KeyValuePair<string, int> item3 in value.TagCondition)
					{
						if (pet.Stat.Tags.TryGetValue(item3.Key, out var value2) && item3.Value <= value2)
						{
							flag = true;
							continue;
						}
						break;
					}
					if (!flag)
					{
						continue;
					}
				}
				list.Add(new Pair<Messages.PetActiveSkill, float>(new Messages.PetActiveSkill
				{
					SkillId = key,
					Rank = key2
				}, item2.Value.Weight));
			}
		}
		list.Sort(ActiveSkillCandidateComparison);
		return list;
	}

	public static void FindLearnableSkills([NotNull] List<Messages.PetActiveSkill> result, int petType, bool includeNonConditionSkill = false)
	{
		Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(petType);
		if (pet == null)
		{
			return;
		}
		foreach (KeyValuePair<string, PetActiveSkillConditionDict> item in SingletonDict<string, PetActiveSkillConditionDict>.Instance)
		{
			string key = item.Key;
			foreach (KeyValuePair<SkillRank, PetActiveSkillCondition> item2 in item.Value)
			{
				SkillRank key2 = item2.Key;
				PetActiveSkillCondition value = item2.Value;
				if ((value.ForRidable && !pet.IsRidable) || (value.ForFightable && !pet.IsFightable))
				{
					continue;
				}
				if (KUtility.GetSize(value.EntityType) > 0)
				{
					bool flag = false;
					int[] entityType = value.EntityType;
					foreach (int num in entityType)
					{
						if (petType == num)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
				else if (!includeNonConditionSkill)
				{
					continue;
				}
				result.Add(new Messages.PetActiveSkill
				{
					SkillId = key,
					Rank = key2
				});
			}
		}
	}

	public static int ActiveSkillCandidateComparison(Pair<Messages.PetActiveSkill, float> p1, Pair<Messages.PetActiveSkill, float> p2)
	{
		int num = (int)((!string.IsNullOrEmpty(p1.Item1.SkillId)) ? p1.Item1.Rank : ((SkillRank)10000));
		int num2 = (int)((!string.IsNullOrEmpty(p2.Item1.SkillId)) ? p2.Item1.Rank : ((SkillRank)10000));
		if (num != num2)
		{
			return num - num2;
		}
		if (p1.Item2 > p2.Item2)
		{
			return -1;
		}
		return 1;
	}

	public static int TagCandidateComparison(Pair<string, float> p1, Pair<string, float> p2)
	{
		int tagCandidateSortPriority = GetTagCandidateSortPriority(p1.Item1);
		int tagCandidateSortPriority2 = GetTagCandidateSortPriority(p2.Item1);
		if (tagCandidateSortPriority != tagCandidateSortPriority2)
		{
			return tagCandidateSortPriority - tagCandidateSortPriority2;
		}
		if (p1.Item2 > p2.Item2)
		{
			return -1;
		}
		return 1;
	}

	private static int GetTagCandidateSortPriority(string tagId)
	{
		if (string.IsNullOrEmpty(tagId))
		{
			return 10000;
		}
		if (SingletonDict<string, Yaml.Tag>.TryGetValue(tagId, out var value))
		{
			return (int)value.Grade;
		}
		return -1;
	}

	public static Yaml.PetActiveSkill GetPlayerPetSkill(string id)
	{
		PetManager petManager = Durango.Utils.Singleton<PetManager>.Instance();
		Messages.Pet? pet = petManager.GetPet(petManager.GetPlayerPetId());
		if (!pet.HasValue)
		{
			return null;
		}
		Messages.PetActiveSkill? petActiveSkill = null;
		Messages.PetActiveSkill[] availableActiveSkill = pet.Value.Statistics.AvailableActiveSkill;
		for (int i = 0; i < availableActiveSkill.Length; i++)
		{
			Messages.PetActiveSkill value = availableActiveSkill[i];
			if (value.SkillId == id)
			{
				petActiveSkill = value;
				break;
			}
		}
		if (!petActiveSkill.HasValue)
		{
			return null;
		}
		return PetActiveSkills.Get(petActiveSkill.Value.SkillId, petActiveSkill.Value.Rank);
	}

	public static MilestoneInfo? GetCurrentPetMilestoneInfo(Messages.Pet pet)
	{
		int currentPetMilestoneIndex = GetCurrentPetMilestoneIndex(pet);
		if (currentPetMilestoneIndex == -1)
		{
			return null;
		}
		return pet.Statistics.MilestonesInformation[currentPetMilestoneIndex];
	}

	public static int GetCurrentPetMilestoneIndex(Messages.Pet pet)
	{
		if (pet.Statistics.MilestonesInformation == null)
		{
			return -1;
		}
		for (int num = KUtility.GetSize(pet.Statistics.MilestonesInformation) - 1; num >= 0; num--)
		{
			if (pet.Statistics.MilestonesInformation[num].Acquired)
			{
				if (pet.Stat.LastMilestoneAccepted)
				{
					return Math.Min(num + 1, KUtility.GetSize(pet.Statistics.MilestonesInformation) - 1);
				}
				return num;
			}
		}
		return 0;
	}

	public static int GetLatestAcquiredPetMilestoneIndex(Messages.Pet pet)
	{
		if (pet.Statistics.MilestonesInformation == null)
		{
			return -1;
		}
		for (int num = KUtility.GetSize(pet.Statistics.MilestonesInformation) - 1; num >= 0; num--)
		{
			if (pet.Statistics.MilestonesInformation[num].Acquired)
			{
				return num;
			}
		}
		return -1;
	}

	public static bool IsPetRemainMilestone(Messages.Pet pet)
	{
		int currentPetMilestoneIndex = GetCurrentPetMilestoneIndex(pet);
		if (currentPetMilestoneIndex == -1)
		{
			return false;
		}
		int num = 0;
		num = KUtility.GetSize(pet.Statistics.MilestonesInformation) - 1;
		while (num >= 0 && !pet.Statistics.MilestonesInformation[num].Acquired)
		{
			num--;
		}
		return currentPetMilestoneIndex != num;
	}

	public static bool PetReadyToDrawActiveSkill(Messages.Pet pet)
	{
		if (PetReadyToActiveSkill(pet))
		{
			return !HasPetActiveSkill(pet);
		}
		return false;
	}

	public static bool PetReadyToActiveSkill(Messages.Pet pet)
	{
		if (pet.Statistics.MilestonesInformation == null)
		{
			return false;
		}
		if (pet.Statistics.MilestonesInformation.LastOrDefault().Acquired)
		{
			return pet.Stat.LastMilestoneAccepted;
		}
		return false;
	}

	public static bool HasPetActiveSkill(Messages.Pet pet)
	{
		return KUtility.GetSize(pet.Statistics.AvailableActiveSkill) > 0;
	}

	public static bool HasAcquiredMilestone(Messages.Pet pet)
	{
		if (pet.Statistics.MilestonesInformation == null)
		{
			return false;
		}
		MilestoneInfo[] milestonesInformation = pet.Statistics.MilestonesInformation;
		for (int i = 0; i < KUtility.GetSize(milestonesInformation); i++)
		{
			if (milestonesInformation[i].Acquired)
			{
				return true;
			}
		}
		return false;
	}
}

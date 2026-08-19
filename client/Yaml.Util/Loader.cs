using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using Durango.Logic;
using Durango.Logic.Clusters;
using Durango.Terrain;
using Durango.Utils;
using Shared.Ability;
using UnityEngine;

namespace Yaml.Util;

public static class Loader
{
	public enum State
	{
		None,
		Loading,
		Succees,
		Failure
	}

	private static MonoBehaviour _routineParent;

	private static Coroutine _loadingRoutine;

	public static State LoadState { get; private set; }

	public static bool Cached { get; set; }

	public static string Error { get; private set; }

	static Loader()
	{
		GameManager.Reset += Stop;
	}

	public static void Load(MonoBehaviour parent)
	{
		_routineParent = parent;
		_loadingRoutine = parent.StartCoroutine(CoLoadingYmls());
	}

	public static void Stop()
	{
		LoadState = State.None;
		if (_routineParent != null && _loadingRoutine != null)
		{
			_routineParent.StopCoroutine(_loadingRoutine);
		}
		_routineParent = null;
		_loadingRoutine = null;
	}

	private static IEnumerator CoLoadingYmls()
	{
		LoadState = State.Loading;
		Error = string.Empty;
		IEnumerator[] routines = new IEnumerator[71]
		{
			LoadYaml<PlayerActions>("/assets/player/player_battle_actions"),
			LoadYaml<TagAllowActions>("/assets/tag_allow_actions"),
			LoadYaml<TagYaml>("/assets/tags"),
			LoadYaml<RecipeDict>("/assets/item/recipes"),
			LoadYaml<GeneratorYaml>("/assets/item/generator_client_data"),
			LoadYaml<CollectibleYaml>("/assets/item/collectible_names"),
			LoadYaml<BlueprintDict>("/assets/building/blueprints"),
			LoadYaml<ArtifactPrototypeDict>("/assets/entity_types/artifact"),
			LoadYaml<BlueprintRemodelingsDict>("/assets/building/blueprint_remodelings"),
			LoadYaml<ArtifactModelDict>("/assets/building/artifact_models"),
			LoadYaml<ArtifactEffectDict>("/assets/building/artifact_effects"),
			LoadYaml<Pets>("/assets/pet/pets_for_client"),
			LoadYaml<PetActiveSkills>("/assets/pet/pet_active_skills"),
			LoadYaml<PetActiveSkillConditions>("/assets/pet/pet_active_skill_conditions"),
			LoadYaml<PetExp>("/assets/pet/pet_exp"),
			LoadYaml<PetTasks>("/assets/pet/pet_task"),
			LoadYaml<Dictionary<int, Natural>>("/assets/entity_types/natural", DataHelper.Initialize, cacheable: true),
			LoadYaml<AnimalYaml>("/assets/entity_types/animal"),
			LoadYaml<ArtifactSetEffectsYaml>("/assets/building/artifact_set_effects"),
			LoadYaml<TitleYaml>("/assets/titles", GameSystem<StatisticsSystem>.Instance().InitTitles),
			LoadYaml<AdviceYaml>("/assets/advices", GameSystem<StatisticsSystem>.Instance().InitAdvices),
			LoadYaml<AdviceCategoriesYaml>("/assets/advice_categories", GameSystem<StatisticsSystem>.Instance().InitAdviceCategories),
			LoadYaml<PrototypeYaml>("/assets/item/prototype_data"),
			LoadYaml<RegionTemplateDict>("/assets/region_templates"),
			LoadYaml<PerformanceVisibleInfoDict>("/assets/performance_visible_infos"),
			LoadYaml<SkillModifierYaml>("/assets/skill/modifiers"),
			LoadYaml<EncyclopediaModifiersYaml>("/assets/encyclopedia/encyclopedia_modifiers"),
			LoadYaml<StatusEffectTemplateYaml>("/assets/survival/status_effects"),
			LoadYaml<FatigueCategoryYaml>("/assets/survival/fatigue_categories"),
			LoadYaml<Constants>("/assets/constants"),
			LoadYaml<CostsYaml>("/assets/costs"),
			LoadYaml<PlayerStatistics>("/assets/statistics/player"),
			LoadYaml<PlayerEntities>("/assets/entity_types/players"),
			LoadYaml<SkillYaml>("/assets/skill/skills", GameSystem<SkillSystem>.Instance().InitSkillList),
			LoadYaml<RewardYaml>("/assets/skill/rewards", GameSystem<SkillSystem>.Instance().InitSkillRewards),
			LoadYaml<SkillCategoryYaml>("/assets/skill/categories"),
			LoadYaml<MemosYaml>("/assets/memos"),
			LoadYaml<CashYaml>("/assets/cash"),
			LoadYaml<DateTimeDict>("/assets/survival/date_time"),
			LoadYaml<Factions>("/assets/factions"),
			LoadYaml<TalksYaml>("/assets/faction_talks"),
			LoadYaml<MessengersYaml>("/assets/faction_messenger_jobs"),
			LoadYaml<JobsYaml>("/assets/player/jobs"),
			LoadYaml<ClanResearchs>("/assets/clan_research"),
			LoadYaml<PersonalResearchs>("/assets/personal_research"),
			LoadYaml<ClanYaml>("/assets/clan"),
			LoadYaml<TimelineMessagesYaml>("/assets/timeline_messages"),
			LoadYaml<Commodities>("/assets/purchaser/commodities"),
			LoadYaml<ShopCategories>("/assets/purchaser/shop_ui_categories"),
			LoadYaml<Vouchers>("/assets/purchaser/vouchers"),
			LoadYaml<Emotions>("/assets/emotions", GameSystem<SocialSystem>.Instance().Emotional.Init),
			LoadYaml<Accessories>("/assets/accessories"),
			LoadYaml<QuestsYml>("/assets/quests/quests_for_client"),
			LoadYaml<WarpRushRewards>("/assets/season/season2_rewards_client"),
			LoadYaml<ArchipelagoTemplateDict>("/assets/archipelago_templates"),
			LoadYaml<ArchipelagoMissionDict>("/assets/quests/archipelago_todos_client"),
			LoadYaml<RegionCoOpDict>("/assets/quests/region_co_op_todos_client"),
			LoadYaml<UnstableFactorDict>("/assets/unstable_factors_client"),
			LoadYaml<ReformTechSupportDict>("/assets/item/tech_support"),
			LoadYaml<Pioneer>("/assets/pioneer"),
			LoadYaml<PioneerGradeRewards>("/assets/pioneer_grade_rewards"),
			LoadYaml<EncyclopediaCategories>("/assets/encyclopedia/encyclopedia_categories"),
			LoadYaml<EncyclopediaItems>("/assets/encyclopedia/encyclopedia_items"),
			LoadYaml<CropData>("/assets/crop_data"),
			LoadYaml<BonusPrototypeYaml>("/assets/item/bonus_prototypes"),
			LoadYaml<DerivedRewards>("/assets/crafting_rewards"),
			LoadYaml<Dictionary<Derived, Dictionary<int, DerivedRewardData>>>("/assets/crafting_rewards_datas", DerivedRewardDatas.Set, cacheable: true),
			LoadYaml<SpecialDealBannersDict>("/assets/purchaser/special_deals"),
			LoadYaml<Rankings>("/assets/ranking"),
			LoadYaml<RankingRewards>("/assets/ranking_rewards"),
			LoadYaml<StoryYaml>("/assets/quests/epics_for_client")
		};
		while (true)
		{
			bool allFinished = true;
			for (int i = 0; i < routines.Length; i++)
			{
				allFinished &= !routines[i].MoveNext();
			}
			if (allFinished)
			{
				break;
			}
			yield return null;
		}
		LoadState = ((!string.IsNullOrEmpty(Error)) ? State.Failure : State.Succees);
		Cached = LoadState == State.Succees;
		if (LoadState == State.Succees)
		{
			Durango.Utils.Singleton<GameManager>.Instance().NotifyYamlLoaded();
		}
	}

	private static IEnumerator LoadYaml<T>(string postFix, Action<T> func = null, bool cacheable = false) where T : class
	{
		string url = GameManager.GatewayUrl + postFix;
		int retryCount = 0;
		bool disableCache = false;
		T yamlData = (T)null;
		while (yamlData == null && retryCount < 5)
		{
			bool isCached;
			if (GameManager.ClusterMode == Mode.Online)
			{
				HTTPRequest request = Http.Request(url, null, disableCache);
				while (request.MoveNext())
				{
					yield return null;
				}
				byte[] bytes = Http.ProcessResult(request, out isCached);
				if (isCached && Cached && (func == null || cacheable))
				{
					yield break;
				}
				yamlData = Json.Read<T>(bytes);
			}
			else
			{
				if (Cached && (func == null || cacheable))
				{
					yield break;
				}
				isCached = true;
				yamlData = Json.ReadFromFile<T>("offline" + postFix);
			}
			retryCount++;
			disableCache = true;
			if (isCached)
			{
			}
		}
		if (yamlData != null)
		{
			if (func != null)
			{
				try
				{
					func(yamlData);
					yield break;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					Error = ex.Message;
					yield break;
				}
			}
			if (yamlData is ISingletonable singletonable)
			{
				singletonable.Initialize(yamlData);
			}
			else
			{
				Debug.LogError("No function provided for initializing - " + typeof(T));
			}
		}
		else
		{
			Error = url;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;

namespace Yaml.Util;

public class Loader : MonoBehaviour
{
	public static bool LoadSucceed { get; set; }

	public bool IsFinished { get; private set; }

	public string Error { get; private set; }

	public void Load()
	{
		((MonoBehaviour)this).StartCoroutine(CoLoadingYmls());
	}

	public void Stop()
	{
		IsFinished = false;
		((MonoBehaviour)this).StopAllCoroutines();
	}

	private IEnumerator CoLoadingYmls()
	{
		IsFinished = false;
		Error = string.Empty;
		IEnumerator[] routines = new IEnumerator[32]
		{
			LoadYaml<ActionSetYaml>("assets/player/player_action_set"),
			LoadYaml<TagYaml>("assets/tags"),
			LoadYaml<RecipeYaml>("assets/item/recipes", GameSystem<RecipeSystem>.Instance().SetRecipes),
			LoadYaml<BlueprintYaml>("assets/building/blueprints", GameSystem<RecipeSystem>.Instance().SetBlueprints),
			LoadYaml<Dictionary<int, ArtifactPrototype>>("entity_types?category=artifact", GameSystem<RecipeSystem>.Instance().SetArtifactPrototypes),
			LoadYaml<ArtifactModelDict>("assets/building/artifact_models"),
			LoadYaml<ArtifactEffectDict>("assets/building/artifact_effects"),
			LoadYaml<Dictionary<int, Natural>>("entity_types?category=natural", TerrainDataHelper.Initialize, cacheable: true),
			LoadYaml<CollectibleNames>("assets/item/collectible_names"),
			LoadYaml<AnimalYaml>("entity_types?category=animal"),
			LoadYaml<TitleYaml>("assets/titles", GameSystem<StatisticsSystem>.Instance().InitTitles),
			LoadYaml<PrototypeYaml>("assets/item/prototype_data"),
			LoadYaml<RegionTemplateDict>("assets/region_templates"),
			LoadYaml<PerformanceVisibleInfoDict>("assets/performance_visible_infos"),
			LoadYaml<SkillModifierYaml>("assets/skill/modifiers"),
			LoadYaml<StatusEffectTemplateYaml>("assets/survival/status_effects"),
			LoadYaml<FatigueCategoryYaml>("assets/survival/fatigue_categories"),
			LoadYaml<Constants>("assets/constants"),
			LoadYaml<PlayerStatistics>("assets/statistics/player"),
			LoadYaml<Dictionary<int, int[]>>("assets/estate/interactionrights", GameSystem<EstateSystem>.Instance().InitInteractionRights),
			LoadYaml<PlayerEntityContainer>("assets/entity_types/players", GameSystem<EquipSystem>.Instance().InitBarehands),
			LoadYaml<ActionPolicyList>("assets/action/action_policies.yml"),
			LoadYaml<SkillYaml>("assets/skill/skills", GameSystem<SkillSystem>.Instance().InitSkillList),
			LoadYaml<RewardYaml>("assets/skill/rewards", GameSystem<SkillSystem>.Instance().InitSkillRewards),
			LoadYaml<SkillCategoryYaml>("assets/skill/categories"),
			LoadYaml<MemoYaml>("assets/memos"),
			LoadYaml<CashYaml>("assets/cash"),
			LoadYaml<DateTimeYaml>("assets/survival/date_time", TimeGauge.Initialize),
			LoadYaml<FactionsYaml>("assets/factions.yml"),
			LoadYaml<JobsYaml>("assets/player/jobs.yml"),
			LoadYaml<ClanResearchYaml>("assets/research.yml"),
			LoadYaml<ClanYaml>("assets/clan.yml")
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
		if (SingletonDict<string, ActionSet>.Instance != null)
		{
			GameSystem<CombatSystem>.Instance().InitActionSetJson(SingletonDict<string, ActionSet>.Instance);
		}
		IsFinished = true;
		LoadSucceed = IsFinished && string.IsNullOrEmpty(Error);
	}

	private IEnumerator LoadYaml<T>(string postFix, Action<T> func = null, bool cacheable = false) where T : class
	{
		string url = KSingleton<GameManager>.Instance().MakeGatewayUrl(postFix);
		int retryCount = 0;
		T yamlData = (T)null;
		while (yamlData == null && retryCount < 5)
		{
			HTTPRequest request = KUtility.RequestUrl(url, null);
			while (request.MoveNext())
			{
				yield return null;
			}
			bool isCached;
			byte[] bytes = KUtility.ProcessResult(request, out isCached);
			if (isCached && LoadSucceed && (func == null || cacheable))
			{
				yield break;
			}
			yamlData = KUtility.ParseMsgPack<T>(bytes);
			retryCount++;
		}
		if (yamlData != null)
		{
			if (func != null)
			{
				func(yamlData);
			}
			else if (yamlData is ISingletonable singletonable)
			{
				singletonable.Initialize(yamlData);
			}
			else
			{
				Debug.LogError((object)("No function provided for initializing - " + typeof(T)));
			}
		}
		else
		{
			Error = url;
		}
	}
}

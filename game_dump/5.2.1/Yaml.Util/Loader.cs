using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

	[CompilerGenerated]
	private sealed class _003CCoLoadingYmls_003Ed__18 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		private IEnumerator[] _003Croutines_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoLoadingYmls_003Ed__18(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Croutines_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
				LoadState = State.Loading;
				Error = string.Empty;
				_003Croutines_003E5__2 = new IEnumerator[71]
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
			}
			bool flag = true;
			for (int i = 0; i < _003Croutines_003E5__2.Length; i++)
			{
				flag &= !_003Croutines_003E5__2[i].MoveNext();
			}
			if (!flag)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			LoadState = ((!string.IsNullOrEmpty(Error)) ? State.Failure : State.Succees);
			Cached = LoadState == State.Succees;
			if (LoadState == State.Succees)
			{
				Durango.Utils.Singleton<GameManager>.Instance().NotifyYamlLoaded();
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CLoadYaml_003Ed__19<T> : IEnumerator<object>, IDisposable, IEnumerator where T : class
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public string postFix;

		public Action<T> func;

		public bool cacheable;

		private string _003Curl_003E5__2;

		private int _003CretryCount_003E5__3;

		private HTTPRequest _003Crequest_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CLoadYaml_003Ed__19(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Curl_003E5__2 = null;
			_003Crequest_003E5__4 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_0079;
			}
			_003C_003E1__state = -1;
			_003Curl_003E5__2 = GameManager.GatewayUrl + postFix;
			_003CretryCount_003E5__3 = 0;
			bool disableCache = false;
			T val = null;
			goto IL_0111;
			IL_0111:
			bool isCached;
			if (val == null && _003CretryCount_003E5__3 < 5)
			{
				if (GameManager.ClusterMode == Mode.Online)
				{
					_003Crequest_003E5__4 = Http.Request(_003Curl_003E5__2, null, disableCache);
					goto IL_0079;
				}
				if (Cached && ((func == null) | cacheable))
				{
					return false;
				}
				isCached = true;
				val = Json.ReadFromFile<T>("offline" + postFix);
				goto IL_00fa;
			}
			if (val != null)
			{
				if (func != null)
				{
					try
					{
						func(val);
						return false;
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						Error = ex.Message;
						return false;
					}
				}
				if (val is ISingletonable singletonable)
				{
					singletonable.Initialize(val);
				}
				else
				{
					Debug.LogError("No function provided for initializing - " + typeof(T));
				}
			}
			else
			{
				Error = _003Curl_003E5__2;
			}
			return false;
			IL_0079:
			if (_003Crequest_003E5__4.MoveNext())
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			byte[] data = Http.ProcessResult(_003Crequest_003E5__4, out isCached);
			if (isCached && Cached && ((func == null) | cacheable))
			{
				return false;
			}
			val = Json.Read<T>(data);
			_003Crequest_003E5__4 = null;
			goto IL_00fa;
			IL_00fa:
			_003CretryCount_003E5__3++;
			disableCache = true;
			goto IL_0111;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadingYmls_003Ed__18(0);
	}

	private static IEnumerator LoadYaml<T>(string postFix, Action<T> func = null, bool cacheable = false) where T : class
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLoadYaml_003Ed__19<T>(0)
		{
			postFix = postFix,
			func = func,
			cacheable = cacheable
		};
	}
}

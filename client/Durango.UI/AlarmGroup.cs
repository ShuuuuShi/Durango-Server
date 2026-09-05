using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.LearningGuide;
using Durango.Logic.Skill;
using Durango.Logic.Statistics;
using Durango.Network;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Accelerator;
using Shared.Economy;
using Shared.Faction;
using Shared.Item;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class AlarmGroup : UIBase
{
	public enum RewardEffectType
	{
		LevelUp,
		Collect,
		CollectFail,
		Craft,
		CraftFail,
		Hunt,
		SkillCategoryUp,
		GetTitle,
		Faction,
		ExplorePOI,
		Taming,
		OfferCompleted,
		Repair,
		AdviseReward,
		TodayAttendanceReward,
		QuestScoreReward,
		AnimalDiscovered,
		WarpRushRewardReceived,
		ArtifactInterior,
		MultipleSkillCategoryUp
	}

	private const float NotifyDuration = 1.8f;

	[SerializeField]
	private AlarmMessageQueue _message;

	[SerializeField]
	private AlarmNotifyQueue _notifyMajor;

	[SerializeField]
	private AlarmScrollNotifyQueue _notifyMinor;

	[SerializeField]
	private AlarmNewsWidget _news;

	[SerializeField]
	private AlarmRewardQueue _rewardAlarms;

	[SerializeField]
	private AlarmWar _war;

	[SerializeField]
	private AlarmWarpRush _alarmWarpRush;

	[SerializeField]
	private EffectAlarmController _effectController;

	[SerializeField]
	private WarpAcceleratorEffects _warpAcceleratorEffects;

	[SerializeField]
	private UIPanel _upperPanel;

	[SerializeField]
	private float _gatherSkillCategoryUpDuration = 1f;

	private readonly Dictionary<int, List<AlarmRewardQueue.Args>> _gatherSkillCategoryUpArgs = new Dictionary<int, List<AlarmRewardQueue.Args>>();

	private float? _gatherSkillCategoryUpArgsStartAt;

	private Rewarded _currentRewarded;

	private Rewarded _prevRewarded;

	public AlarmWar War => _war;

	public WarpAcceleratorEffects WarpAcceleratorEffects => _warpAcceleratorEffects;

	private void Awake()
	{
		_message.gameObject.SetActive(value: true);
		_notifyMajor.gameObject.SetActive(value: true);
		_notifyMinor.gameObject.SetActive(value: true);
		_news.gameObject.SetActive(value: true);
	}

	private void Start()
	{
		_rewardAlarms.AddMessageGroup(0, _message);
		_rewardAlarms.AddMessageGroup(0, _alarmWarpRush);
		_rewardAlarms.AddMessageGroup(0, _warpAcceleratorEffects);
		GameSystem<StatisticsSystem>.Instance().Rewarded += StatisticsSystem_Rewarded;
		GameSystem<PvpIslandSystem>.Instance().Kill += PvpIslandSystem_Kill;
		Durango.Utils.Singleton<PlayerManager>.Instance().PlayerAppeared += OnPlayerAppear;
		Durango.Utils.Singleton<PlayerManager>.Instance().PlayerDisappeared += OnPlayerDisappear;
		Connections.Frontend.On<NotificationAdded>(OnNewsAlarm);
		Connections.Frontend.On<NotificationCanceled>(OnCancelNewsAlarm);
		Connections.Frontend.On<AlarmNotify>(OnAlarmNotify);
		NGUITools.SetLayer(_upperPanel.gameObject, LayerHelper.UIOverLayer);
	}

	private void Update()
	{
		float? gatherSkillCategoryUpArgsStartAt = _gatherSkillCategoryUpArgsStartAt;
		if (!gatherSkillCategoryUpArgsStartAt.HasValue || !(Time.time - _gatherSkillCategoryUpArgsStartAt.Value >= _gatherSkillCategoryUpDuration))
		{
			return;
		}
		foreach (KeyValuePair<int, List<AlarmRewardQueue.Args>> gatherSkillCategoryUpArg in _gatherSkillCategoryUpArgs)
		{
			int key = gatherSkillCategoryUpArg.Key;
			List<AlarmRewardQueue.Args> value = gatherSkillCategoryUpArg.Value;
			if (value.Count > 1)
			{
				AlarmRewardQueue.Args args = default(AlarmRewardQueue.Args);
				args.Main = T._("{0:lv:}", key);
				args.Sub = value[0].Sub;
				args.Icon = string.Empty;
				args.ExtraIcons = value.Select((AlarmRewardQueue.Args arg) => arg.Icon).ToArray();
				RewardAlarm(args, RewardEffectType.MultipleSkillCategoryUp);
			}
			else
			{
				RewardAlarm(value[0], RewardEffectType.SkillCategoryUp);
			}
		}
		_gatherSkillCategoryUpArgs.Clear();
		_gatherSkillCategoryUpArgsStartAt = null;
	}

	private void PvpIslandSystem_Kill(S02PVPKill msg)
	{
		string text = T._("[c=ui_pale_red]{0}[-]님을 처치했습니다.", msg.VictimName);
		string icon = "Icon_battle_emblem";
		bool major = false;
		Color32? iconColor = PresetColor.UIPaleRed;
		ShowNotify(text, icon, major, 1.8f, null, null, iconColor);
	}

	private void StatisticsSystem_Rewarded(Rewarded msg)
	{
		if (GameSystem<StatisticsSystem>.Instance().Level > 1)
		{
			_currentRewarded = msg;
			ShowRewardAlarm(msg.Effect);
			GameSystem<SocialSystem>.Instance().AddSystemChat(MakeRewardedComment(msg), string.Empty);
			_prevRewarded = msg;
		}
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		_notifyMinor.RefreshVisibleHeight();
	}

	public void PushMessage(string key, string message, float duration, float scale = 1f)
	{
		_message.PushMessage(key, message, duration, scale);
	}

	public bool HasNotify(string key, bool major)
	{
		return GetAlarmNotifyQueue(major).HasAlarm(key);
	}

	public void ShowNotify(string text, PortraitBuilder.Argument arg, bool major, float duration = 1.8f, Action viewMoreAction = null, string key = null)
	{
		GetAlarmNotifyQueue(major).ShowAlarm(key, text, arg, duration, viewMoreAction);
	}

	public void ShowNotify(string text, string icon, bool major, float duration = 1.8f, Action viewMoreAction = null, string key = null, Color32? iconColor = null)
	{
		if (!iconColor.HasValue)
		{
			iconColor = ((!major) ? PresetColor.UIBeige : Color.white);
		}
		GetAlarmNotifyQueue(major).ShowAlarm(key, text, icon, iconColor.Value, duration, viewMoreAction);
	}

	public void HideNotify(string key, bool major)
	{
		GetAlarmNotifyQueue(major).HideAlarm(key);
	}

	public void RewardAlarm(AlarmRewardQueue.Args args, RewardEffectType type, float delay = 0f)
	{
		RewardAlarm(null, args, type, delay);
	}

	public void RewardAlarm(string key, AlarmRewardQueue.Args args, RewardEffectType type, float delay = 0f)
	{
		_rewardAlarms.Register(key, args, type, delay);
	}

	public void StopRewardAlarm(string key)
	{
		_rewardAlarms.Stop(key);
	}

	public void PauseRewardAlarm(string key, bool pause)
	{
		_rewardAlarms.Pause(key, pause);
	}

	private AlarmNotifyQueueBase GetAlarmNotifyQueue(bool major)
	{
		if (major)
		{
			return _notifyMajor;
		}
		return _notifyMinor;
	}

	private static void ShowMissionRewardPopup(Rewarded rewarded, Rewarded? bonus)
	{
		ReceiveRewardsPopup receiveRewardsPopup = UIManager.Popup.FindTooltip<ReceiveRewardsPopup>();
		receiveRewardsPopup.ShowMissionRewarded(rewarded, bonus);
		if (bonus.HasValue)
		{
			receiveRewardsPopup.Hide();
		}
	}

	private static bool IsRewardPopupAllowed(MissionCompletedEffect msg)
	{
		if (!string.IsNullOrEmpty(msg.MissionId))
		{
			return msg.MissionId != "sh_sq_01";
		}
		return false;
	}

	private void OnNewsAlarm(NotificationAdded msg, PacketHeader header)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double num = ((!msg.Since.HasValue) ? predictedServerTime : msg.Since.Value);
		double num2 = ((!msg.Until.HasValue) ? (predictedServerTime + 86400.0) : msg.Until.Value);
		float time = Time.time;
		float since = time + (float)(num - predictedServerTime);
		float until = time + (float)(num2 - predictedServerTime);
		_news.Register(msg.Id, msg.Text, since, until, msg.Period);
	}

	private void OnCancelNewsAlarm(NotificationCanceled msg, PacketHeader header)
	{
		_news.Remove(msg.Id);
	}

	private void OnAlarmNotify(AlarmNotify msg, PacketHeader header)
	{
		ShowNotify(msg.Text, msg.Icon, major: false, (!msg.Duration.HasValue) ? 1.8f : msg.Duration.Value, delegate
		{
			UIUtility.OpenUri(string.Empty, msg.Uri);
		}, msg.Key.ToString());
	}

	public void ShowRewardAlarm(object effect)
	{
		if (effect is HuntRewardEffect)
		{
			DoHuntRewardEffect(effect);
		}
		else if (effect is LevelUpEffect)
		{
			DoLevelUpEffect(effect);
		}
		else if (effect is CategoryLevelUpRewardEffect)
		{
			DoCategoryLevelUpRewardEffect(effect);
		}
		else if (effect is TamingCompletedEffect)
		{
			DoTamingCompletedEffect(effect);
		}
		else if (effect is AdviceCompletedEffect)
		{
			DoAdviceCompltedEffect(effect);
		}
		else if (effect is FactionLevelUpEffect)
		{
			DoFactionLevelUpEffect(effect);
		}
		else if (effect is DailyMissionCompletedEffect)
		{
			DoFactionEventDailyCompletedEffect();
		}
		else if (effect is MissionCompletedEffect)
		{
			DoFactionEventCompletedEffect(effect);
		}
		else if (effect is AttachmentReceivedEffect)
		{
			DoAttachmentReceivedEffect();
		}
		else if (effect is ExplorePOIEffect)
		{
			DoExplorePoiEffect(effect);
		}
		else if (effect is RepairEffect)
		{
			DoRepairEffect(effect);
		}
		else if (effect is S02SupplyRewardsEffect)
		{
			DoS02SupplyRewardsEffect(effect);
		}
		else if (effect is PetTaskFinishedEffect)
		{
			DoPetTaskFinishedEffect(effect);
		}
		else if (effect is PetLevelUpEffect)
		{
			DoPetLevelUpEffect(effect);
		}
		else if (effect is ArchipelagoRegionRewardsEffect)
		{
			DoArchipelagoRegionRewardsEffect();
		}
		else if (effect is PioneerGradeUpEffect)
		{
			DoPioneerGradeUpEffect(effect);
		}
		else if (effect is ResistanceLevelUpEffect)
		{
			DoResistanceLevelUpEffect(effect);
		}
		else if (effect is RankingRewardEffect)
		{
			DoWarpRushRankingRewardsEffect();
		}
		else if (effect is WarpAccelerationRewardsEffect)
		{
			DoWarpAccelerationRewardsEffect();
		}
		else if (effect is OpenRewardBoxEffect)
		{
			DoOpenRewardBoxEffect(effect);
		}
	}

	private void DoHuntRewardEffect(object effect)
	{
		HuntRewardEffect huntRewardEffect = (HuntRewardEffect)effect;
		RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = $"<em>{huntRewardEffect.TargetAnimal}</em>",
			Sub = T._("사냥 성공")
		}, RewardEffectType.Hunt);
	}

	private void DoLevelUpEffect(object effect)
	{
		LevelUpEffect ef = (LevelUpEffect)effect;
		RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = T._("[b]LEVEL {0}", ef.Level),
			Sub = T._("레벨이 올랐습니다."),
			Action = delegate
			{
				_effectController.Play((ef.Level >= 10) ? EffectAlarmController.EffectType.LevelUp : EffectAlarmController.EffectType.SmallLevelUp);
			}
		}, RewardEffectType.LevelUp);
	}

	private void DoPetLevelUpEffect(object effect)
	{
		PetLevelUpEffect petLevelUpEffect = (PetLevelUpEffect)effect;
		string playerPetId = Durango.Utils.Singleton<PetManager>.Instance().GetPlayerPetId();
		_effectController.Play(playerPetId, EffectAlarmController.EffectType.PetLevelUp);
		string petName = petLevelUpEffect.PetName;
		string text = T._("{0:이} {1:이} 되었습니다.", petName, LocalizeUtil.FormatLevel(petLevelUpEffect.Level));
		UIManager.FindScript<IndicatorGroup>().Show("icon_levelup_pet", text);
		UIManager.Alarm.ShowNotify(text, "act_open_pet", major: false);
		SoundManager.PlayEvent("ui_animal_levelup");
		if (petLevelUpEffect.MilestoneAvailable)
		{
			string text2 = T._("지금 {0}의 속성을 발견할 수 있습니다", petName);
			UIManager.FindScript<IndicatorGroup>().Show("icon_levelup_pet", text2);
			UIManager.Alarm.ShowNotify(text2, "act_open_pet", major: true);
		}
	}

	private void DoCategoryLevelUpRewardEffect(object effect)
	{
		CategoryLevelUpRewardEffect obj = (CategoryLevelUpRewardEffect)effect;
		string sub = T._("스킬 계열 레벨업");
		foreach (KeyValuePair<Shared.Skill.Category, int> changedLevel in obj.ChangedLevels)
		{
			GatherSkillCategoryUpArgs(changedLevel.Value, new AlarmRewardQueue.Args
			{
				Main = T._("{0} {1:lv:}", Durango.Logic.Skill.Util.CategoryLocalizeName(changedLevel.Key), changedLevel.Value),
				Sub = sub,
				Icon = Durango.Logic.Skill.Util.CategoryIcon(changedLevel.Key)
			});
		}
	}

	private void DoTamingCompletedEffect(object effect)
	{
		TamingCompletedEffect tamingCompletedEffect = (TamingCompletedEffect)effect;
		Animal animal = SingletonDict<int, Animal>.Get(tamingCompletedEffect.AnimalEntityType);
		RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = ((animal != null) ? animal.Name.ToString() : tamingCompletedEffect.AnimalEntityType.ToString()),
			Sub = T._("포획 성공!"),
			Icon = ((animal != null) ? animal.Portrait : string.Empty)
		}, RewardEffectType.Taming);
		GameSystem<InventorySystem>.Instance().AddOnItemEvent(tamingCompletedEffect.ReinsId, delegate(ItemData item)
		{
			IconMoveEffectGroup iconMoveEffectGroup = UIManager.FindScript<IconMoveEffectGroup>();
			if (!(iconMoveEffectGroup == null))
			{
				iconMoveEffectGroup.ShowGatheringItemEffect(item.Icon);
			}
		});
	}

	private void DoAdviceCompltedEffect(object effect)
	{
		AdviceCompletedEffect adviceCompletedEffect = (AdviceCompletedEffect)effect;
		Durango.Logic.LearningGuide.Advice adviceByTitleId = GameSystem<StatisticsSystem>.Instance().GetAdviceByTitleId(adviceCompletedEffect.TitleId);
		string main = $"<em>{((adviceByTitleId == null) ? adviceCompletedEffect.TitleId : adviceByTitleId.Name)}</em>";
		if (adviceByTitleId != null)
		{
			Durango.Logic.LearningGuide.AdviceCategory adviceCategory = GameSystem<StatisticsSystem>.Instance().GetAdviceCategory(adviceByTitleId.Category);
			string text = ((adviceCategory == null) ? string.Empty : adviceCategory.Icon);
			RewardAlarm(new AlarmRewardQueue.Args
			{
				Main = main,
				Sub = T._("가이드 완료"),
				Icon = text,
				IconScale = 1.1f
			}, RewardEffectType.AdviseReward);
		}
	}

	private void DoFactionLevelUpEffect(object effect)
	{
		FactionLevelUpEffect factionLevelUpEffect = (FactionLevelUpEffect)effect;
		string text = IconMap.Get("portrait_" + factionLevelUpEffect.FactionType);
		RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = $"<em>{factionLevelUpEffect.LevelName}</em>",
			Sub = T._("단체 관계 상승"),
			Icon = text,
			IconScale = 1.1f
		}, RewardEffectType.Faction);
	}

	private void DoFactionEventCompletedEffect(object effect)
	{
		MissionCompletedEffect msg = (MissionCompletedEffect)effect;
		if (IsRewardPopupAllowed(msg))
		{
			ShowMissionRewardPopup(_currentRewarded, null);
			return;
		}
		RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = $"<em>{msg.FactionType.GetName()}</em>",
			Sub = T._("임무 성공"),
			Icon = IconMap.Get(msg.FactionType)
		}, RewardEffectType.Faction);
	}

	private void DoFactionEventDailyCompletedEffect()
	{
		if (_prevRewarded.Effect is MissionCompletedEffect)
		{
			ShowMissionRewardPopup(_prevRewarded, _currentRewarded);
		}
	}

	private void DoAttachmentReceivedEffect()
	{
		UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowRewardInfo(T._("우편 받기"), T._("확인"), "ui_menu_quest_recieve", effectOn: false, _currentRewarded.Reward);
	}

	private void DoExplorePoiEffect(object effect)
	{
		ExplorePOIEffect explorePOIEffect = (ExplorePOIEffect)effect;
		RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = ((!string.IsNullOrEmpty(explorePOIEffect.PoiName)) ? explorePOIEffect.PoiName : T._("워프 에너지 불균형점")),
			Sub = T._("발견")
		}, RewardEffectType.ExplorePOI);
	}

	private void DoRepairEffect(object effect)
	{
		RepairEffect repairEffect = (RepairEffect)effect;
		string text = string.Empty;
		string sub = string.Empty;
		if (!string.IsNullOrEmpty(repairEffect.ItemId))
		{
			ItemData itemData = GameSystem<InventorySystem>.Instance().FindItem(repairEffect.ItemId);
			text = T._("<em>{0}</em> {1:lv:}", itemData.Name, itemData.Level);
			sub = repairEffect.Result switch
			{
				Result.GreatSuccess => T._("수리 대성공"), 
				Result.Success => T._("수리 성공"), 
				_ => T._("수리 완료"), 
			};
		}
		else if (!string.IsNullOrEmpty(repairEffect.EntityId))
		{
			Artifact artifact = Durango.Utils.Singleton<ArtifactManager>.Instance().Find(repairEffect.EntityId);
			if (artifact != null)
			{
				text = repairEffect.Result switch
				{
					Result.GreatSuccess => T._("수리에 대성공하였습니다."), 
					Result.Success => T._("수리에 성공하였습니다."), 
					_ => (!artifact.ArtifactState.IsRepairing()) ? T._("수리가 완료되었습니다.") : T._("수리가 시작됩니다."), 
				};
				if (artifact.ArtifactState.Repairement.HasValue)
				{
					double seconds = artifact.ArtifactState.Repairement.Value.Item2 - Connections.Frontend.GetPredictedServerTime();
					sub = T._("{0} 후 내구도가 회복됩니다.", TimedeltaFormatter.Format(seconds));
				}
			}
		}
		if (text != string.Empty)
		{
			RewardAlarm(new AlarmRewardQueue.Args
			{
				Main = text,
				Sub = sub
			}, RewardEffectType.Repair);
		}
	}

	private void DoS02SupplyRewardsEffect(object effectObject)
	{
		S02SupplyRewardsEffect s02SupplyRewardsEffect = (S02SupplyRewardsEffect)effectObject;
		ReceiveRewardsPopup receiveRewardsPopup = UIManager.Popup.FindTooltip<ReceiveRewardsPopup>();
		WarpRushReward reward;
		if (s02SupplyRewardsEffect.IsLevelUpReward)
		{
			reward = Yaml.Util.Singleton<Yaml.WarpRushRewards>.Instance.GetLevelReward(s02SupplyRewardsEffect.ResourceType, s02SupplyRewardsEffect.Level);
		}
		else
		{
			List<WarpRushReward> supplyReward = Yaml.Util.Singleton<Yaml.WarpRushRewards>.Instance.GetSupplyReward(s02SupplyRewardsEffect.ResourceType, s02SupplyRewardsEffect.Level);
			if (!s02SupplyRewardsEffect.RewardIndex.HasValue || supplyReward.Count <= s02SupplyRewardsEffect.RewardIndex.Value)
			{
				return;
			}
			reward = supplyReward[s02SupplyRewardsEffect.RewardIndex.Value];
		}
		string deliveryMessage = WarpRushSystem.GetDeliveryMessage(s02SupplyRewardsEffect.IsLevelUpReward, s02SupplyRewardsEffect.ResourceType);
		receiveRewardsPopup.ShowWarpRushRewardItemReceived(deliveryMessage, reward);
	}

	private void DoArchipelagoRegionRewardsEffect()
	{
		if (!_currentRewarded.Reward.IsEmpty())
		{
			UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowRewardInfo(T._("개척 임무 완료 보상"), T._("확인"), "ui_menu_quest_recieve", effectOn: true, _currentRewarded.Reward);
		}
		GameSystem<ArchipelagoMissionSystem>.Instance().EndMission();
	}

	private void DoWarpRushRankingRewardsEffect()
	{
		UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowRewardInfo(T._("워프 러시 랭킹 보상"), T._("확인"), "ui_menu_quest_recieve", effectOn: true, _currentRewarded.Reward);
		WarpRushSystem.RequestRewardedRanking();
	}

	private void DoWarpAccelerationRewardsEffect()
	{
		List<WarpAcceleratorInfo> warpAccelerators = GameSystem<WarpAcceleratorSystem>.Instance().WarpAccelerators;
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		Point2? point = null;
		if (lastInteractionTarget != null)
		{
			point = new Point2(lastInteractionTarget.Tile);
		}
		WarpAcceleratorInfo? warpAcceleratorInfo = null;
		WarpAcceleratorInfo? warpAcceleratorInfo2 = null;
		foreach (WarpAcceleratorInfo item in warpAccelerators)
		{
			if (item.Warpaccelerator.Status == AcceleratorStatus.End && item.Warpaccelerator.Participants != null && item.Warpaccelerator.Participants.Contains(GameManager.PlayerId))
			{
				warpAcceleratorInfo2 = item;
				if (point.HasValue && item.Tile == point.Value)
				{
					warpAcceleratorInfo = item;
				}
			}
		}
		if (warpAcceleratorInfo.HasValue)
		{
			warpAcceleratorInfo2 = warpAcceleratorInfo;
		}
		if (!warpAcceleratorInfo2.HasValue)
		{
			UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowRewardInfo(T._("워프 가속 보상"), T._("확인"), "ui_warpaccelerator_reward", effectOn: true, _currentRewarded.Reward);
		}
		else
		{
			UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowWarpAcceleratorRewardInfo(T._("워프 가속 보상"), T._("확인"), "ui_warpaccelerator_reward", effectOn: true, _currentRewarded.Reward, warpAcceleratorInfo2.Value);
		}
	}

	private void DoPioneerGradeUpEffect(object effectObject)
	{
		PioneerGradeUpEffect effect = (PioneerGradeUpEffect)effectObject;
		UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowPioneerGradeUp(effect, _currentRewarded.Reward);
	}

	private void DoResistanceLevelUpEffect(object effectObject)
	{
		ResistanceLevelUpEffect resistanceLevelUpEffect = (ResistanceLevelUpEffect)effectObject;
		RewardAlarm(new AlarmRewardQueue.Args
		{
			Main = LocalizeUtil.FormatLevel(resistanceLevelUpEffect.Level),
			Sub = T._("신체 {0} 레벨업", resistanceLevelUpEffect.ResistanceType.GetName()),
			Icon = IconMap.Get(resistanceLevelUpEffect.ResistanceType)
		}, RewardEffectType.SkillCategoryUp);
	}

	private void DoOpenRewardBoxEffect(object effectObject)
	{
		OpenRewardBoxEffect effect = (OpenRewardBoxEffect)effectObject;
		UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowOpenRewardBox(effect, _currentRewarded.Reward);
	}

	private void DoPetTaskFinishedEffect(object effectObject)
	{
		PetTaskFinishedEffect effect = (PetTaskFinishedEffect)effectObject;
		UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowPetTaskFinished(effect, _currentRewarded.Reward);
	}

	private void GatherSkillCategoryUpArgs(int changedLevel, AlarmRewardQueue.Args args)
	{
		float? gatherSkillCategoryUpArgsStartAt = _gatherSkillCategoryUpArgsStartAt;
		if (!gatherSkillCategoryUpArgsStartAt.HasValue)
		{
			_gatherSkillCategoryUpArgsStartAt = Time.time;
		}
		if (!_gatherSkillCategoryUpArgs.TryGetValue(changedLevel, out var value))
		{
			value = new List<AlarmRewardQueue.Args>();
			_gatherSkillCategoryUpArgs[changedLevel] = value;
		}
		value.Add(args);
	}

	private void OnPlayerAppear(PlayerBehavior player)
	{
		if (GameManager.ClusterMode != 0 && !player.IsLocalPlayer)
		{
			_notifyMinor.ShowAlarm("OfflinePlayerConnect", $"<em>{player.PlayerName} [icon=friends_add]</em>", player.GetPortraitArgument(), 5f, null);
		}
	}

	private void OnPlayerDisappear(PlayerBehavior player)
	{
		if (GameManager.ClusterMode != 0 && !player.IsLocalPlayer)
		{
			_notifyMinor.ShowAlarm("OfflinePlayerDisconnect", $"<weak>{player.PlayerName} [icon=icon_offline]</weak>", player.GetPortraitArgument(), 5f, null);
		}
	}

	private unsafe static string MakeRewardedComment(Rewarded rewarded)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>();
		object effect = rewarded.Effect;
		if (effect is HuntRewardEffect)
		{
			stringBuilder.AppendLine(T._("{0} 사냥에 성공했습니다", ((HuntRewardEffect)effect).TargetAnimal));
		}
		else if (effect is LevelUpEffect)
		{
			stringBuilder.AppendLine(T._("{0:lv:}{0:-이} 되었습니다", ((LevelUpEffect)effect).Level));
		}
		else if (effect is SkillRewardEffect skillRewardEffect)
		{
			Node node = GameSystem<SkillSystem>.Instance().FindSkill(skillRewardEffect.LearnedSkill);
			if (node != null)
			{
				stringBuilder.AppendLine(T._("{0:을} 배웠습니다", node.Name));
				if (KUtility.GetSize(node.Rewards) > 0)
				{
					for (int i = 0; i < node.Rewards.Length; i++)
					{
						node.Rewards[i].ToReadableText(stringBuilder, node.State);
					}
				}
			}
		}
		else if (effect is object obj)
		{
			list.Clear();
			foreach (KeyValuePair<Shared.Skill.Category, int> changedLevel in ((CategoryLevelUpRewardEffect*)(&obj))->ChangedLevels)
			{
				list.Add(T._("{0} {1:lv:}", Durango.Logic.Skill.Util.CategoryLocalizeName(changedLevel.Key), changedLevel.Value));
			}
			stringBuilder.AppendLine(T._("{0:l:{}|, } 되었습니다", list));
		}
		else if (effect is TamingCompletedEffect tamingCompletedEffect)
		{
			Animal animal = SingletonDict<int, Animal>.Get(tamingCompletedEffect.AnimalEntityType);
			string text = ((animal != null) ? animal.Name.ToString() : tamingCompletedEffect.AnimalEntityType.ToString());
			stringBuilder.AppendLine(T._("{0:을} 포획했습니다", text));
		}
		else if (effect is AdviceCompletedEffect adviceCompletedEffect)
		{
			Durango.Logic.Statistics.Title title = GameSystem<StatisticsSystem>.Instance().GetTitle(adviceCompletedEffect.TitleId);
			stringBuilder.AppendLine(T._("타이틀 {0:을} 얻었습니다", (title != null) ? title.Name : adviceCompletedEffect.TitleId));
		}
		else if (effect is FactionLevelUpEffect factionLevelUpEffect)
		{
			stringBuilder.AppendLine(T._("{0:와} {1:이} 되었습니다", factionLevelUpEffect.FactionType.GetName(), factionLevelUpEffect.LevelName));
		}
		else if (effect is MissionCompletedEffect)
		{
			stringBuilder.AppendLine(T._("{0}의 임무를 완료했습니다", ((MissionCompletedEffect)effect).FactionType.GetName()));
		}
		else if (effect is DailyMissionCompletedEffect)
		{
			stringBuilder.AppendLine(T._("다음의 보상을 받았습니다"));
		}
		else if (effect is ExplorePOIEffect explorePOIEffect)
		{
			stringBuilder.AppendLine(T._("{0:을} 발견했습니다", (!string.IsNullOrEmpty(explorePOIEffect.PoiName)) ? explorePOIEffect.PoiName : T._("워프 에너지 불균형점")));
		}
		else if (effect is RepairEffect)
		{
			switch (((RepairEffect)effect).Result)
			{
			default:
				stringBuilder.AppendLine(T._("수리를 완료했습니다"));
				break;
			case Result.GreatSuccess:
				stringBuilder.AppendLine(T._("수리에 대성공하였습니다"));
				break;
			case Result.Success:
				stringBuilder.AppendLine(T._("수리에 성공하였습니다"));
				break;
			}
		}
		RewardInfo reward = rewarded.Reward;
		if (reward.Exp > 0)
		{
			stringBuilder.AppendLine(T._("경험치 {0:+0;-0}", reward.Exp));
		}
		if (KUtility.GetSize(reward.Currency) > 0)
		{
			list.Clear();
			foreach (KeyValuePair<Currency, long> item in reward.Currency)
			{
				if (item.Value > 0)
				{
					list.Add(Durango.Logic.Item.Inventory.CurrencyFormat(item.Value, item.Key));
				}
			}
			if (list.Count > 0)
			{
				stringBuilder.AppendLine(T._("{0:l:{}|, }", list));
			}
		}
		if (reward.SkillPoints > 0)
		{
			stringBuilder.AppendLine(T._("스킬 포인트 {0:+0;-0}", reward.SkillPoints));
		}
		if (KUtility.GetSize(reward.Titles) > 0)
		{
			list.Clear();
			int j = 0;
			for (int num = reward.Titles.Length; j < num; j++)
			{
				Durango.Logic.Statistics.Title title2 = GameSystem<StatisticsSystem>.Instance().GetTitle(reward.Titles[j]);
				list.Add((title2 != null) ? title2.Name : reward.Titles[j]);
			}
			stringBuilder.AppendLine(T._("칭호 획득: {0:l:{}|, }", list));
		}
		if (KUtility.GetSize(reward.UnlockedSkills) > 0)
		{
			list.Clear();
			int k = 0;
			for (int num2 = reward.UnlockedSkills.Length; k < num2; k++)
			{
				Node node2 = GameSystem<SkillSystem>.Instance().FindSkill(reward.UnlockedSkills[k]);
				list.Add((node2 != null) ? node2.Name : reward.UnlockedSkills[k].SkillId);
			}
			stringBuilder.AppendLine(T._("스킬 잠금 해제: {0:l:{}|, }", list));
		}
		if (KUtility.GetSize(reward.Abilities) > 0)
		{
			list.Clear();
			foreach (KeyValuePair<Basic, int> ability in reward.Abilities)
			{
				if (ability.Value != 0)
				{
					string text2 = LocalizeUtil.Get(ability.Key);
					stringBuilder.AppendLine(T._("{0} {1:+0;-0}", text2, ability.Value));
				}
			}
			stringBuilder.AppendLine(T._("{0:l:{}|, }", list));
		}
		if (KUtility.GetSize(reward.FriendshipPoint) > 0)
		{
			foreach (KeyValuePair<FactionType, int> item2 in reward.FriendshipPoint)
			{
				if (item2.Value != 0)
				{
					Yaml.Faction faction = SingletonDict<FactionType, Yaml.Faction>.Get(item2.Key);
					stringBuilder.AppendLine(T._("{0} 우호도 {1:+0;-0}", faction.Name, item2.Value));
				}
			}
		}
		if (KUtility.GetSize(reward.Items) > 0)
		{
			for (int l = 0; l < reward.Items.Length; l++)
			{
				stringBuilder.AppendLine(T._("{0} x{1}", reward.Items[l].NameGettext, reward.Items[l].Count));
			}
		}
		return stringBuilder.ToString().Trim();
	}

	[ExposedInEditor(null)]
	private void KillMsgTest()
	{
		Connections.Frontend.PushPacket(new S02PVPKill
		{
			VictimName = "댕댕이"
		});
	}
}

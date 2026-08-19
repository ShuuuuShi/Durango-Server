using System;
using System.Collections.Generic;
using System.Text;
using AutoGuide;
using CameraEffects;
using ItemSystem;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Economy;
using Shared.Faction;
using Shared.Guide;
using Shared.Skill;
using SkillData;
using StatisticsData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class RewardAlarmGroup : UIBase
{
	public enum RewardEffectType
	{
		LevelUp,
		SkillCategoryLevelUp,
		POI,
		Craft,
		HuntReward,
		SkillReward,
		Collect
	}

	public enum RewardReason
	{
		GrownUp,
		Success
	}

	[Serializable]
	[EnumType(typeof(RewardEffectType))]
	private class EffectOptionList : EnumKeyList
	{
		[SerializeField]
		private List<EffectOption> _values;

		public List<EffectOption> Values => _values;

		public EffectOption Get(RewardEffectType type)
		{
			int num = IndexOf((int)type);
			if (num == -1)
			{
				return default(EffectOption);
			}
			return _values[num];
		}
	}

	private struct RewardStruct
	{
		public float ValidAt;

		public string Title;

		public string Comment;

		public EffectOption EffectOption;
	}

	[Serializable]
	private struct EffectOption
	{
		public int Priority;

		public float FontScale;

		public Vector3 EffectOffset;

		public float CameraZoomRatio;

		public float CameraZoomPeriod;

		public AudioClipType Sound;

		public float BgmMute;
	}

	[SerializeField]
	private RewardAlarm _rewardAlarmBase;

	[SerializeField]
	private GameObject _rewardEffect;

	[SerializeField]
	private EffectOptionList _effectOptionList;

	private readonly List<RewardAlarm> _rewardAlarms = new List<RewardAlarm>();

	private readonly Stack<RewardAlarm> _alarmaPool = new Stack<RewardAlarm>();

	private readonly List<RewardStruct> _rewardMessageQueue = new List<RewardStruct>();

	private int _defaultFontsize;

	private void Awake()
	{
		((Component)_rewardAlarmBase).gameObject.SetActive(false);
		_rewardEffect.gameObject.SetActive(false);
		_defaultFontsize = _rewardAlarmBase.TitleFontSize;
	}

	private void Start()
	{
		int i = 0;
		for (int count = _effectOptionList.Values.Count; i < count; i++)
		{
			EffectOption effectOption = _effectOptionList.Values[i];
			if (!string.IsNullOrEmpty(effectOption.Sound.Path))
			{
				SoundManager.Cache(effectOption.Sound.Path);
			}
		}
	}

	private void OnEnable()
	{
		GameSystem<StatisticsSystem>.Instance().OnRewarded += OnRewarded;
	}

	private void OnDisable()
	{
		GameSystem<StatisticsSystem>.Instance().OnRewarded -= OnRewarded;
		int i = 0;
		for (int count = _rewardAlarms.Count; i < count; i++)
		{
			((Component)_rewardAlarms[i]).gameObject.SetActive(false);
		}
		_rewardAlarms.Clear();
	}

	private void Update()
	{
		if (UIManager.IsLoadingCurtain || _rewardMessageQueue.Count <= 0)
		{
			return;
		}
		float time = Time.time;
		int num = -1;
		int num2 = -1;
		int i = 0;
		for (int count = _rewardMessageQueue.Count; i < count; i++)
		{
			RewardStruct rewardStruct = _rewardMessageQueue[i];
			if (rewardStruct.ValidAt > time)
			{
				break;
			}
			if (rewardStruct.ValidAt <= time && num < rewardStruct.EffectOption.Priority)
			{
				num = rewardStruct.EffectOption.Priority;
				num2 = i;
			}
		}
		if (num2 != -1)
		{
			RewardAlarm rewardAlarm = ((_rewardAlarms.Count != 0) ? _rewardAlarms[0] : null);
			if ((Object)(object)rewardAlarm == (Object)null || rewardAlarm.Priority < num || rewardAlarm.ReadyToHide)
			{
				RewardStruct reward = _rewardMessageQueue[num2];
				_rewardMessageQueue.RemoveAt(num2);
				Show(reward);
			}
		}
	}

	[ExposedInEditor(null)]
	public void Show(string title, string comment, RewardEffectType type, float delay = 0f)
	{
		AddToQueue(title, comment, _effectOptionList.Get(type), delay);
	}

	private void AddToQueue(string title, string comment, EffectOption option, float delay)
	{
		if (KSingleton<PlayerController>.Instance().IsInServerSideBattle)
		{
			delay += Connections.Frontend.SeverDelayTime;
		}
		RewardStruct rewardStruct = default(RewardStruct);
		rewardStruct.ValidAt = Time.time + delay;
		rewardStruct.Title = title;
		rewardStruct.Comment = comment;
		rewardStruct.EffectOption = option;
		RewardStruct item = rewardStruct;
		int num = -1;
		int i = 0;
		for (int count = _rewardMessageQueue.Count; i < count; i++)
		{
			if (_rewardMessageQueue[i].ValidAt > item.ValidAt)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			_rewardMessageQueue.Add(item);
		}
		else
		{
			_rewardMessageQueue.Insert(num, item);
		}
	}

	private void Show(RewardStruct reward)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		RewardAlarm rewardAlarm = AlarmPop();
		rewardAlarm.Set(reward.Title, reward.Comment);
		rewardAlarm.TitleFontSize = (int)((float)_defaultFontsize * reward.EffectOption.FontScale);
		rewardAlarm.Priority = reward.EffectOption.Priority;
		if (!string.IsNullOrEmpty(reward.EffectOption.Sound))
		{
			SoundManager.Play((string)reward.EffectOption.Sound, loop: false, default(SoundManager.PitchRange));
			KSingleton<BGMManager>.Instance().Mute(reward.EffectOption.BgmMute);
		}
		if (_rewardAlarms.Count == 0)
		{
			rewardAlarm.Show();
		}
		else
		{
			RewardAlarm rewardAlarm2 = _rewardAlarms[0];
			Vector3 val = ((Component)rewardAlarm2.AnimWidget).transform.localPosition + Vector3.down * (float)rewardAlarm2.Widget.height;
			rewardAlarm.Show(val);
			AnimationWidget animWidget = rewardAlarm2.AnimWidget;
			animWidget.Position += (float)rewardAlarm.Widget.height * Vector3.up * 0.5f - val;
			rewardAlarm2.Hide();
		}
		_rewardAlarms.Add(rewardAlarm);
		_rewardEffect.transform.localPosition = reward.EffectOption.EffectOffset;
		_rewardEffect.SetActive(false);
		_rewardEffect.SetActive(true);
		if (reward.EffectOption.CameraZoomRatio > 0f)
		{
			DollyCameraEffect cameraEffect = new DollyCameraEffect(reward.EffectOption.CameraZoomRatio, reward.EffectOption.CameraZoomPeriod, 0f, 3f);
			KSingleton<CameraController>.Instance().AddCameraEffect(cameraEffect);
		}
	}

	private RewardAlarm AlarmPop()
	{
		RewardAlarm rewardAlarm;
		if (_alarmaPool.Count > 0)
		{
			rewardAlarm = _alarmaPool.Pop();
		}
		else
		{
			GameObject val = ((Component)((Component)_rewardAlarmBase).transform.parent).gameObject.AddChild(((Component)_rewardAlarmBase).gameObject);
			rewardAlarm = val.GetComponent<RewardAlarm>();
			rewardAlarm.Disabled = AlarmPush;
			rewardAlarm.OnHide = AlarmHide;
		}
		return rewardAlarm;
	}

	private void AlarmPush(RewardAlarm alarm)
	{
		_alarmaPool.Push(alarm);
	}

	private void AlarmHide(RewardAlarm alarm)
	{
		_rewardAlarms.Remove(alarm);
	}

	private void OnRewarded(Rewarded msg)
	{
		if (MakeRewardedTitle(msg.Effect, out var title, out var effectType))
		{
			string comment = MakeRewardedComment(msg);
			Show(title, comment, effectType);
			PlayerController.PlayRewardMotion(RewardReason.GrownUp);
		}
	}

	private static bool MakeRewardedTitle(object effect, out string title, out RewardEffectType effectType)
	{
		if (effect is HuntRewardEffect)
		{
			DoHuntRewardEffect(effect, out title, out effectType);
		}
		else if (effect is LevelUpEffect)
		{
			DoLevelUpEffect(effect, out title, out effectType);
		}
		else if (effect is SkillRewardEffect)
		{
			DoSkillRewardEffect(effect, out title, out effectType);
		}
		else if (effect is CategoryLevelUpRewardEffect)
		{
			DoCategoryLevelUpRewardEffect(effect, out title, out effectType);
		}
		else if (effect is TamingCompletedEffect)
		{
			DoTamingCompletedEffect(effect, out title, out effectType);
		}
		else if (effect is OfferCompletedEffect)
		{
			DoOfferCompletedEffect(effect, out title, out effectType);
		}
		else if (effect is GetTargetTitleEffect)
		{
			DoGetTargetTitleEffect(effect, out title, out effectType);
		}
		else if (effect is FactionLevelUpEffect)
		{
			DoFactionLevelUpEffect(effect, out title, out effectType);
		}
		else if (effect is FactionEventCompletedEffect)
		{
			DoFactionEventCompletedEffect(effect, out title, out effectType);
		}
		else
		{
			if (!(effect is ExplorePOIEffect))
			{
				title = null;
				effectType = RewardEffectType.LevelUp;
				return false;
			}
			DoExplorePoiEffect(effect, out title, out effectType);
		}
		return true;
	}

	private static void DoHuntRewardEffect(object effect, out string title, out RewardEffectType effectType)
	{
		HuntRewardEffect huntRewardEffect = (HuntRewardEffect)effect;
		title = T._("[E5C24B]{0}[-] 사냥 성공", huntRewardEffect.TargetAnimal);
		effectType = RewardEffectType.HuntReward;
	}

	private static void DoLevelUpEffect(object effect, out string title, out RewardEffectType effectType)
	{
		LevelUpEffect levelUpEffect = (LevelUpEffect)effect;
		title = T._("[E5C24B]{0:lv:}[-]", levelUpEffect.Level);
		effectType = RewardEffectType.LevelUp;
	}

	private static void DoSkillRewardEffect(object effect, out string title, out RewardEffectType effectType)
	{
		SkillRewardEffect skillRewardEffect = (SkillRewardEffect)effect;
		SkillNode skillNode = GameSystem<SkillSystem>.Instance().FindSkill(skillRewardEffect.LearnedSkill);
		title = T._("[E5C24B]{0} 랭크 {1}[-] 습득", (skillNode != null) ? skillNode.Name : skillRewardEffect.LearnedSkill.SkillId, skillRewardEffect.LearnedSkill.Level);
		effectType = RewardEffectType.SkillReward;
	}

	private static void DoCategoryLevelUpRewardEffect(object effect, out string title, out RewardEffectType effectType)
	{
		CategoryLevelUpRewardEffect categoryLevelUpRewardEffect = (CategoryLevelUpRewardEffect)effect;
		List<Category> list = new List<Category>();
		foreach (KeyValuePair<Category, int> changedLevel in categoryLevelUpRewardEffect.ChangedLevels)
		{
			if (changedLevel.Value > 0)
			{
				list.Add(changedLevel.Key);
			}
		}
		string text;
		if (list.Count == 1)
		{
			text = T._("{0} {1:lv:}", SkillUtil.CategoryLocalizeName(list[0]), categoryLevelUpRewardEffect.ChangedLevels[list[0]]);
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(T._("{0} {1:lv:}", SkillUtil.CategoryLocalizeName(list[i]), categoryLevelUpRewardEffect.ChangedLevels[list[i]]));
			}
			stringBuilder.Append("]");
			text = stringBuilder.ToString();
		}
		title = T._("[E5C24B]{0}[-] 달성", text);
		effectType = RewardEffectType.SkillCategoryLevelUp;
	}

	private static void DoTamingCompletedEffect(object effect, out string title, out RewardEffectType effectType)
	{
		TamingCompletedEffect tamingCompletedEffect = (TamingCompletedEffect)effect;
		KSingleton<AnimalManager>.Instance().AnimalTamed(tamingCompletedEffect.AnimalEntityId, tamingCompletedEffect.Rider);
		title = T._("[E5C24B]{0}[-] 포획", tamingCompletedEffect.Rider.VehicleName);
		PlayerController.PlayRewardMotion(RewardReason.Success);
		effectType = RewardEffectType.LevelUp;
	}

	private static void DoOfferCompletedEffect(object effect, out string title, out RewardEffectType effectType)
	{
		Template template = TemplateFactory.Create(OfferType.Invalid, ((OfferCompletedEffect)effect).Offer);
		title = T._("[E5C24B]{0}[-] 완수", template.TitleText);
		effectType = RewardEffectType.POI;
	}

	private static void DoGetTargetTitleEffect(object effect, out string title, out RewardEffectType effectType)
	{
		GetTargetTitleEffect getTargetTitleEffect = (GetTargetTitleEffect)effect;
		StatisticsData.Title title2 = GameSystem<StatisticsSystem>.Instance().GetTitle(getTargetTitleEffect.TitleId);
		title = T._("[E5C24B]{0}[-] 획득", (title2 == null) ? getTargetTitleEffect.TitleId : title2.Name);
		effectType = RewardEffectType.LevelUp;
	}

	private static void DoFactionLevelUpEffect(object effect, out string title, out RewardEffectType effectType)
	{
		FactionLevelUpEffect factionLevelUpEffect = (FactionLevelUpEffect)effect;
		title = T._("<em>{0}</em>{0:-와} <em>{1}</em>{1:-이} 되었습니다", factionLevelUpEffect.FactionName, factionLevelUpEffect.LevelName);
		effectType = RewardEffectType.SkillCategoryLevelUp;
	}

	private static void DoFactionEventCompletedEffect(object effect, out string title, out RewardEffectType effectType)
	{
		FactionEventCompletedEffect factionEventCompletedEffect = (FactionEventCompletedEffect)effect;
		title = T._("[E5C24B]{0}[-]에게 보상을 받았습니다", factionEventCompletedEffect.FactionName);
		effectType = RewardEffectType.SkillCategoryLevelUp;
	}

	private static void DoExplorePoiEffect(object effect, out string title, out RewardEffectType effectType)
	{
		ExplorePOIEffect explorePOIEffect = (ExplorePOIEffect)effect;
		string text = ((!string.IsNullOrEmpty(explorePOIEffect.PoiName)) ? explorePOIEffect.PoiName : "워프 에너지 불균형점");
		title = T._("[E5C24B]{0}[-] 발견", text);
		effectType = RewardEffectType.POI;
	}

	private static string MakeRewardedComment(Rewarded reward)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (reward.Exp > 0)
		{
			stringBuilder.AppendLine(T._("[E5C24B]+{0}[-] 경험치", reward.Exp));
		}
		foreach (KeyValuePair<Currency, int> item in reward.Currency)
		{
			stringBuilder.AppendLine(ItemSystem.Inventory.CurrencyFormat(item.Value, item.Key));
		}
		if (reward.SkillPoints > 0)
		{
			stringBuilder.AppendLine(T._("+[E5C24B]{0}[-] 스킬 포인트", reward.SkillPoints));
		}
		if (reward.UsableSkillPoints > 0)
		{
			stringBuilder.AppendLine(T._("사용 가능한 스킬포인트: [E5C24B]{0}[-]", reward.UsableSkillPoints));
		}
		if (reward.Effect is SkillRewardEffect)
		{
			SkillRewardEffect skillRewardEffect = (SkillRewardEffect)reward.Effect;
			SkillNode skillNode = GameSystem<SkillSystem>.Instance().FindSkill(skillRewardEffect.LearnedSkill);
			if (skillNode != null)
			{
				for (int i = 0; i < skillNode.Rewards.Length; i++)
				{
					stringBuilder.AppendLine(skillNode.Rewards[i].ToReadableText());
				}
			}
		}
		if (reward.Effect is OfferCompletedEffect)
		{
			OfferCompletedEffect offerCompletedEffect = (OfferCompletedEffect)reward.Effect;
			if (offerCompletedEffect.Offer.Point > 0)
			{
				stringBuilder.AppendLine(T._("<em>{0} 학점</em>을 받았습니다.", offerCompletedEffect.Offer.Point));
			}
			TodoTemplate? newOffer = offerCompletedEffect.NewOffer;
			if (newOffer.HasValue)
			{
				Template template = TemplateFactory.Create(OfferType.Invalid, offerCompletedEffect.NewOffer.Value);
				stringBuilder.AppendLine(T._("<em>{0}</em> 목표를 새로 추천 받았습니다.", template.TitleText));
			}
		}
		if (reward.Titles != null && reward.Titles.Length > 0)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			int j = 0;
			for (int num = reward.Titles.Length; j < num; j++)
			{
				if (j > 0)
				{
					stringBuilder2.Append(", ");
				}
				StatisticsData.Title title = GameSystem<StatisticsSystem>.Instance().GetTitle(reward.Titles[j]);
				stringBuilder2.Append((title != null) ? title.Name : reward.Titles[j]);
			}
			stringBuilder.AppendLine(T._("[E5C24B]{0}[-] 칭호를 획득했습니다.", stringBuilder2));
		}
		if (reward.UnlockedSkills != null && reward.UnlockedSkills.Length > 0)
		{
			StringBuilder stringBuilder3 = new StringBuilder();
			int k = 0;
			for (int num2 = reward.UnlockedSkills.Length; k < num2; k++)
			{
				if (k > 0)
				{
					stringBuilder3.Append(", ");
				}
				SkillNode skillNode2 = GameSystem<SkillSystem>.Instance().FindSkill(reward.UnlockedSkills[k]);
				stringBuilder3.Append(T._("{0} 랭크 {1}", (skillNode2 != null) ? skillNode2.Name : reward.UnlockedSkills[k].SkillId, reward.UnlockedSkills[k].Level));
			}
			stringBuilder.AppendLine(T._("[E5C24B]{0}[-] 스킬이 잠금해제", stringBuilder3));
		}
		if (reward.Abilities != null)
		{
			foreach (KeyValuePair<Basic, int> ability in reward.Abilities)
			{
				if (ability.Value > 0)
				{
					string text = LocalizeUtil.Get(ability.Key);
					stringBuilder.AppendLine(T._("{0} [E5C24B]+ {1}[-]", text, ability.Value));
				}
			}
		}
		if (reward.FriendshipPoint != null)
		{
			Dictionary<FactionType, int>.Enumerator enumerator3 = reward.FriendshipPoint.GetEnumerator();
			while (enumerator3.MoveNext())
			{
				if (enumerator3.Current.Value > 0)
				{
					Yaml.Faction value;
					string text2 = ((!SingletonDict<FactionType, Yaml.Faction>.Instance.TryGetValue(enumerator3.Current.Key, out value)) ? enumerator3.Current.Key.ToString() : ((string)value.name));
					stringBuilder.AppendLine(T._("{0} 우호도 [E5C24B]+ {1}[-]", text2, enumerator3.Current.Value));
				}
			}
		}
		return stringBuilder.ToString().Trim();
	}
}

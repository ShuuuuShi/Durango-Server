using System;
using Durango.Development;
using Durango.Logic;
using Durango.Logic.Quest;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class QuestNodeWidget : MonoBehaviour
{
	private static readonly Color ProgressBlue = new Color32(59, 96, 123, byte.MaxValue);

	private static readonly Color ProgressGray = new Color32(93, 93, 93, byte.MaxValue);

	[SerializeField]
	private RectLayoutComponent _mainLayout;

	[SerializeField]
	private RectLayoutComponent _infoLayout;

	[SerializeField]
	private UISprite _fgSprite;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subscriptionLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _scoreLabel;

	[SerializeField]
	private UILabel _rewardLabel;

	[SerializeField]
	private UILabel _remainTimeLabel;

	[SerializeField]
	private UIProgressBar _progress;

	[SerializeField]
	private UILabel _progressCountLabel;

	[SerializeField]
	private SelectableButton _receiveButton;

	[SerializeField]
	private UIWidget _itemRoot;

	[SerializeField]
	private QuestItemWidget _questItemBase;

	private UIWidget _widget;

	private readonly ListObjectPool<QuestItemWidget> _questItemPool = new ListObjectPool<QuestItemWidget>();

	private QuestToDo _quest;

	private bool _isWaitRewardRequest;

	public string QuestId { get; private set; }

	public Transform GetRecieveButtonTransform()
	{
		return _receiveButton.transform;
	}

	public void Init()
	{
		_widget = GetComponent<UIWidget>();
		_questItemPool.BaseObject = _questItemBase;
		_questItemPool.UseBase = true;
		_questItemPool.Clear();
		SelectableButton receiveButton = _receiveButton;
		receiveButton.Clicked = (Action)Delegate.Combine(receiveButton.Clicked, new Action(OnClickReceiveButton));
		_receiveButton.SetEffect(PresetButton.Effect.Emphasis);
	}

	public void Set(QuestToDo quest)
	{
		_quest = quest;
		QuestId = quest.Id;
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(quest.Id);
		if (questYml != null)
		{
			_iconSprite.spriteName = questYml.Icon;
		}
		_mainLayout.UpdateLayout(_widget.width, _widget.height);
		_receiveButton.ShowLoadingRing(show: false);
		_isWaitRewardRequest = false;
		UpdateProgressAndLabel();
		UpdateQuestRewards();
		UpdateQuestTime();
		UpdateNodeHeight();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void OnClickReceiveButton()
	{
		if (!_isWaitRewardRequest)
		{
			_receiveButton.ShowLoadingRing(show: true);
			_isWaitRewardRequest = true;
			GameSystem<QuestSystem>.Instance().RequestQuestReward(QuestId);
		}
	}

	private void UpdateNodeHeight()
	{
		_infoLayout.UpdateLayout(_infoLayout.ParentWidget.width, null);
		int height = _infoLayout.ParentWidget.height + 60;
		_widget.height = height;
		_mainLayout.UpdateLayout(_widget.width, _widget.height);
	}

	private void UpdateProgressAndLabel()
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(_quest.Id);
		if (questYml != null)
		{
			string text = questYml.Subject;
			_titleLabel.text = ((!_quest.Finished) ? text : $"{text} [247332][icon=icon_autoguidegroup_complete][-]");
			_titleLabel.color = ((!_quest.Finished) ? PresetColor.UIYellow : PresetColor.UILightSilverGray);
			_subscriptionLabel.text = questYml.Description;
		}
		else if (DailyQuestText.TryGet(_quest.Id, out string title, out string description))
		{
			_titleLabel.text = ((!_quest.Finished) ? title : $"{title} [247332][icon=icon_autoguidegroup_complete][-]");
			_titleLabel.color = ((!_quest.Finished) ? PresetColor.UIYellow : PresetColor.UILightSilverGray);
			_subscriptionLabel.text = description;
		}
		else
		{
			_titleLabel.text = _quest.Finished ? $"{_quest.Id} [247332][icon=icon_autoguidegroup_complete][-]" : _quest.Id;
			_titleLabel.color = ((!_quest.Finished) ? PresetColor.UIYellow : PresetColor.UILightSilverGray);
			_subscriptionLabel.text = "เควสตรวจระบบจากเซิร์ฟเวอร์";
		}
		int progress = _quest.Progress;
		int goalCount = _quest.GoalCount;
		_progress.value = Mathf.Min((float)progress / (float)goalCount, 1f);
		_progressCountLabel.text = $"{progress}/{goalCount}";
		_progressCountLabel.UpdateAnchors();
		Color color = (_quest.Finished ? ProgressGray : ((progress < goalCount) ? ProgressBlue : PresetColor.UIYellow));
		_progress.foregroundWidget.color = color;
		_mainLayout.ParentWidget.alpha = ((!_quest.Finished) ? 1f : 0.7f);
		_fgSprite.gameObject.SetActive(_quest.Finished);
	}

	private void UpdateQuestRewards()
	{
		if (!_quest.Reward.HasValue)
		{
			_rewardLabel.text = string.Empty;
			_scoreLabel.text = "0";
			_questItemPool.Clear();
			return;
		}
		_rewardLabel.text = Util.RewardToString(_quest.Reward.Value);
		Messages.RewardItem[] items = _quest.Reward.Value.Items;
		string[] recipeIds = _quest.Reward.Value.RecipeIds;
		string[] blueprintIds = _quest.Reward.Value.BlueprintIds;
		string[] titles = _quest.Reward.Value.Titles;
		int size = KUtility.GetSize(items);
		int size2 = KUtility.GetSize(recipeIds);
		int size3 = KUtility.GetSize(blueprintIds);
		int size4 = KUtility.GetSize(titles);
		_questItemPool.BeginLoad();
		for (int i = 0; i < size; i++)
		{
			QuestItemWidget next = _questItemPool.GetNext();
			next.SetItem(items[i], _quest.Finished);
		}
		for (int j = 0; j < size2; j++)
		{
			QuestItemWidget next2 = _questItemPool.GetNext();
			next2.SetRecipe(recipeIds[j], _quest.Finished);
		}
		for (int k = 0; k < size3; k++)
		{
			QuestItemWidget next3 = _questItemPool.GetNext();
			next3.SetBlueprint(blueprintIds[k], _quest.Finished);
		}
		for (int l = 0; l < size4; l++)
		{
			QuestItemWidget next4 = _questItemPool.GetNext();
			next4.SetTitle(titles[l], _quest.Finished);
		}
		_questItemPool.EndLoad();
		UIBase componentInParent = base.transform.GetComponentInParent<UIBase>();
		if (componentInParent != null && componentInParent.IsPortrait)
		{
			_itemRoot.pivot = UIWidget.Pivot.Left;
			UIUtility.WidgetsReposition(_questItemPool, Vector3.right, Vector3.zero, 10f);
		}
		else
		{
			_itemRoot.pivot = UIWidget.Pivot.TopRight;
			Vector3 basePos = new Vector3(-10f, -20f) + new Vector3(0f, -_questItemBase.height) * 0.5f;
			UIUtility.WidgetsReposition(_questItemPool, Vector3.left, basePos, 10f);
		}
		_itemRoot.gameObject.SetActive(_questItemPool.Count > 0);
		_scoreLabel.text = $"[icon=crown:0.66] {_quest.Reward.Value.QuestScore}";
		_receiveButton.transform.parent.gameObject.SetActive(IsReached() && !_quest.Finished);
	}

	private void UpdateQuestTime()
	{
		if (!double.IsInfinity(_quest.EndAt) && _quest.EndAt > 0.0)
		{
			double remainTime = _quest.EndAt - Connections.Frontend.GetPredictedServerTime();
			if (remainTime < 0.0)
			{
				_remainTimeLabel.SetText(T._("만료"));
				_receiveButton.Disabled = true;
				return;
			}
			_remainTimeLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double endAt = _quest.EndAt;
				string format = T._("{0} 남음");
				string expired = T._("만료");
				string granularity = ((!(remainTime >= 600.0)) ? "sec" : "min");
				remainTime = SyncString.UpdateRemainTimeMsg(endAt, format, out text, out period, expired, 2, granularity);
				if (remainTime <= 0.0)
				{
					_receiveButton.Disabled = true;
				}
			}));
			_receiveButton.Disabled = false;
		}
		else
		{
			_remainTimeLabel.SetText(string.Empty);
			_receiveButton.Disabled = false;
		}
	}

	private bool IsReached()
	{
		int progress = _quest.Progress;
		int goalCount = _quest.GoalCount;
		return progress >= goalCount;
	}

	private void OnClick()
	{
		if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftControl))
		{
			string cheat = $"aqp {_quest.Id} 1";
			Durango.Utils.Singleton<Commands>.Instance().Cheat(cheat);
			return;
		}
		ShowQuestDetailTooltip();
	}

	private void ShowQuestDetailTooltip()
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(_quest.Id);
		string title;
		string description;
		if (questYml != null)
		{
			title = questYml.Subject;
			description = questYml.Description;
		}
		else if (!DailyQuestText.TryGet(_quest.Id, out title, out description))
		{
			title = _quest.Id;
			description = "เควสตรวจระบบจากเซิร์ฟเวอร์";
		}
		int progress = _quest.Progress;
		int goalCount = _quest.GoalCount;
		string progressText = $"{progress}/{goalCount}";
		string statusText = _quest.Finished ? T._("완료") : (progress >= goalCount ? T._("보상 수령 가능") : T._("진행 중"));
		string text = $"{description}\n\n[247332]{progressText}[-]  {statusText}";
		Durango.UI.Popup.WidgetTooltipControl tooltip = UIManager.Popup.Tooltip<Durango.UI.Popup.WidgetTooltipControl>();
		tooltip.Set(title, text, 600, 300);
		tooltip.Show();
	}
}

internal static class DailyQuestText
{
	public static bool TryGet(string id, out string title, out string description)
	{
		title = null;
		description = null;
		switch (id)
		{
			case "daily_survival_rest":
				title = "พักที่จุดพัก";
				description = "นั่งพักที่กองไฟหรือเต็นท์ให้ระบบติดบัพพักและลดความเหนื่อยจริง";
				return true;
			case "daily_local_warp":
				title = "วาปในเกาะ";
				description = "ใช้ warphole ที่สร้างจริงเพื่อย้ายตำแหน่งภายในเกาะ";
				return true;
			case "daily_island_travel":
				title = "ย้ายเกาะที่ท่าเรือ";
				description = "เดินทางผ่านท่าเรือและรอ handoff ไปเซิร์ฟเวอร์เกาะปลายทาง";
				return true;
			default:
				return false;
		}
	}
}

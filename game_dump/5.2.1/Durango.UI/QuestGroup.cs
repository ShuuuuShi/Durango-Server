using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Notification;
using Durango.Logic.Quest;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Quest")]
public class QuestGroup : UIBase, INotificationable
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private QuestMenuTabs _questMenuTabs;

	[SerializeField]
	private QuestBannerWidget _questBannerWidget;

	[SerializeField]
	private QuestMainWidget _questMainWidget;

	[SerializeField]
	private QuestBottomWidget _questBottomWidget;

	[SerializeField]
	private RectLayoutComponent _mainLayout;

	[SerializeField]
	private RectLayoutComponent _contentsLayout;

	private readonly Toggle _notification = new Toggle(Type.Important);

	private StackableAlarm<string, KeyValuePair<string, QuestYml>> _questSucceedAlarm;

	public string SelectedCategory { get; private set; }

	public Notification Notification => _notification;

	public Transform GetQuestMenuTabTransform(string category)
	{
		return _questMenuTabs.GetQuestMenuTab(category);
	}

	public Transform GetQuestReceiveButtonTransform(string questTodoId)
	{
		return _questMainWidget.GetQuestReceiveButtonTransform(questTodoId);
	}

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.Quest;
		SetChildrenActive(activated: false);
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("퀘스트"));
		_questMenuTabs.TabClicked += OnClickQuestTab;
		QuestSystem questSystem = GameSystem<QuestSystem>.Instance();
		questSystem.QuestProceeded += QuestProceeded;
		questSystem.QuestNotificationUpdated += RefreshNotification;
		questSystem.QuestScoreInfosUpdated += QuestScoreInfosUpdated;
		questSystem.ChapterStarted += OnChapterStarted;
		questSystem.QuestCategoryChanged += OnQuestCategoryChanged;
		questSystem.Rewarded += OnRewarded;
		_questSucceedAlarm = new StackableAlarm<string, KeyValuePair<string, QuestYml>>("QuestSucceed", (KeyValuePair<string, QuestYml> pair) => pair.Key, (KeyValuePair<string, QuestYml> pair, int count) => (count <= 1) ? T._("<em>{0}</em> 퀘스트 완료", pair.Value.Subject) : T._("<em>{0}</em> 외 {1}개 퀘스트 완료", pair.Value.Subject, count - 1), "alarm_quest", majorAlarm: true, 1.8f, delegate(KeyValuePair<string, QuestYml> pair)
		{
			if (!base.IsOpened)
			{
				Open(pair.Value.Category);
			}
		});
	}

	protected override bool TryOpen()
	{
		if (string.IsNullOrEmpty(SelectedCategory))
		{
			Category category = GameSystem<QuestSystem>.Instance().VisibleCategories.FirstOrDefault();
			if (category != null)
			{
				SelectedCategory = category.Key;
			}
		}
		if (string.IsNullOrEmpty(SelectedCategory))
		{
			return false;
		}
		bool result = base.TryOpen();
		SelectTab(SelectedCategory);
		return result;
	}

	public void Open(string category)
	{
		if (category == GameSystem<QuestSystem>.Instance().EpicCategory)
		{
			MenuHelper.Open(MenuType.Story);
			return;
		}
		SelectedCategory = category;
		if (base.IsOpened)
		{
			SelectTab(SelectedCategory);
		}
		else
		{
			Open();
		}
	}

	private void UpdateQuestScores()
	{
		Category category = GameSystem<QuestSystem>.Instance().GetCategory(SelectedCategory);
		if (category == null || !category.HasQuestScore.HasValue || category.HasQuestScore.Value)
		{
			_questBottomWidget.BeginLoading();
			GameSystem<QuestSystem>.Instance().GetQuestScoreInfos(SelectedCategory);
		}
		UpdateQuestBottomWidgetActive();
	}

	private void UpdateQuestBottomWidgetActive()
	{
		Category category = GameSystem<QuestSystem>.Instance().GetCategory(SelectedCategory);
		if (category != null && category.HasQuestScore.HasValue && !category.HasQuestScore.Value)
		{
			_questBottomWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_questBottomWidget.gameObject.SetActive(value: true);
		}
		_contentsLayout.ParentWidget.UpdateAnchors();
		_contentsLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void SelectTab(string category)
	{
		SelectedCategory = category;
		Category cat = GameSystem<QuestSystem>.Instance().GetCategory(category);
		Season? season = GameSystem<SeasonSystem>.Instance().GetSeason((cat != null) ? cat.Season : null);
		_questBannerWidget.Set(season);
		_mainLayout.UpdateLayout();
		UpdateQuestScores();
		_questMenuTabs.SelectTab(category);
		if (cat == null)
		{
			_questMainWidget.Set(null, reset: true);
			return;
		}
		bool loaded = false;
		cat.GetQuestList(delegate(List<QuestToDo> quests)
		{
			loaded = true;
			if (!(cat.Key != SelectedCategory))
			{
				_questMainWidget.Set(quests, reset: true);
			}
		});
		if (!loaded)
		{
			_questMainWidget.ShowLoading();
		}
	}

	private void OnClickQuestTab(string category)
	{
		SelectTab(category);
	}

	private void QuestProceeded(NotifyQuestProceed msg)
	{
		if (msg.Progress >= msg.GoalCount)
		{
			QuestYml questYml = SingletonDict<string, QuestYml>.Instance.Get(msg.QuestId);
			if (questYml != null && string.IsNullOrEmpty(questYml.ChapterSubject) && (questYml.AutoFinish || !msg.Finished))
			{
				_questSucceedAlarm.Add(new KeyValuePair<string, QuestYml>(msg.QuestId, questYml));
			}
		}
	}

	private void QuestScoreInfosUpdated(QuestScoreInfos questScoreInfos)
	{
		if (base.IsOpened && !(questScoreInfos.Category != SelectedCategory))
		{
			_questBottomWidget.UpdateScoreInfo(questScoreInfos);
			UpdateQuestBottomWidgetActive();
		}
	}

	private void RefreshNotification(bool hasNotification)
	{
		_notification.On = hasNotification;
		_questMenuTabs.UpdateNotification();
	}

	private void OnChapterStarted(string questId)
	{
		if (base.IsOpened)
		{
			ForceClose();
		}
	}

	private void OnQuestCategoryChanged(string category)
	{
		if (!(SelectedCategory != category))
		{
			Category category2 = GameSystem<QuestSystem>.Instance().GetCategory(category);
			if (category2 == null)
			{
				_questMainWidget.Set(null, reset: true);
			}
			else
			{
				_questMainWidget.Set(category2.GetCachedQuestList(), reset: false);
			}
		}
	}

	private void OnRewarded(QuestRewardResults result)
	{
		QuestYml questYml = SingletonDict<string, QuestYml>.Get(result.QuestId);
		if (questYml != null && questYml.LastQuest)
		{
			UIManager.FindScript<ChapterGroup>().Show(result);
		}
		else
		{
			UIManager.Popup.FindTooltip<ReceiveRewardsPopup>().ShowQuestRewarded(result);
		}
	}
}

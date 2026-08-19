using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Notification;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

[Uri("LearningGuide")]
public class LearningGuideGroup : UIBase, INotificationable
{
	[Serializable]
	public struct LearningTagOption
	{
		public Color BackgroundColor;

		public SpriteData SpriteData;

		public bool ShowEffect;
	}

	[Serializable]
	[EnumType(typeof(Learning))]
	public class LearningTagOptions : EnumKeyList
	{
		[SerializeField]
		private List<LearningTagOption> _values;

		public LearningTagOption Get(Learning type)
		{
			int num = IndexOf((int)type);
			if (num == -1)
			{
				return default(LearningTagOption);
			}
			return _values[num];
		}
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private LearningCategoryListWidget _categoryListWidget;

	[SerializeField]
	private SubCategoryListWidget _subCategoryListWidget;

	[SerializeField]
	private SubjectInfoWidget _subjectInfoWidget;

	[SerializeField]
	private LearningTagOptions _learningTagOptions;

	private readonly Notification _notification = new Toggle(Durango.Logic.Notification.Type.Important);

	public Notification Notification => _notification;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Default;
		_titleWidget.Object.SetTitle(T._("진로 가이드"));
		_subCategoryListWidget.Init();
		_subjectInfoWidget.Init();
		_categoryListWidget.SelectionChanged += CategoryListWidgetSelectionChanged;
		LearningGuideSystem learningGuideSystem = GameSystem<LearningGuideSystem>.Instance();
		learningGuideSystem.AchievedInfoUpdated += LearningGuideSystem_AchievedInfoUpdated;
		learningGuideSystem.TargetAdviceUpdated += LearningGuideSystem_TargetAdviceUpdated;
		GameSystem<SkillSystem>.Instance().SkillListUpdated += SkillSystem_SkillListUpdated;
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged += SkillSystem_CategoryLevelChanged;
		base.VisibleController.Changed += LearningGuideGroup_OnVisible;
		TryClose();
	}

	public void CloseAndRedirectToSkillNode(Node skillNode)
	{
		SkillGroup skillGroup = UIManager.FindScript<SkillGroup>();
		bool softOpen = skillGroup.SoftOpen;
		skillGroup.SoftOpen = false;
		skillGroup.Open(skillNode);
		skillGroup.SoftOpen = softOpen;
	}

	public LearningTagOption GetLearningTagOption(Learning state)
	{
		return _learningTagOptions.Get(state);
	}

	public void SelectSubject(Advice subject, bool moveTo = false)
	{
		_subCategoryListWidget.SelectSubject(subject, moveTo);
		_subjectInfoWidget.SetSubject(subject);
	}

	[Uri("Help")]
	private void ShowHintPopup(string id)
	{
		Advice advice = GameSystem<StatisticsSystem>.Instance().GetAdvice(id);
		if (advice != null)
		{
			ShowHintPopup(advice);
		}
	}

	private void ShowHintPopup(Advice subject)
	{
		CardNewsPopup cardNewsPopup = UIManager.Popup.Tooltip<CardNewsPopup>();
		string id = subject.Id;
		if (cardNewsPopup.Load(id))
		{
			cardNewsPopup.Show();
			return;
		}
		Gettext[] hints = subject.GetHints();
		SimpleTextListPopup simpleTextListPopup = UIManager.Popup.Tooltip<SimpleTextListPopup>();
		simpleTextListPopup.Set(subject.Name, hints?.Select((Gettext o) => o.ToString()).ToArray());
		simpleTextListPopup.Show();
	}

	public static void ShowRewardPopupWidget(Advice subject, bool isRewarded)
	{
		ReceiveRewardsPopup receiveRewardsPopup = UIManager.Popup.FindTooltip<ReceiveRewardsPopup>();
		receiveRewardsPopup.ShowAdviceReward(subject, isRewarded);
		if (isRewarded)
		{
			GameSystem<LearningGuideSystem>.Instance().ReceiveReward(subject.RewardTitleId);
		}
	}

	private void CategoryListWidgetSelectionChanged(AdviceCategory category)
	{
		_subCategoryListWidget.SetCategory(category);
		Advice advice = GameSystem<StatisticsSystem>.Instance().GetAdvice(category, 0);
		SelectSubject(advice, moveTo: true);
	}

	private void LearningGuideSystem_AchievedInfoUpdated()
	{
		if (base.IsOpened)
		{
			_subCategoryListWidget.Refresh();
			_subjectInfoWidget.RefreshAchievedInfo();
		}
		RefreshNotification();
	}

	private void SkillSystem_CategoryLevelChanged(Category category)
	{
		if (base.IsOpened)
		{
			_subCategoryListWidget.Refresh();
			_subjectInfoWidget.RefreshAchievedInfo();
		}
		RefreshNotification();
	}

	private void LearningGuideSystem_TargetAdviceUpdated()
	{
		Advice targetAdvice = GameSystem<LearningGuideSystem>.Instance().TargetAdvice;
		if (targetAdvice != null && base.IsOpened)
		{
			UIManager.SystemMsg(T._("목표가 {0:으로} 설정되었습니다.", targetAdvice.Name));
			ShowHintPopup(targetAdvice);
		}
		if (base.IsOpened)
		{
			_subCategoryListWidget.Refresh();
			_subjectInfoWidget.RefreshLearningState();
		}
		RefreshNotification();
	}

	private void SkillSystem_SkillListUpdated()
	{
		if (base.IsOpened)
		{
			_subjectInfoWidget.RefreshSkills();
		}
		RefreshNotification();
	}

	public override bool Open()
	{
		RefreshSelectedSubject();
		RefreshNotification();
		UpdateAchivementInfo();
		return base.Open();
	}

	private void LearningGuideGroup_OnVisible(bool visible)
	{
		if (visible)
		{
			UpdateAchivementInfo();
		}
	}

	private static void UpdateAchivementInfo()
	{
		GameSystem<LearningGuideSystem>.Instance().UpdateAchievementInfo();
	}

	private void RefreshSelectedSubject()
	{
		LearningGuideSystem learningGuideSystem = GameSystem<LearningGuideSystem>.Instance();
		Advice subject;
		if (learningGuideSystem.TargetAdvice != null)
		{
			AdviceCategory adviceCategory = GameSystem<StatisticsSystem>.Instance().GetAdviceCategory(learningGuideSystem.TargetAdvice.Category);
			_categoryListWidget.SetSelectedCategory(adviceCategory);
			subject = learningGuideSystem.TargetAdvice;
		}
		else
		{
			_categoryListWidget.SetSelectedCategory(GameSystem<StatisticsSystem>.Instance().GetAdviceCategory(0));
			subject = GameSystem<StatisticsSystem>.Instance().GetAdvice(0);
		}
		SelectSubject(subject, moveTo: true);
	}

	private void RefreshNotification()
	{
		_categoryListWidget.RefreshNotification();
		_subCategoryListWidget.RefreshNotification();
		Notification.BeginSetting();
		Notification.Type = _categoryListWidget.NotificationType;
		Notification.On = _categoryListWidget.NotificationOn;
		Notification.EndSetting();
	}
}

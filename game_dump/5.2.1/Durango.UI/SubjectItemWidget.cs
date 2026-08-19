using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Notification;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class SubjectItemWidget : SelectableWidget
{
	[SerializeField]
	private SkillLearningTag _progressTag;

	[SerializeField]
	private GameObject _iconCompleted;

	[SerializeField]
	private GameObject _lockedIcon;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private UISprite _processRatioSprite;

	[SerializeField]
	private UILabel _processRatioLabel;

	[SerializeField]
	private UISprite _notification;

	public Advice Subject { get; private set; }

	public bool IsCompleted
	{
		get
		{
			return _iconCompleted.activeSelf;
		}
		set
		{
			if (IsCompleted != value)
			{
				_iconCompleted.SetActive(value);
			}
		}
	}

	public void SetSubject(Advice subject)
	{
		if (Subject != subject)
		{
			Subject = subject;
			_text.text = ((!subject.Recommended) ? subject.Name : string.Format("{0} {1}", T._("[추천]"), subject.Name));
			base.Selected = false;
		}
		RefreshAchievedInfo();
		RefreshNotification();
	}

	public void RefreshAchievedInfo()
	{
		if (Subject == null)
		{
			return;
		}
		LearningGuideSystem learningGuideSystem = GameSystem<LearningGuideSystem>.Instance();
		AdviceAchievement achievementState = learningGuideSystem.GetAchievementState(Subject.Id);
		if (achievementState != null)
		{
			IsCompleted = achievementState.Complete;
			bool flag = learningGuideSystem.IsSubjectLocked(Subject);
			if (Subject == learningGuideSystem.TargetAdvice)
			{
				_progressTag.gameObject.SetActive(value: true);
				_progressTag.Refresh((!achievementState.Achieved) ? Learning.InProgress : Learning.Learned);
			}
			else
			{
				_progressTag.gameObject.SetActive(value: false);
			}
			_iconCompleted.SetActive(IsCompleted);
			_lockedIcon.SetActive(flag);
			bool flag2 = !IsCompleted && !flag;
			_processRatioSprite.gameObject.SetActive(flag2);
			_processRatioLabel.gameObject.SetActive(flag2);
			if (flag2)
			{
				float ratio = achievementState.Ratio;
				_processRatioSprite.fillAmount = ratio;
				_processRatioLabel.text = $"{ratio:P0}";
			}
		}
	}

	public void RefreshNotification()
	{
		AdviceAchievement achievementState = GameSystem<LearningGuideSystem>.Instance().GetAchievementState(Subject.Id);
		Type type = Type.Important;
		bool active = false;
		if (achievementState != null && achievementState.CanReward)
		{
			active = true;
		}
		else
		{
			Advice targetAdvice = GameSystem<LearningGuideSystem>.Instance().TargetAdvice;
			if (targetAdvice != null && targetAdvice.Id == Subject.Id && GameSystem<LearningGuideSystem>.Instance().HasLearnableSkillForCurrentTitle())
			{
				type = Type.Normal;
				active = true;
			}
		}
		_notification.color = Notification.GetTypeColor(type);
		_notification.gameObject.SetActive(active);
	}

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}
}

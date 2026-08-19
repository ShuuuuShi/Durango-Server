using System;
using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class SubjectTitleWidget : MonoBehaviour
{
	public enum Mode
	{
		None,
		Locked,
		Learning,
		CanReceiveReward,
		ReceivedReward
	}

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UISprite _progressRatioSprite;

	[SerializeField]
	private UILabel _progressRatioText;

	[SerializeField]
	private GameObject _completedIcon;

	[SerializeField]
	private SelectableButton _guideButton;

	[SerializeField]
	private GameObject _lockedIcon;

	[SerializeField]
	private UILabel _completedLabel;

	private Mode _mode;

	private Advice _subject;

	private bool _initialized;

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_guideButton.Init();
			SelectableButton guideButton = _guideButton;
			guideButton.Clicked = (Action)Delegate.Combine(guideButton.Clicked, new Action(GuideButtonClicked));
		}
	}

	public void SetSubject(Advice subject)
	{
		_subject = subject;
		_titleLabel.text = $"{_subject.Name} [preset=em_information?ui://LearningGuide/Help/{_subject.Id}]";
		RefreshMode();
		RefreshAchievedInfo();
	}

	public void SetMode(Mode mode)
	{
		_mode = mode;
		switch (_mode)
		{
		case Mode.None:
			_guideButton.gameObject.SetActive(value: true);
			_guideButton.Text = T._("가이드 시작");
			_guideButton.SetStyle(PresetButton.Style.Border);
			_guideButton.SetEffect(PresetButton.Effect.None);
			_completedLabel.gameObject.SetActive(value: false);
			break;
		case Mode.Locked:
			_guideButton.gameObject.SetActive(value: false);
			_completedLabel.gameObject.SetActive(value: false);
			break;
		case Mode.Learning:
			_guideButton.gameObject.SetActive(value: true);
			_guideButton.Text = T._("가이드 취소");
			_guideButton.SetStyle(PresetButton.Style.Border);
			_guideButton.SetEffect(PresetButton.Effect.None);
			_completedLabel.gameObject.SetActive(value: false);
			break;
		case Mode.CanReceiveReward:
			_guideButton.gameObject.SetActive(value: true);
			_guideButton.Text = T._("가이드 완료");
			_guideButton.SetStyle(PresetButton.Style.Solid);
			_guideButton.SetEffect(PresetButton.Effect.Emphasis);
			_completedLabel.gameObject.SetActive(value: false);
			break;
		case Mode.ReceivedReward:
			_guideButton.gameObject.SetActive(value: false);
			_completedLabel.gameObject.SetActive(value: true);
			break;
		}
	}

	public void RefreshMode()
	{
		if (_subject == null)
		{
			return;
		}
		LearningGuideSystem learningGuideSystem = GameSystem<LearningGuideSystem>.Instance();
		AdviceAchievement achievementState = learningGuideSystem.GetAchievementState(_subject.Id);
		if (achievementState != null)
		{
			bool flag = GameSystem<LearningGuideSystem>.Instance().IsSubjectLocked(_subject);
			if (achievementState.Complete)
			{
				SetMode(Mode.ReceivedReward);
			}
			else if (achievementState.Achieved)
			{
				SetMode(Mode.CanReceiveReward);
			}
			else if (learningGuideSystem.TargetAdvice == _subject)
			{
				SetMode(Mode.Learning);
			}
			else if (flag)
			{
				SetMode(Mode.Locked);
			}
			else
			{
				SetMode(Mode.None);
			}
		}
	}

	public void RefreshAchievedInfo()
	{
		if (_subject == null)
		{
			return;
		}
		AdviceAchievement achievementState = GameSystem<LearningGuideSystem>.Instance().GetAchievementState(_subject.Id);
		if (achievementState != null)
		{
			bool flag = GameSystem<LearningGuideSystem>.Instance().IsSubjectLocked(_subject);
			_completedIcon.SetActive(achievementState.Complete);
			_lockedIcon.SetActive(flag);
			bool flag2 = !achievementState.Complete && !flag;
			_progressRatioSprite.gameObject.SetActive(flag2);
			_progressRatioText.gameObject.SetActive(flag2);
			if (flag2)
			{
				_progressRatioSprite.fillAmount = achievementState.Ratio;
				_progressRatioText.text = $"{achievementState.Ratio:P0}";
			}
		}
	}

	private void GuideButtonClicked()
	{
		switch (_mode)
		{
		case Mode.None:
			GameSystem<LearningGuideSystem>.Instance().SelectCurriculum(_subject);
			break;
		case Mode.Learning:
			GameSystem<LearningGuideSystem>.Instance().CancelCurriculum(_subject);
			break;
		case Mode.CanReceiveReward:
			ShowRewardPopup();
			break;
		}
	}

	private void ShowRewardPopup()
	{
		LearningGuideGroup.ShowRewardPopupWidget(_subject, isRewarded: true);
	}
}

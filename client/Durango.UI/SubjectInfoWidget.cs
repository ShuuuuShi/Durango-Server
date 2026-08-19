using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Shared.Skill;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class SubjectInfoWidget : MonoBehaviour
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private SubjectTitleWidget _subjectTitleWidget;

	[SerializeField]
	private SubjectDetailWidget _subjectDetailWidget;

	[SerializeField]
	private LessonListWidget _lessonListWidget;

	[SerializeField]
	private UILabel _lockedLabel;

	private bool _initialized;

	private Durango.Logic.LearningGuide.Advice _subject;

	public void Init()
	{
		if (!_initialized)
		{
			_subjectTitleWidget.Init();
			_subjectDetailWidget.Init();
			_lessonListWidget.Init();
			_initialized = true;
		}
	}

	public void SetSubject([CanBeNull] Durango.Logic.LearningGuide.Advice subject)
	{
		bool flag = _subject != subject;
		_subject = subject;
		if (subject != null)
		{
			_subjectTitleWidget.SetSubject(subject);
			_subjectDetailWidget.SetSubject(subject);
			if (GameSystem<LearningGuideSystem>.Instance().IsSubjectLocked(subject))
			{
				_lockedLabel.gameObject.SetActive(value: true);
				_lessonListWidget.gameObject.SetActive(value: false);
				RequiredSkill requiredSkill = subject.RequiredSkill();
				_lockedLabel.text = ((requiredSkill.skill_category != Category.Invalid) ? T._("{0} {1:lv:}에 안내 받을 수 있습니다.", requiredSkill.skill_category.GetName(), requiredSkill.level) : string.Empty);
			}
			else
			{
				_lockedLabel.gameObject.SetActive(value: false);
				_lessonListWidget.gameObject.SetActive(value: true);
				_lessonListWidget.SetSubject(subject);
			}
			_scrollView.Reposition(flag, !flag);
		}
	}

	public void RefreshLearningState()
	{
		_subjectTitleWidget.RefreshMode();
		_lessonListWidget.RefreshSkills();
	}

	public void RefreshAchievedInfo()
	{
		_subjectTitleWidget.RefreshAchievedInfo();
		_subjectTitleWidget.RefreshMode();
		_lessonListWidget.RefreshSkills();
	}

	public void RefreshSkills()
	{
		_lessonListWidget.RefreshSkills();
	}
}

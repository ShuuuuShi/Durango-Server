using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Skill;
using JetBrains.Annotations;
using Shared.Skill;
using UnityEngine;

namespace Durango.UI;

public class LessonItemWidget : MonoBehaviour
{
	public class Lesson
	{
		public Shared.Skill.Category Category { get; private set; }

		public List<Node> RequiredSkills { get; private set; }

		public Lesson(Shared.Skill.Category category)
		{
			Category = category;
			RequiredSkills = new List<Node>();
		}

		public void AddSkill([NotNull] Node skillNode)
		{
			RequiredSkills.Add(skillNode);
		}
	}

	private const int MinSkillWidgetCount = 2;

	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private UIWidget _categoryPane;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _textName;

	[SerializeField]
	private LearningSkillWidget _baseSkillWidget;

	private bool _initialized;

	private int _defaultWidgetHeight;

	private int _defaultCategoryPaneHeight;

	private int _skillWidgetHeight;

	private readonly ListObjectPool<LearningSkillWidget> _learningSkillWidgets = new ListObjectPool<LearningSkillWidget>();

	public Shared.Skill.Category Category { get; private set; }

	public void Init()
	{
		if (!_initialized)
		{
			_defaultWidgetHeight = _widget.height;
			_defaultCategoryPaneHeight = _categoryPane.height;
			_skillWidgetHeight = _baseSkillWidget.GetComponent<UIWidget>().height;
			_learningSkillWidgets.BaseObject = _baseSkillWidget;
			_learningSkillWidgets.Init(delegate(LearningSkillWidget skillWidget)
			{
				skillWidget.Init();
			});
			Category = Shared.Skill.Category.Invalid;
			_initialized = true;
		}
	}

	public void SetLesson([NotNull] Advice subject, Lesson lesson)
	{
		Category = lesson.Category;
		_icon.spriteName = Util.CategoryIcon(Category);
		_textName.text = LocalizeUtil.Get(Category);
		LearningGuideSystem learningGuideSystem = GameSystem<LearningGuideSystem>.Instance();
		SetSkills(lesson, learningGuideSystem.TargetAdvice == subject);
	}

	public void RefreshSkills([NotNull] Advice subject)
	{
		bool clickable = GameSystem<LearningGuideSystem>.Instance().TargetAdvice == subject;
		for (int i = 0; i < _learningSkillWidgets.Count; i++)
		{
			_learningSkillWidgets[i].Refresh(clickable);
		}
	}

	private LearningSkillWidget CreateNewSkillWidget(Vector3 pos, bool showUpperDotLine)
	{
		LearningSkillWidget learningSkillWidget = _learningSkillWidgets.Add();
		learningSkillWidget.transform.localPosition = pos;
		learningSkillWidget.ShowUpperDotLine = showUpperDotLine;
		return learningSkillWidget;
	}

	private void SetSkills(Lesson lesson, bool clickable)
	{
		_learningSkillWidgets.Clear();
		Vector3 localPosition = _baseSkillWidget.transform.localPosition;
		bool showUpperDotLine = false;
		foreach (Node requiredSkill in lesson.RequiredSkills)
		{
			CreateNewSkillWidget(localPosition, showUpperDotLine).SetSkill(requiredSkill, clickable);
			localPosition += Vector3.down * _skillWidgetHeight;
			showUpperDotLine = true;
		}
		if (_learningSkillWidgets.Count < 2)
		{
			for (int i = _learningSkillWidgets.Count; i < 2; i++)
			{
				CreateNewSkillWidget(localPosition, showUpperDotLine).SetSkill();
				localPosition += Vector3.down * _skillWidgetHeight;
				showUpperDotLine = true;
			}
		}
		_categoryPane.height = _learningSkillWidgets.Count * _skillWidgetHeight;
		_widget.height = _defaultWidgetHeight + (_categoryPane.height - _defaultCategoryPaneHeight);
	}
}

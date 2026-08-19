using System.Collections.Generic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Skill;
using JetBrains.Annotations;
using Shared.Skill;
using UnityEngine;

namespace Durango.UI;

public class LessonListWidget : MonoBehaviour
{
	[SerializeField]
	private LessonItemWidget _nodeBase;

	private ListObjectPool<LessonItemWidget> _nodes;

	private bool _initialized;

	private Advice _subject;

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_nodes = new ListObjectPool<LessonItemWidget>();
			_nodes.BaseObject = _nodeBase;
			_nodes.Init(delegate(LessonItemWidget w)
			{
				w.Init();
			});
		}
	}

	public void SetSubject([NotNull] Advice subject)
	{
		_subject = subject;
		Dictionary<Shared.Skill.Category, LessonItemWidget.Lesson> dictionary = new Dictionary<Shared.Skill.Category, LessonItemWidget.Lesson>();
		int num = _subject.SkillsCount();
		for (int i = 0; i < num; i++)
		{
			Node skill = _subject.GetSkill(i);
			if (skill != null)
			{
				if (!dictionary.TryGetValue(skill.Category, out var value))
				{
					value = new LessonItemWidget.Lesson(skill.Category);
					dictionary[skill.Category] = value;
				}
				value.AddSkill(skill);
			}
		}
		_nodes.BeginLoad();
		foreach (KeyValuePair<Shared.Skill.Category, LessonItemWidget.Lesson> item in dictionary)
		{
			_nodes.GetNext().SetLesson(_subject, item.Value);
		}
		_nodes.EndLoad();
		UIWidget component = GetComponent<UIWidget>();
		float num2 = UIUtility.WidgetsReposition(_nodes, component, Vector3.down);
		component.height = (int)num2;
		UIUtility.UpdateAnchors(base.transform);
	}

	public void RefreshSkills()
	{
		foreach (LessonItemWidget node in _nodes)
		{
			node.RefreshSkills(_subject);
		}
		UIUtility.UpdateAnchors(base.transform);
	}
}

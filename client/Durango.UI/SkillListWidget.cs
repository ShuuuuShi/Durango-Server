using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Shared.Skill;
using UnityEngine;

namespace Durango.UI;

public class SkillListWidget : MonoBehaviour
{
	[SerializeField]
	private KeyValueLabel _titleLabel;

	[SerializeField]
	private KScrollView _skillList;

	private readonly List<Group> _skillGroups = new List<Group>();

	public event Action<Bundle> SkillSelected;

	public event Action<Group> SkillGroupSelected;

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnSkillListUpdate;
		UIPanel panel = _skillList.Panel;
		if (UIManager.IsPortraitWidget(base.gameObject))
		{
			_titleLabel.gameObject.SetActive(value: false);
			_skillList.ScrollView.movement = UIScrollView.Movement.Horizontal;
			_skillList.ScrollView.contentPivot = UIWidget.Pivot.Left;
			panel.topAnchor.absolute = -3;
		}
		else
		{
			_titleLabel.gameObject.SetActive(value: true);
			_skillList.ScrollView.movement = UIScrollView.Movement.Vertical;
			_skillList.ScrollView.contentPivot = UIWidget.Pivot.Top;
			panel.topAnchor.absolute = -_titleLabel.Widget.height;
		}
		_skillList.ResetPosition();
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnSkillListUpdate;
	}

	private void OnSkillListUpdate()
	{
		ListObjectPool nodes = _skillList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			SkillListNode component = nodes[i].GetComponent<SkillListNode>();
			component.UpdateData();
		}
	}

	public SkillListNode GetSkillListNode(string subCategory)
	{
		int num = IndexOf(subCategory);
		if (num < 0 || num >= _skillList.Nodes.Count)
		{
			return null;
		}
		return _skillList.Nodes.Get<SkillListNode>(num);
	}

	public void Set(Shared.Skill.Category cat, string group)
	{
		if (cat == Shared.Skill.Category.Invalid)
		{
			return;
		}
		_titleLabel.Set($"[icon={Util.CategoryIcon(cat)}:1.5]", LocalizeUtil.FormatLevel(GameSystem<SkillSystem>.Instance().GetCategoryLevel(cat)));
		_titleLabel.UpdateLayout(GetComponent<UIWidget>().width);
		_skillGroups.Clear();
		List<Bundle> skills = GameSystem<SkillSystem>.Instance().Skills;
		for (int i = 0; i < skills.Count; i++)
		{
			if (skills[i].Category != cat)
			{
				continue;
			}
			string group2 = skills[i].Group;
			if (string.IsNullOrEmpty(group2))
			{
				_skillGroups.Add(new Group
				{
					Skill = skills[i]
				});
				continue;
			}
			int num = IndexOf(skills[i].Group);
			Group group3;
			if (num == -1)
			{
				group3 = new Group();
				group3.Name = group2;
				group3.Skills = new List<Bundle>();
				_skillGroups.Add(group3);
			}
			else
			{
				group3 = _skillGroups[num];
			}
			group3.Skills.Add(skills[i]);
		}
		for (int j = 0; j < _skillGroups.Count; j++)
		{
			_skillGroups[j].Sort();
		}
		_skillGroups.Sort(SkillBundleComparison);
		ListObjectPool nodes = _skillList.Nodes;
		nodes.Init(OnInitSkillListNode);
		nodes.Set(_skillGroups.Count);
		int num2 = -1;
		for (int k = 0; k < _skillGroups.Count; k++)
		{
			SkillListNode component = nodes[k].GetComponent<SkillListNode>();
			Group group4 = _skillGroups[k];
			component.Set(group4);
			if (num2 == -1 && group == group4.Name)
			{
				num2 = k;
			}
		}
		_skillList.Reposition(resetPosition: true, tween: false);
		OnSelectSkillGroup((num2 != -1) ? _skillGroups[num2] : _skillGroups[0]);
	}

	private int IndexOf(string subCategory)
	{
		for (int i = 0; i < _skillGroups.Count; i++)
		{
			if (_skillGroups[i].Name == subCategory)
			{
				return i;
			}
		}
		return -1;
	}

	private static int SkillBundleComparison(Group s1, Group s2)
	{
		int learnableCount = s1.GetLearnableCount();
		int learnableCount2 = s2.GetLearnableCount();
		int num = learnableCount2 - learnableCount;
		if (num != 0)
		{
			return num;
		}
		int num2 = s1.HighestLevel();
		int num3 = s2.HighestLevel();
		num = num3 - num2;
		if (num == 0)
		{
			int num4 = s1.NearestNextAvailableCategoryLevel();
			int num5 = s2.NearestNextAvailableCategoryLevel();
			num = num4 - num5;
			if (num == 0)
			{
				int renderPrioirty = s1.RenderPrioirty;
				int renderPrioirty2 = s2.RenderPrioirty;
				num = renderPrioirty - renderPrioirty2;
			}
		}
		return num;
	}

	private void OnInitSkillListNode(GameObject obj)
	{
		SkillListNode component = obj.GetComponent<SkillListNode>();
		component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickSkillListNode));
	}

	private void OnClickSkillListNode()
	{
		SkillListNode skillListNode = Selectable.Current as SkillListNode;
		if (!(skillListNode == null) && !skillListNode.Selected)
		{
			OnSelectSkillGroup(skillListNode.Group);
		}
	}

	private void OnSelectSkillGroup(Group group)
	{
		ListObjectPool nodes = _skillList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			SkillListNode component = nodes[i].GetComponent<SkillListNode>();
			component.Selected = group == component.Group;
		}
		if (group == null)
		{
			return;
		}
		if (group.Skill == null)
		{
			if (this.SkillGroupSelected != null)
			{
				this.SkillGroupSelected(group);
			}
		}
		else if (this.SkillSelected != null)
		{
			this.SkillSelected(group.Skill);
		}
	}
}

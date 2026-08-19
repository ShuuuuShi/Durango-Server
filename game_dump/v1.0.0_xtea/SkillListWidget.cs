using System;
using System.Collections.Generic;
using Shared.Skill;
using SkillData;
using UnityEngine;

public class SkillListWidget : MonoBehaviour
{
	public class SkillGroup
	{
		public string Name;

		public List<SkillBundle> Skills;

		public SkillBundle Skill;

		public int RenderPrioirty => (Skill != null) ? Skill.RenderPriority : int.MaxValue;

		public int GetLearnableCount()
		{
			if (Skill != null)
			{
				return Skill.GetLearnableCount();
			}
			int num = 0;
			int i = 0;
			for (int size = KUtility.GetSize(Skills); i < size; i++)
			{
				num += Skills[i].GetLearnableCount();
			}
			return num;
		}

		public int HighestLevel()
		{
			int num = 0;
			int i = 0;
			for (int num2 = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num2; i++)
			{
				SkillBundle skillBundle = ((Skill != null) ? Skill : Skills[i]);
				int num3 = skillBundle.HighestLevel();
				if (num3 > num)
				{
					num = num3;
				}
			}
			return num;
		}

		public int NearestNextAvailableCategoryLevel()
		{
			int num = 1000000;
			int i = 0;
			for (int num2 = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num2; i++)
			{
				SkillBundle skillBundle = ((Skill != null) ? Skill : Skills[i]);
				int num3 = skillBundle.NearestNextAvailableCategoryLevel();
				if (num3 < num)
				{
					num = num3;
				}
			}
			return num;
		}

		public bool HasNew()
		{
			int i = 0;
			for (int num = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num; i++)
			{
				SkillBundle skillBundle = ((Skill != null) ? Skill : Skills[i]);
				if (skillBundle.HasNew())
				{
					return true;
				}
			}
			return false;
		}

		public void Readed()
		{
			int i = 0;
			for (int num = ((Skill != null) ? 1 : KUtility.GetSize(Skills)); i < num; i++)
			{
				SkillBundle skillBundle = ((Skill != null) ? Skill : Skills[i]);
				int j = 0;
				for (int num2 = KUtility.GetSize(skillBundle.Sub) + 1; j < num2; j++)
				{
					Skill skill = ((j != 0) ? skillBundle.Sub[j - 1] : skillBundle.Base);
					for (int k = 0; k < skill.MaxLevel; k++)
					{
						int level = k + 1;
						skill.Get(level).IsNew = false;
					}
				}
			}
		}

		public void Sort()
		{
			if (Skills != null)
			{
				Skills.Sort(Comparison);
			}
		}

		private int Comparison(SkillBundle s1, SkillBundle s2)
		{
			int num = s1.RenderPriority - s2.RenderPriority;
			if (num == 0)
			{
				num = s1.Base.Get(1).CategoryLevel - s2.Base.Get(1).CategoryLevel;
			}
			return num;
		}
	}

	[SerializeField]
	private KeyValueLabel _titleLabel;

	[SerializeField]
	private KScrollView _skillList;

	private readonly List<SkillGroup> _skillGroups = new List<SkillGroup>();

	public event Action<SkillBundle> SkillSelected;

	public event Action<IList<SkillBundle>> SkillGroupSelected;

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnSkillListUpdate;
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

	public void Set(Category cat)
	{
		if (cat == Category.Invalid)
		{
			return;
		}
		_titleLabel.Set($"[{SkillUtil.CategoryIcon(cat)}:1.5]", LocalizeUtil.FormatLevel(GameSystem<SkillSystem>.Instance().GetCategoryLevel(cat)));
		_titleLabel.UpdateLayout(((Component)this).GetComponent<UIWidget>().width);
		_skillGroups.Clear();
		List<SkillBundle> skills = GameSystem<SkillSystem>.Instance().Skills;
		for (int i = 0; i < skills.Count; i++)
		{
			if (skills[i].Category != cat)
			{
				continue;
			}
			string group = skills[i].Group;
			if (string.IsNullOrEmpty(group))
			{
				_skillGroups.Add(new SkillGroup
				{
					Skill = skills[i]
				});
				continue;
			}
			int num = IndexOf(skills[i].Group);
			SkillGroup skillGroup;
			if (num == -1)
			{
				skillGroup = new SkillGroup();
				skillGroup.Name = group;
				skillGroup.Skills = new List<SkillBundle>();
				_skillGroups.Add(skillGroup);
			}
			else
			{
				skillGroup = _skillGroups[num];
			}
			skillGroup.Skills.Add(skills[i]);
		}
		for (int j = 0; j < _skillGroups.Count; j++)
		{
			_skillGroups[j].Sort();
		}
		_skillGroups.Sort(SkillBundleComparison);
		ListObjectPool nodes = _skillList.Nodes;
		nodes.Init(OnInitSkillListNode);
		nodes.Set(_skillGroups.Count);
		for (int k = 0; k < _skillGroups.Count; k++)
		{
			SkillListNode component = nodes[k].GetComponent<SkillListNode>();
			component.Set(_skillGroups[k]);
		}
		_skillList.Reposition(resetPosition: true, tween: false);
		OnSelectSkill((_skillGroups.Count != 0) ? _skillGroups[0] : null);
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

	private int SkillBundleComparison(SkillGroup s1, SkillGroup s2)
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
		if (!((Object)(object)skillListNode == (Object)null) && !skillListNode.Select)
		{
			OnSelectSkill(skillListNode.Group);
		}
	}

	public void SelectSkillGroup(string bundleId)
	{
		ListObjectPool nodes = _skillList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			SkillListNode component = nodes[i].GetComponent<SkillListNode>();
			if (component.Group == null || component.Group.Skills == null)
			{
				continue;
			}
			for (int j = 0; j < component.Group.Skills.Count; j++)
			{
				if (component.Group.Skills[j].Id == bundleId)
				{
					OnSelectSkill(component.Group);
					return;
				}
			}
		}
		OnSelectSkill(null);
	}

	private void OnSelectSkill(SkillGroup skill)
	{
		ListObjectPool nodes = _skillList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			SkillListNode component = nodes[i].GetComponent<SkillListNode>();
			component.Select = skill == component.Group;
		}
		if (skill == null)
		{
			return;
		}
		if (skill.Skill == null)
		{
			if (this.SkillGroupSelected != null)
			{
				this.SkillGroupSelected(skill.Skills);
			}
		}
		else if (this.SkillSelected != null)
		{
			this.SkillSelected(skill.Skill);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using K1Network;
using Messages;
using Shared.Skill;
using SkillData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class SkillSystem : GameSystem<SkillSystem>
{
	private List<SkillData.SkillCategory> _skillCategories;

	private readonly Dictionary<string, SkillData.SkillBundle> _skillDict = new Dictionary<string, SkillData.SkillBundle>();

	private readonly List<SkillData.SkillBundle> _skills = new List<SkillData.SkillBundle>();

	private bool _isInitSkills;

	private List<SkillData.SkillCategory> SkillCategories
	{
		get
		{
			if (_skillCategories == null)
			{
				_skillCategories = new List<SkillData.SkillCategory>();
				Array values = Enum.GetValues(typeof(Category));
				int i = 0;
				for (int length = values.Length; i < length; i++)
				{
					Category category = (Category)(int)values.GetValue(i);
					if (category != Category.Invalid)
					{
						_skillCategories.Add(new SkillData.SkillCategory(category));
					}
				}
			}
			return _skillCategories;
		}
	}

	public List<SkillData.SkillBundle> Skills => _skills;

	public int SkillPoint { get; private set; }

	public bool Untrainable { get; private set; }

	public int RemainSkillPoint { get; private set; }

	public event Action<SkillNode> SkillLearned;

	public event Action<Category, int, int> CategoryLevelChanged;

	public event Action<Category, int> CategoryExpChanged;

	public event Action SkillListUpdated;

	private void Awake()
	{
		Connections.Frontend.On<Skills>(OnReceiveSkillMsg);
		Connections.Frontend.On<SkillNeeded>(OnSkillNeededMsg);
		Connections.Frontend.On<SkillCategoryExperienced>(SkillExpChanged);
		KSingleton<GameManager>.Instance().Ready += delegate
		{
			Connections.Frontend.Send(default(GetSkills));
		};
	}

	public void InitSkillList(SkillYaml skills)
	{
		_skills.Clear();
		_skillDict.Clear();
		foreach (KeyValuePair<Category, Dictionary<string, Dictionary<string, Yaml.Skill[]>>> skill in skills)
		{
			foreach (KeyValuePair<string, Dictionary<string, Yaml.Skill[]>> item in skill.Value)
			{
				SkillData.SkillBundle skillBundle = new SkillData.SkillBundle(item, skill.Key);
				_skills.Add(skillBundle);
				_skillDict.Add(skillBundle.Id, skillBundle);
			}
		}
	}

	public void InitSkillRewards(RewardYaml rewards)
	{
		((MonoBehaviour)this).StartCoroutine(CoInitSkillRewards(rewards));
	}

	private IEnumerator CoInitSkillRewards(RewardYaml rewards)
	{
		while (_skills.Count == 0)
		{
			yield return null;
		}
		for (int i = 0; i < _skills.Count; i++)
		{
			_skills[i].InitRewards(rewards);
		}
	}

	public void OnSkillNeededMsg(SkillNeeded msg, PacketHeader header)
	{
		SkillNode skillNode = FindSkill(msg.SkillId, msg.SubId, msg.Level);
		if (skillNode != null)
		{
			string text = SkillUtil.CategoryLocalizeName(skillNode.Category);
			string text2 = SkillUtil.CategoryIcon(skillNode.Category);
			if (skillNode.CategoryLevel > 0)
			{
				UIManager.SystemMsg(LocalizeSystem.Format("#skill_needed_with_level", skillNode.Name, text2, text, skillNode.CategoryLevel.ToString()));
			}
			else
			{
				UIManager.SystemMsg(LocalizeSystem.Format("#skill_needed_without_level", skillNode.Name, text2, text));
			}
		}
	}

	private void OnReceiveSkillMsg(Skills msg, PacketHeader header)
	{
		for (int i = 0; i < Skills.Count; i++)
		{
			Skills[i].Valid = false;
		}
		int j = 0;
		for (int size = KUtility.GetSize(msg.SkillList); j < size; j++)
		{
			Messages.SkillBundle skillBundle = msg.SkillList[j];
			SkillData.SkillBundle skillBundle2 = FindSkill(skillBundle.SkillId);
			if (skillBundle2 != null)
			{
				skillBundle2.SetLevel(skillBundle.Levels);
				skillBundle2.Valid = true;
			}
		}
		for (int k = 0; k < Skills.Count; k++)
		{
			if (!Skills[k].Valid)
			{
				Skills[k].SetLevel(null);
			}
			Skills[k].Valid = true;
		}
		int l = 0;
		for (int count = SkillCategories.Count; l < count; l++)
		{
			SkillCategories[l].PrevLevel = SkillCategories[l].Level;
		}
		foreach (KeyValuePair<Category, Messages.SkillCategory> category in msg.Categories)
		{
			SkillData.SkillCategory skillCategory = GetSkillCategory(category.Key);
			if (skillCategory != null)
			{
				bool flag = skillCategory.IsResearching();
				skillCategory.Set(category.Value);
				if (!flag && skillCategory.IsResearching())
				{
					KSingleton<GameManager>.Instance().PushNotification.CancelLocalPush(PushNotification.Type.SkillCategoryResearch);
					double num = skillCategory.ResearchEnd - Connections.Frontend.GetPredictedServerTime();
					string text = LocalizeSystem.Get(LocalizeUtil.GetKey(category.Key));
					KSingleton<GameManager>.Instance().PushNotification.LocalPushAfter(PushNotification.Type.SkillCategoryResearch, LocalizeSystem.Format("#skill_push_research_over", text), "offline_only", (int)num);
				}
			}
		}
		SkillPoint = msg.SkillPoint;
		int num2 = 0;
		for (int m = 0; m < _skills.Count; m++)
		{
			num2 += _skills[m].UsedSp();
		}
		RemainSkillPoint = SkillPoint - num2;
		Untrainable = msg.Untrainable;
		for (int n = 0; n < Skills.Count; n++)
		{
			Skills[n].UpdateState();
		}
		RaiseSkillEvent();
	}

	private void RaiseSkillEvent()
	{
		int i = 0;
		for (int count = _skills.Count; i < count; i++)
		{
			SkillData.SkillBundle skillBundle = _skills[i];
			RaiseSkillEvent(skillBundle.Base);
			int j = 0;
			for (int size = KUtility.GetSize(skillBundle.Sub); j < size; j++)
			{
				RaiseSkillEvent(skillBundle.Sub[j]);
			}
		}
		int k = 0;
		for (int count2 = SkillCategories.Count; k < count2; k++)
		{
			SkillData.SkillCategory skillCategory = SkillCategories[k];
			if (skillCategory.PrevLevel != skillCategory.Level && this.CategoryLevelChanged != null)
			{
				this.CategoryLevelChanged(skillCategory.Category, skillCategory.PrevLevel, skillCategory.Level);
			}
		}
		if (_isInitSkills)
		{
			if (this.SkillListUpdated != null)
			{
				this.SkillListUpdated();
			}
		}
		else
		{
			_isInitSkills = true;
		}
	}

	private void SkillExpChanged(SkillCategoryExperienced msg, PacketHeader header)
	{
		if (this.CategoryExpChanged != null)
		{
			this.CategoryExpChanged(msg.Category, msg.Exp);
		}
	}

	private void RaiseSkillEvent(SkillData.Skill skill)
	{
		for (int i = skill.PrevLevel + 1; i <= skill.Level; i++)
		{
			if (this.SkillLearned != null)
			{
				this.SkillLearned(skill.Get(i));
			}
		}
	}

	public void LearnSkill(SkillData.Skill skill)
	{
		int maxLevel = skill.MaxLevel;
		if (skill.Level < maxLevel)
		{
			LearnSkill learnSkill = default(LearnSkill);
			learnSkill.SkillId = skill.Id;
			learnSkill.SubId = skill.SubId;
			learnSkill.Level = skill.Level + 1;
			LearnSkill msg = learnSkill;
			Connections.Frontend.Send(msg);
		}
	}

	public void UntrainSkill(SkillData.Skill skill)
	{
		if (skill.Level >= 1)
		{
			UntrainSkill untrainSkill = default(UntrainSkill);
			untrainSkill.SkillId = skill.Id;
			untrainSkill.SubId = skill.SubId;
			untrainSkill.Level = skill.Level;
			UntrainSkill msg = untrainSkill;
			Connections.Frontend.Send(msg);
		}
	}

	public SkillData.SkillBundle FindSkill(string id)
	{
		return _skillDict.Get(id);
	}

	public SkillData.Skill FindBaseSkill(string id)
	{
		return FindSkill(id, "__base__");
	}

	public SkillData.Skill FindSkill(string id, string sub)
	{
		return FindSkill(id)?.Get(sub);
	}

	public SkillNode FindSkill(string id, string sub, int lv)
	{
		return FindSkill(id, sub)?.Get(lv);
	}

	public SkillNode FindSkill(Messages.Skill skill)
	{
		return FindSkill(skill.SkillId, skill.SubId, skill.Level);
	}

	public SkillNode FindSkill(Func<SkillNode, bool> checker)
	{
		for (int i = 0; i < _skills.Count; i++)
		{
			SkillData.SkillBundle skillBundle = _skills[i];
			int j = 0;
			for (int num = KUtility.GetSize(skillBundle.Sub) + 1; j < num; j++)
			{
				SkillData.Skill skill = ((j != 0) ? skillBundle.Sub[j - 1] : skillBundle.Base);
				for (int k = 1; k <= skill.MaxLevel; k++)
				{
					if (checker(skill.Get(k)))
					{
						return skill.Get(k);
					}
				}
			}
		}
		return null;
	}

	public SkillData.SkillCategory GetSkillCategory(Category cat)
	{
		int i = 0;
		for (int count = SkillCategories.Count; i < count; i++)
		{
			if (SkillCategories[i].Category == cat)
			{
				return SkillCategories[i];
			}
		}
		return null;
	}

	public int GetCategoryLevel(Category category)
	{
		return GetSkillCategory(category)?.Level ?? 0;
	}

	public SkillData.SkillCategory GetResearchingCategory()
	{
		int i = 0;
		for (int count = SkillCategories.Count; i < count; i++)
		{
			SkillData.SkillCategory skillCategory = SkillCategories[i];
			if (skillCategory.IsResearching())
			{
				return skillCategory;
			}
		}
		return null;
	}

	public int GetCategoryUsedSp(Category category)
	{
		List<SkillData.SkillBundle> skills = Skills;
		int num = 0;
		for (int i = 0; i < skills.Count; i++)
		{
			SkillData.SkillBundle skillBundle = skills[i];
			if (skillBundle.Category == category)
			{
				num += skillBundle.UsedSp();
			}
		}
		return num;
	}

	public void GetCategoryExp(Category category, out int current, out int max)
	{
		int categoryLevel = GetCategoryLevel(category);
		if (categoryLevel <= 0)
		{
			current = 0;
			max = -1;
		}
		else
		{
			max = SingletonDict<Category, Yaml.SkillCategory>.Get(category).exp_needed.Get(categoryLevel, -1);
			current = GetSkillCategory(category).Exp;
		}
	}

	public void ResearchSkillCategory(Category cat, Category? skipCat = null, int skipCost = 0)
	{
		Connections.Frontend.Send(new ResearchSkillCategory
		{
			Category = cat,
			SkipCategory = skipCat,
			SkipCost = skipCost
		});
	}

	public void SkipResearchSkillCategory(Category cat)
	{
		SkillData.SkillCategory skillCategory = GetSkillCategory(cat);
		if (skillCategory != null && skillCategory.ResearchSkipCost != null)
		{
			int cost = (int)skillCategory.ResearchSkipCost.Get();
			Connections.Frontend.Send(new SkipSkillCategoryResearch
			{
				SkillCategory = cat,
				Cost = cost
			});
		}
	}
}

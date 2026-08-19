using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic.Skill;
using Durango.Network;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Skill;
using Shared.StatusEffect;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class SkillSystem : GameSystem<SkillSystem>
{
	private enum SkillEventType
	{
		LevelChanged
	}

	private enum CategoryEventType
	{
		LevelChanged,
		ResearchStarted,
		ResearchFinished,
		ResearchReady,
		ResearchUpdated
	}

	private List<Durango.Logic.Skill.Category> _skillCategories;

	private readonly Dictionary<string, Bundle> _skillDict = new Dictionary<string, Bundle>();

	private readonly List<Bundle> _skills = new List<Bundle>();

	private readonly List<KeyValuePair<SkillEventType, Durango.Logic.Skill.Skill>> _eventedSkills = new List<KeyValuePair<SkillEventType, Durango.Logic.Skill.Skill>>();

	private readonly List<KeyValuePair<CategoryEventType, Durango.Logic.Skill.Category>> _eventedCategories = new List<KeyValuePair<CategoryEventType, Durango.Logic.Skill.Category>>();

	private bool _isInitSkills;

	private List<Durango.Logic.Skill.Category> SkillCategories
	{
		get
		{
			if (_skillCategories == null)
			{
				_skillCategories = new List<Durango.Logic.Skill.Category>();
				Array values = Enum.GetValues(typeof(Shared.Skill.Category));
				int i = 0;
				for (int length = values.Length; i < length; i++)
				{
					Shared.Skill.Category category = (Shared.Skill.Category)values.GetValue(i);
					if (category != Shared.Skill.Category.Invalid)
					{
						_skillCategories.Add(new Durango.Logic.Skill.Category(category));
					}
				}
			}
			return _skillCategories;
		}
	}

	[NotNull]
	public List<Bundle> Skills => _skills;

	public int SkillPoint { get; private set; }

	public int UntrainedCount { get; private set; }

	public int RemainSkillPoint { get; private set; }

	public int SkillCategoryCount => SkillCategories.Count;

	public event Action<Durango.Logic.Skill.Skill> SkillLevelChanged;

	public event Action<Durango.Logic.Skill.Category> CategoryLevelChanged;

	public event Action<Durango.Logic.Skill.Category> CategoryResearchStarted;

	public event Action<Durango.Logic.Skill.Category> CategoryResearchUpdated;

	public event Action<Durango.Logic.Skill.Category> CategoryResearchFinished;

	public event Action<Durango.Logic.Skill.Category> CategoryResearchReady;

	public event Action SkillListUpdated;

	public event Action<SkillCategoryExperienced> SkillCategoryExperienced;

	private void Awake()
	{
		Connections.Frontend.On<Skills>(OnReceiveSkillMsg);
		Connections.Frontend.On<SkillNeeded>(OnSkillNeededMsg);
		Connections.Frontend.On<SkillCategoryExperienced>(SkillExpChanged);
		GameSystem<StatisticsSystem>.Instance().LevelChanged += OnChangePlayerLevel;
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetSkills));
		});
	}

	public void InitSkillList(SkillYaml skills)
	{
		_skills.Clear();
		_skillDict.Clear();
		foreach (KeyValuePair<Shared.Skill.Category, Dictionary<string, Dictionary<string, Yaml.Skill[]>>> skill in skills)
		{
			foreach (KeyValuePair<string, Dictionary<string, Yaml.Skill[]>> item in skill.Value)
			{
				Bundle bundle = new Bundle(item, skill.Key);
				_skills.Add(bundle);
				_skillDict.Add(bundle.Id, bundle);
			}
		}
	}

	public void InitSkillRewards(RewardYaml rewards)
	{
		StartCoroutine(CoInitSkillRewards(rewards));
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

	private void OnSkillNeededMsg(SkillNeeded msg, PacketHeader header)
	{
		SkillNeeded(msg);
	}

	public void SkillNeeded(SkillNeeded msg)
	{
		SkillNeeded(FindSkill(msg.SkillId, msg.SubId, msg.Level));
	}

	public void SkillNeeded(Node skill)
	{
		if (skill != null)
		{
			string text = Util.CategoryLocalizeName(skill.Category);
			string text2 = Util.CategoryIcon(skill.Category);
			Action action = delegate
			{
				SkillGroup skillGroup = UIManager.FindScript<SkillGroup>();
				skillGroup.Open(skill);
			};
			string comment = ((skill.CategoryLevel <= 0) ? T._("<em>{0}</em> ([icon={1}]{2}) 스킬이 필요합니다.", skill.Name, text2, text) : T._("<em>{0}</em> ([icon={1}]{2} {3:lv:}) 스킬이 필요합니다.", skill.Name, text2, text, skill.CategoryLevel));
			ConfirmPopup confirmPopup = UIManager.Popup.Tooltip<ConfirmPopup>();
			confirmPopup.AddButton(new MessageBox.Button(T._("스킬 보기")), action).Show(comment, 600f);
		}
	}

	private void OnChangePlayerLevel(int prev, int level)
	{
		int i = 0;
		for (int count = SkillCategories.Count; i < count; i++)
		{
			Durango.Logic.Skill.Category category = SkillCategories[i];
			if (category.Level >= prev && category.IsReadyToResearch() && this.CategoryResearchReady != null)
			{
				this.CategoryResearchReady(category);
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
			SkillBundle skillBundle = msg.SkillList[j];
			Bundle bundle = FindSkill(skillBundle.SkillId);
			if (bundle == null || bundle.Base == null)
			{
				continue;
			}
			int k = 0;
			for (int num = KUtility.GetSize(bundle.Sub) + 1; k < num; k++)
			{
				Durango.Logic.Skill.Skill skill = ((k != 0) ? bundle.Sub[k - 1] : bundle.Base);
				int level = ((skillBundle.Levels != null) ? skillBundle.Levels.Get(skill.SubId, 0) : 0);
				int level2 = skill.Level;
				skill.Level = level;
				if (level2 != skill.Level)
				{
					_eventedSkills.Add(new KeyValuePair<SkillEventType, Durango.Logic.Skill.Skill>(SkillEventType.LevelChanged, skill));
				}
			}
			bundle.Valid = true;
		}
		for (int l = 0; l < Skills.Count; l++)
		{
			Bundle bundle2 = Skills[l];
			if (!bundle2.Valid && bundle2.Base != null)
			{
				int m = 0;
				for (int num2 = KUtility.GetSize(bundle2.Sub) + 1; m < num2; m++)
				{
					Durango.Logic.Skill.Skill skill2 = ((m != 0) ? bundle2.Sub[m - 1] : bundle2.Base);
					int level3 = skill2.Level;
					skill2.Level = 0;
					if (level3 != skill2.Level)
					{
						_eventedSkills.Add(new KeyValuePair<SkillEventType, Durango.Logic.Skill.Skill>(SkillEventType.LevelChanged, skill2));
					}
				}
			}
			bundle2.Valid = true;
		}
		foreach (KeyValuePair<Shared.Skill.Category, Messages.SkillCategory> category in msg.Categories)
		{
			Durango.Logic.Skill.Category skillCategory = GetSkillCategory(category.Key);
			if (skillCategory != null)
			{
				bool flag = skillCategory.IsResearching();
				bool flag2 = skillCategory.IsReadyToResearch();
				double researchEnd = skillCategory.ResearchEnd;
				int level4 = skillCategory.Level;
				skillCategory.Set(category.Value);
				if (level4 != skillCategory.Level)
				{
					_eventedCategories.Add(new KeyValuePair<CategoryEventType, Durango.Logic.Skill.Category>(CategoryEventType.LevelChanged, skillCategory));
				}
				bool flag3 = skillCategory.IsResearching();
				if (flag != flag3)
				{
					_eventedCategories.Add((!flag3) ? new KeyValuePair<CategoryEventType, Durango.Logic.Skill.Category>(CategoryEventType.ResearchFinished, skillCategory) : new KeyValuePair<CategoryEventType, Durango.Logic.Skill.Category>(CategoryEventType.ResearchStarted, skillCategory));
				}
				else if (researchEnd != skillCategory.ResearchEnd)
				{
					_eventedCategories.Add(new KeyValuePair<CategoryEventType, Durango.Logic.Skill.Category>(CategoryEventType.ResearchUpdated, skillCategory));
				}
				bool flag4 = skillCategory.IsReadyToResearch();
				if (flag2 != flag4 && flag4)
				{
					_eventedCategories.Add(new KeyValuePair<CategoryEventType, Durango.Logic.Skill.Category>(CategoryEventType.ResearchReady, skillCategory));
				}
			}
		}
		SkillPoint = msg.SkillPoint;
		int num3 = 0;
		for (int n = 0; n < _skills.Count; n++)
		{
			num3 += _skills[n].UsedSp();
		}
		RemainSkillPoint = SkillPoint - num3;
		UntrainedCount = msg.UntrainedCount;
		for (int num4 = 0; num4 < Skills.Count; num4++)
		{
			Skills[num4].UpdateState();
		}
		RaiseSkillEvent();
	}

	private void RaiseSkillEvent()
	{
		if (!_isInitSkills)
		{
			_isInitSkills = true;
			_eventedSkills.Clear();
			_eventedCategories.Clear();
			return;
		}
		for (int i = 0; i < _eventedSkills.Count; i++)
		{
			SkillEventType key = _eventedSkills[i].Key;
			Durango.Logic.Skill.Skill value = _eventedSkills[i].Value;
			if (key == SkillEventType.LevelChanged && this.SkillLevelChanged != null)
			{
				this.SkillLevelChanged(value);
			}
		}
		for (int j = 0; j < _eventedCategories.Count; j++)
		{
			CategoryEventType key2 = _eventedCategories[j].Key;
			Durango.Logic.Skill.Category value2 = _eventedCategories[j].Value;
			switch (key2)
			{
			case CategoryEventType.LevelChanged:
				if (this.CategoryLevelChanged != null)
				{
					this.CategoryLevelChanged(value2);
				}
				break;
			case CategoryEventType.ResearchStarted:
				if (this.CategoryResearchStarted != null)
				{
					this.CategoryResearchStarted(value2);
				}
				break;
			case CategoryEventType.ResearchFinished:
				if (this.CategoryResearchFinished != null)
				{
					this.CategoryResearchFinished(value2);
				}
				break;
			case CategoryEventType.ResearchReady:
				if (this.CategoryResearchReady != null)
				{
					this.CategoryResearchReady(value2);
				}
				break;
			case CategoryEventType.ResearchUpdated:
				if (this.CategoryResearchUpdated != null)
				{
					this.CategoryResearchUpdated(value2);
				}
				break;
			}
		}
		_eventedSkills.Clear();
		_eventedCategories.Clear();
		if (this.SkillListUpdated != null)
		{
			this.SkillListUpdated();
		}
	}

	private void SkillExpChanged(SkillCategoryExperienced msg, PacketHeader header)
	{
		if (this.SkillCategoryExperienced != null)
		{
			this.SkillCategoryExperienced(msg);
		}
	}

	public void LearnSkill([NotNull] Durango.Logic.Skill.Skill skill, Action<bool> onResult)
	{
		int maxLevel = skill.MaxLevel;
		if (skill.Level >= maxLevel)
		{
			return;
		}
		LearnSkill learnSkill = default(LearnSkill);
		learnSkill.SkillId = skill.Id;
		learnSkill.SubId = skill.SubId;
		learnSkill.Level = skill.Level + 1;
		LearnSkill msg = learnSkill;
		Connections.Frontend.Send(msg).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(packet.Header.TypeCode == 1231);
			}
		});
	}

	public void UntrainSkill([NotNull] Durango.Logic.Skill.Skill skill, string voucherId, Action<bool> onResult)
	{
		if (skill.Level < 1)
		{
			return;
		}
		UntrainSkill untrainSkill = default(UntrainSkill);
		untrainSkill.SkillId = skill.Id;
		untrainSkill.SubId = skill.SubId;
		untrainSkill.Level = skill.Level;
		untrainSkill.VoucherId = voucherId;
		UntrainSkill msg = untrainSkill;
		Connections.Frontend.Send(msg).All(delegate(Packet packet)
		{
			if (onResult != null)
			{
				onResult(packet.Header.TypeCode == 1231);
			}
		});
	}

	public Bundle FindSkill(string id)
	{
		return _skillDict.Get(id);
	}

	public Durango.Logic.Skill.Skill FindBaseSkill(string id)
	{
		return FindSkill(id, "__base__");
	}

	[CanBeNull]
	public Durango.Logic.Skill.Skill FindSkill(string id, string sub)
	{
		return FindSkill(id)?.Get(sub);
	}

	[CanBeNull]
	public Node FindSkill(string id, string sub, int lv)
	{
		return FindSkill(id, sub)?.Get(lv);
	}

	[CanBeNull]
	public Node FindSkill(Messages.Skill skill)
	{
		return FindSkill(skill.SkillId, skill.SubId, skill.Level);
	}

	[CanBeNull]
	public Node FindSkill(Predicate<Node> checker)
	{
		for (int i = 0; i < _skills.Count; i++)
		{
			Bundle bundle = _skills[i];
			int j = 0;
			for (int num = KUtility.GetSize(bundle.Sub) + 1; j < num; j++)
			{
				Durango.Logic.Skill.Skill skill = ((j != 0) ? bundle.Sub[j - 1] : bundle.Base);
				if (skill == null)
				{
					continue;
				}
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

	public Durango.Logic.Skill.Category GetSkillCategory(int index)
	{
		return SkillCategories[index];
	}

	public Durango.Logic.Skill.Category GetSkillCategory(Shared.Skill.Category cat)
	{
		int i = 0;
		for (int count = SkillCategories.Count; i < count; i++)
		{
			if (SkillCategories[i].Type == cat)
			{
				return SkillCategories[i];
			}
		}
		return null;
	}

	public int GetCategoryLevel(Shared.Skill.Category category)
	{
		return GetSkillCategory(category)?.Level ?? 0;
	}

	public Shared.Skill.Category GetMaxLevelCategory()
	{
		int num = -1;
		Shared.Skill.Category result = Shared.Skill.Category.Survival;
		int i = 0;
		for (int count = SkillCategories.Count; i < count; i++)
		{
			Durango.Logic.Skill.Category category = SkillCategories[i];
			if (category.Level > num)
			{
				result = category.Type;
				num = category.Level;
			}
		}
		return result;
	}

	public Durango.Logic.Skill.Category GetResearchingCategory()
	{
		int i = 0;
		for (int count = SkillCategories.Count; i < count; i++)
		{
			Durango.Logic.Skill.Category category = SkillCategories[i];
			if (category.IsResearching())
			{
				return category;
			}
		}
		return null;
	}

	public int GetCategoryUsedSp(Shared.Skill.Category category)
	{
		List<Bundle> skills = Skills;
		int num = 0;
		for (int i = 0; i < skills.Count; i++)
		{
			Bundle bundle = skills[i];
			if (bundle.Category == category)
			{
				num += bundle.UsedSp();
			}
		}
		return num;
	}

	public void GetCategoryExp(Shared.Skill.Category category, out int current, out int max)
	{
		int categoryLevel = GetCategoryLevel(category);
		if (categoryLevel <= 0)
		{
			current = 0;
			max = -1;
		}
		else
		{
			max = SingletonDict<Shared.Skill.Category, Yaml.SkillCategory>.Get(category).ExpNeeded.Get(categoryLevel, -1);
			current = GetSkillCategory(category).Exp;
		}
	}

	public void ResearchSkillCategory(Shared.Skill.Category cat, Shared.Skill.Category? skipCat = null, int skipCost = 0)
	{
		Connections.Frontend.Send(new ResearchSkillCategory
		{
			Category = cat,
			SkipCategory = skipCat
		});
	}

	public void SkipResearchSkillCategory(Shared.Skill.Category cat)
	{
		Durango.Logic.Skill.Category skillCategory = GetSkillCategory(cat);
		if (skillCategory != null && skillCategory.ResearchSkipCost != null)
		{
			Connections.Frontend.Send(new SkipSkillCategoryResearch
			{
				SkillCategory = cat
			});
		}
	}

	public void CancelResearchSkillCategory(Shared.Skill.Category cat)
	{
		Connections.Frontend.Send(new CancelSkillCategoryResearch
		{
			SkillCategory = cat
		});
	}

	public static StatusEffect FindSkillExpModifier(Shared.Skill.Category category, out float value)
	{
		string key = category.ToString().ToSnakeCase();
		StatusEffects statusEffects = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects();
		foreach (StatusEffect item in statusEffects.List)
		{
			value = item.GetDetail(EffectType.SkillExpModifier, key);
			if (value > 0f)
			{
				return item;
			}
		}
		value = 0f;
		return null;
	}
}

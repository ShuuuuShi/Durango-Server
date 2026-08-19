using System;
using System.Collections.Generic;
using Shared.Skill;
using Yaml;

namespace SkillData;

public class SkillBundle
{
	public const string BaseKey = "__base__";

	public readonly string Id;

	public readonly Category Category;

	public readonly Skill Base;

	public readonly Skill[] Sub;

	public bool Valid { get; set; }

	public string Group
	{
		get
		{
			if (Base == null)
			{
				return null;
			}
			return Base.Get(1).Group;
		}
	}

	public int RenderPriority
	{
		get
		{
			if (Base == null)
			{
				return int.MaxValue;
			}
			return Base.Get(1).RenderPriority;
		}
	}

	public SkillBundle(KeyValuePair<string, Dictionary<string, Yaml.Skill[]>> data, Category category)
	{
		Id = data.Key;
		Category = category;
		Sub = ((data.Value.Count <= 1) ? null : new Skill[data.Value.Count - 1]);
		int num = 0;
		foreach (KeyValuePair<string, Yaml.Skill[]> item in data.Value)
		{
			if (item.Key == "__base__")
			{
				Base = new Skill(item, this);
			}
			else
			{
				Sub[num++] = new Skill(item, this);
			}
		}
		if (Sub != null)
		{
			Array.Sort(Sub, Comparison);
		}
	}

	private static int Comparison(Skill s1, Skill s2)
	{
		int num = s1.Get(1).RenderPriority - s2.Get(1).RenderPriority;
		if (num == 0)
		{
			num = s1.Get(1).CategoryLevel - s2.Get(1).CategoryLevel;
		}
		return num;
	}

	public Skill Get(string key)
	{
		if (Base.SubId == key)
		{
			return Base;
		}
		if (Sub == null)
		{
			return null;
		}
		for (int i = 0; i < Sub.Length; i++)
		{
			if (Sub[i].SubId == key)
			{
				return Sub[i];
			}
		}
		return null;
	}

	public void InitRewards(RewardYaml yml)
	{
		if (Base != null)
		{
			Base.InitRewards(yml);
		}
		if (Sub != null)
		{
			for (int i = 0; i < Sub.Length; i++)
			{
				Sub[i].InitRewards(yml);
			}
		}
	}

	public void SetLevel(Dictionary<string, int> dict)
	{
		int i = 0;
		for (int num = ((Base.Level == 0) ? 1 : (KUtility.GetSize(Sub) + 1)); i < num; i++)
		{
			Skill skill = ((i != 0) ? Sub[i - 1] : Base);
			skill.Valid = false;
		}
		if (dict != null)
		{
			foreach (KeyValuePair<string, int> item in dict)
			{
				Skill skill2 = Get(item.Key);
				if (skill2 != null)
				{
					skill2.SetLevel(item.Value);
					skill2.Valid = true;
				}
			}
		}
		int j = 0;
		for (int num2 = KUtility.GetSize(Sub) + 1; j < num2; j++)
		{
			Skill skill3 = ((j != 0) ? Sub[j - 1] : Base);
			if (!skill3.Valid)
			{
				skill3.SetLevel(0);
			}
			skill3.Valid = true;
		}
	}

	public void UpdateState()
	{
		int i = 0;
		for (int num = KUtility.GetSize(Sub) + 1; i < num; i++)
		{
			Skill skill = ((i != 0) ? Sub[i - 1] : Base);
			skill.UpdateState();
		}
	}

	public int UsedSp()
	{
		int num = Base.UsedSp();
		if (Sub != null)
		{
			for (int i = 0; i < Sub.Length; i++)
			{
				num += Sub[i].UsedSp();
			}
		}
		return num;
	}

	public int GetLearnableCount()
	{
		int num = 0;
		int i = 0;
		for (int num2 = KUtility.GetSize(Sub) + 1; i < num2; i++)
		{
			Skill skill = ((i != 0) ? Sub[i - 1] : Base);
			SkillNode skillNode = skill.Get(skill.Level + 1);
			if (skillNode != null && skillNode.State == SkillState.Learnable)
			{
				num++;
			}
		}
		return num;
	}

	public int HighestLevel()
	{
		int num = 0;
		int i = 0;
		for (int num2 = KUtility.GetSize(Sub) + 1; i < num2; i++)
		{
			Skill skill = ((i != 0) ? Sub[i - 1] : Base);
			if (skill.Level > num)
			{
				num = skill.Level;
			}
		}
		return num;
	}

	public int NearestNextAvailableCategoryLevel()
	{
		int num = 1000000;
		int i = 0;
		for (int num2 = KUtility.GetSize(Sub) + 1; i < num2; i++)
		{
			Skill skill = ((i != 0) ? Sub[i - 1] : Base);
			SkillNode skillNode = skill.Get(skill.Level + 1);
			if (skillNode != null && skillNode.CategoryLevel < num)
			{
				num = skillNode.CategoryLevel;
			}
		}
		return num;
	}

	public bool HasNew()
	{
		int i = 0;
		for (int num = KUtility.GetSize(Sub) + 1; i < num; i++)
		{
			Skill skill = ((i != 0) ? Sub[i - 1] : Base);
			if (skill.HasNew())
			{
				return true;
			}
		}
		return false;
	}
}

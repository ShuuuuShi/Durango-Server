using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Shared.Skill;
using Yaml;

namespace Durango.Logic.Skill;

public class Bundle
{
	public const string BaseKey = "__base__";

	public readonly string Id;

	public readonly Shared.Skill.Category Category;

	[CanBeNull]
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

	public Bundle(KeyValuePair<string, Dictionary<string, Yaml.Skill[]>> data, Shared.Skill.Category category)
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
		if (Base != null && Base.SubId == key)
		{
			return Base;
		}
		int i = 0;
		for (int size = KUtility.GetSize(Sub); i < size; i++)
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
		int i = 0;
		for (int size = KUtility.GetSize(Sub); i < size; i++)
		{
			Sub[i].InitRewards(yml);
		}
	}

	public void UpdateState()
	{
		int i = 0;
		for (int num = KUtility.GetSize(Sub) + 1; i < num; i++)
		{
			((i != 0) ? Sub[i - 1] : Base)?.UpdateState();
		}
	}

	public int UsedSp()
	{
		int num = 0;
		if (Base != null)
		{
			num = Base.UsedSp();
		}
		int i = 0;
		for (int size = KUtility.GetSize(Sub); i < size; i++)
		{
			num += Sub[i].UsedSp();
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
			if (skill != null)
			{
				Node node = skill.Get(skill.Level + 1);
				if (node != null && node.State == State.Learnable)
				{
					num++;
				}
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
			if (skill != null && skill.Level > num)
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
			if (skill != null)
			{
				Node node = skill.Get(skill.Level + 1);
				if (node != null && node.CategoryLevel < num)
				{
					num = node.CategoryLevel;
				}
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
			if (skill != null && skill.HasNew())
			{
				return true;
			}
		}
		return false;
	}
}

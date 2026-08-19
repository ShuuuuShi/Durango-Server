using System.Collections.Generic;
using Shared.Skill;
using UnityEngine;
using Yaml;

namespace SkillData;

public class Skill
{
	public string SubId;

	private SkillNode[] _list;

	public bool Valid { get; set; }

	public SkillBundle Parent { get; private set; }

	public string Id => Parent.Id;

	public int Level { get; private set; }

	public int PrevLevel { get; private set; }

	public int MaxLevel => _list.Length;

	public Category Category => Parent.Category;

	public Skill(KeyValuePair<string, Yaml.Skill[]> data, SkillBundle parent)
	{
		SubId = data.Key;
		_list = new SkillNode[data.Value.Length];
		for (int i = 0; i < _list.Length; i++)
		{
			_list[i] = new SkillNode(data.Value[i], this, i + 1);
		}
		Parent = parent;
	}

	public SkillNode Get()
	{
		return Get(Mathf.Max(1, Level));
	}

	public SkillNode Get(int level)
	{
		int num = level - 1;
		if (num < 0 || num >= _list.Length)
		{
			return null;
		}
		return _list[num];
	}

	public void InitRewards(RewardYaml yml)
	{
		for (int i = 0; i < _list.Length; i++)
		{
			_list[i].InitRewards(yml);
		}
	}

	public void SetLevel(int level)
	{
		PrevLevel = Level;
		Level = level;
	}

	public void UpdateState()
	{
		for (int i = 0; i < _list.Length; i++)
		{
			_list[i].UpdateState();
		}
	}

	public bool HasNew()
	{
		for (int i = 0; i < _list.Length; i++)
		{
			if (_list[i].IsNew)
			{
				return true;
			}
		}
		return false;
	}

	public int UsedSp()
	{
		int num = 0;
		for (int i = 1; i <= Level; i++)
		{
			num += Get(i).SkillPoints;
		}
		return num;
	}
}

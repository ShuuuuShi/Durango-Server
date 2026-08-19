using System.Collections.Generic;
using Shared.Skill;
using UnityEngine;
using Yaml;

namespace Durango.Logic.Skill;

public class Skill
{
	public string SubId;

	private readonly Node[] _list;

	public Bundle Bundle { get; private set; }

	public string Id => Bundle.Id;

	public int Level { get; set; }

	public int MaxLevel => _list.Length;

	public Shared.Skill.Category Category => Bundle.Category;

	public Skill(KeyValuePair<string, Yaml.Skill[]> data, Bundle parent)
	{
		SubId = data.Key;
		_list = new Node[data.Value.Length];
		for (int i = 0; i < _list.Length; i++)
		{
			_list[i] = new Node(data.Value[i], this, i + 1);
		}
		Bundle = parent;
	}

	public Node Get()
	{
		return Get(Mathf.Max(1, Level));
	}

	public Node Get(int level)
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

	public bool HasLearnableNode()
	{
		Node[] list = _list;
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i].State == State.Learnable)
			{
				return true;
			}
		}
		return false;
	}
}

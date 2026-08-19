using System.Collections.Generic;
using Durango.Logic.Skill;

namespace Durango.Logic.LearningGuide;

public class SkillWithPreviousNodesSet
{
	private class SubWithMaxLevel
	{
		private readonly Dictionary<string, int> _dictionary = new Dictionary<string, int>();

		public int GetMaxLevel(string sub)
		{
			int value;
			return (!_dictionary.TryGetValue(sub, out value)) ? (-1) : value;
		}

		public void Add(string sub, Node skill)
		{
			int maxLevel = GetMaxLevel(sub);
			if (maxLevel == -1 || maxLevel < skill.Level)
			{
				_dictionary[sub] = skill.Level;
			}
		}
	}

	private readonly Dictionary<string, SubWithMaxLevel> _dictionary = new Dictionary<string, SubWithMaxLevel>();

	public bool Contains(Node skill)
	{
		if (_dictionary.TryGetValue(skill.Id, out var value))
		{
			int maxLevel = value.GetMaxLevel(skill.Sub);
			if (maxLevel != -1)
			{
				return skill.Level <= maxLevel;
			}
			if (skill.Level == 1 && skill.Sub == "__base__")
			{
				return true;
			}
		}
		return false;
	}

	public void Clear()
	{
		_dictionary.Clear();
	}

	public void Add(Node skill)
	{
		if (!_dictionary.TryGetValue(skill.Id, out var value))
		{
			value = new SubWithMaxLevel();
			_dictionary.Add(skill.Id, value);
		}
		value.Add(skill.Sub, skill);
	}
}

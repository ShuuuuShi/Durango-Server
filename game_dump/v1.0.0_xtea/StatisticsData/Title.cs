using System.Collections.Generic;
using System.Linq;
using Shared.Ability;
using Shared.Skill;
using SkillData;
using Yaml;

namespace StatisticsData;

public class Title
{
	private readonly Yaml.Title _title;

	public bool Enabled { get; set; }

	public string Id { get; private set; }

	public string Name => _title.name;

	public string Description => _title.description;

	public KeyValuePair<Category, int>[] CategoryLevels { get; private set; }

	public bool ForAdvisor => _title.for_advisor;

	public int ExptectedLevelOfAchieved => _title.exptected_level_of_achieved;

	public string Icon => _title.icon;

	public Title(string key, Yaml.Title title)
	{
		Id = key;
		_title = title;
		if (_title.category_levels != null && _title.category_levels.Count > 0)
		{
			CategoryLevels = _title.category_levels.ToArray();
		}
	}

	public int GetAbility(Basic key)
	{
		return (_title.abilities != null) ? _title.abilities.Get(key, 0) : 0;
	}

	public int RequiredSkillCount()
	{
		return (_title.skills != null) ? _title.skills.Length : 0;
	}

	public SkillNode GetRequiredSkill(int index)
	{
		if (_title.skills == null || _title.skills.Length <= index)
		{
			return null;
		}
		RequiredSkill requiredSkill = _title.skills[index];
		return GameSystem<SkillSystem>.Instance().FindSkill(requiredSkill.skill_id, requiredSkill.sub_id, requiredSkill.level);
	}
}

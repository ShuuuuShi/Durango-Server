using Durango.Logic.Skill;
using Yaml;

namespace Durango.Logic.LearningGuide;

public class Advice
{
	private readonly Yaml.Advice _advice;

	public bool Enabled { get; set; }

	public string Id { get; private set; }

	public string Name => _advice.name;

	public string Description => _advice.description;

	public int Difficulty => _advice.difficulty;

	public int Cooperation => _advice.cooperation;

	public bool Recommended => _advice.recommended;

	public string Category => _advice.category;

	public string SubCategory => _advice.subcategory;

	public string RewardTitleId => _advice.reward_title_id;

	public Gettext RewardItemsName => _advice.reward_items_name;

	public RewardItem[] RewardItems => _advice.reward_items;

	public Advice(string key, Yaml.Advice advice)
	{
		Id = key;
		_advice = advice;
	}

	public int SkillsCount()
	{
		if (_advice.skills == null)
		{
			return 0;
		}
		return _advice.skills.Length;
	}

	public Node GetSkill(int index)
	{
		if (_advice.skills == null || _advice.skills.Length <= index)
		{
			return null;
		}
		SkillAdvice skillAdvice = _advice.skills[index];
		return GameSystem<SkillSystem>.Instance().FindSkill(skillAdvice.skill_id, skillAdvice.sub_id, skillAdvice.level);
	}

	public RequiredSkill RequiredSkill()
	{
		return _advice.required_skill;
	}

	public Gettext[] GetHints()
	{
		return _advice.hints;
	}
}

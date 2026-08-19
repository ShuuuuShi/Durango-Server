using Messages;
using Shared.Skill;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Skill;

public class Category
{
	public Shared.Skill.Category Type;

	public int Level;

	public int Exp;

	public double ResearchBegin;

	public double ResearchEnd;

	public double ResearchReducedTime;

	public float ResearchReduceRate;

	public Gauge ResearchSkipCost;

	public Category(Shared.Skill.Category cat)
	{
		Type = cat;
	}

	public void Set(Messages.SkillCategory msg)
	{
		Level = msg.Level;
		Exp = msg.Exp;
		if (msg.Researching.HasValue)
		{
			SkillCategoryResearching value = msg.Researching.Value;
			ResearchBegin = ((!value.StartedAt.HasValue) ? 0.0 : value.StartedAt.Value);
			ResearchEnd = ((!value.EndsAt.HasValue) ? 0.0 : value.EndsAt.Value);
			ResearchReducedTime = ((!value.SavedTime.HasValue) ? 0f : value.SavedTime.Value);
			ResearchSkipCost = value.SkipCost;
			ResearchReduceRate = ((!msg.ResearchTime.HasValue) ? 0f : msg.ResearchTime.Value.ReduceRate);
		}
		else
		{
			ResearchBegin = 0.0;
			ResearchEnd = 0.0;
			ResearchReducedTime = 0.0;
			ResearchSkipCost = null;
		}
	}

	public bool IsResearching()
	{
		return ResearchBegin != 0.0 && ResearchEnd != 0.0;
	}

	public bool IsReadyToResearch()
	{
		if (!IsResearching() && Level < GameSystem<StatisticsSystem>.Instance().Level)
		{
			Yaml.SkillCategory skillCategory = SingletonDict<Shared.Skill.Category, Yaml.SkillCategory>.Get(Type);
			int num = skillCategory.ResearchTimes.Get(Level, 0);
			if (num > 0)
			{
				int num2 = skillCategory.ExpNeeded.Get(Level, -1);
				return num2 > 0 && Exp >= num2;
			}
			return false;
		}
		return false;
	}
}

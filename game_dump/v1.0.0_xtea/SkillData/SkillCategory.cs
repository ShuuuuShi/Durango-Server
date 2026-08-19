using Messages;
using Shared.Skill;

namespace SkillData;

public class SkillCategory
{
	public Category Category;

	public int PrevLevel;

	public int Level;

	public int Exp;

	public double ResearchBegin;

	public double ResearchEnd;

	public Gauge ResearchSkipCost;

	public SkillCategory(Category cat)
	{
		Category = cat;
	}

	public void Set(Messages.SkillCategory msg)
	{
		PrevLevel = Level;
		Level = msg.Level;
		Exp = msg.Exp;
		if (msg.Researching.HasValue)
		{
			ResearchBegin = msg.Researching.Value.Key;
			ResearchEnd = msg.Researching.Value.Value;
		}
		else
		{
			ResearchBegin = 0.0;
			ResearchEnd = 0.0;
		}
		ResearchSkipCost = msg.ResearchSkipCost;
	}

	public bool IsResearching()
	{
		return ResearchBegin != 0.0 && ResearchEnd != 0.0;
	}
}

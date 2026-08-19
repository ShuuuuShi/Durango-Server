using System.Collections.Generic;
using Shared.Pet;

namespace Yaml;

public class PetActiveSkillConditionDict : Dictionary<SkillRank, PetActiveSkillCondition>
{
	public PetActiveSkillConditionDict()
		: base((IEqualityComparer<SkillRank>)default(SkillRankComparer))
	{
	}
}

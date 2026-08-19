using System.Collections.Generic;
using Shared.Pet;

namespace Yaml;

public class PetActiveSkillRankDict : Dictionary<SkillRank, PetActiveSkill>
{
	public PetActiveSkillRankDict()
		: base((IEqualityComparer<SkillRank>)default(SkillRankComparer))
	{
	}
}

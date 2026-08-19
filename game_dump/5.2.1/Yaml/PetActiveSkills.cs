using Shared.Pet;
using Yaml.Util;

namespace Yaml;

public class PetActiveSkills : SingletonDict<string, PetActiveSkillRankDict>
{
	public static PetActiveSkill Get(string skillId, SkillRank rank)
	{
		if (SingletonDict<string, PetActiveSkillRankDict>.TryGetValue(skillId, out var value) && value.TryGetValue(rank, out var value2))
		{
			return value2;
		}
		return null;
	}
}

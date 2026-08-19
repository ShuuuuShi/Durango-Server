using Shared.Pet;
using Yaml.Util;

namespace Yaml;

public class PetActiveSkillConditions : SingletonDict<string, PetActiveSkillConditionDict>
{
	public static PetActiveSkillCondition Get(string skillId, SkillRank rank)
	{
		if (SingletonDict<string, PetActiveSkillConditionDict>.TryGetValue(skillId, out var value) && value.TryGetValue(rank, out var value2))
		{
			return value2;
		}
		return null;
	}
}

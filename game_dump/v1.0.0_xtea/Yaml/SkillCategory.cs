using System.Collections.Generic;

namespace Yaml;

public class SkillCategory
{
	public LevelRange level;

	public Dictionary<int, int> exp_needed;

	public Dictionary<int, int> research_times;
}

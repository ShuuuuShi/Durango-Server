using System.Collections.Generic;
using Shared.Skill;

namespace Yaml;

public class SkillYaml : Dictionary<Category, Dictionary<string, Dictionary<string, Skill[]>>>
{
}

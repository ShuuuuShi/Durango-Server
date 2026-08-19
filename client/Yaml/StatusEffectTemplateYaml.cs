using Yaml.Util;

namespace Yaml;

public class StatusEffectTemplateYaml : SingletonDict<string, StatusEffectTemplate[]>
{
	public static StatusEffectTemplate GetStatusEffectTemplate(string id, int level)
	{
		StatusEffectTemplate[] array = SingletonDict<string, StatusEffectTemplate[]>.Get(id);
		if (array != null)
		{
			StatusEffectTemplate[] array2 = array;
			foreach (StatusEffectTemplate statusEffectTemplate in array2)
			{
				if (statusEffectTemplate.MinLevel <= level && statusEffectTemplate.MaxLevel >= level)
				{
					return statusEffectTemplate;
				}
			}
		}
		return null;
	}
}

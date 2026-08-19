using Yaml.Util;

namespace Yaml;

public class StatusEffectTemplateYaml : SingletonDict<string, StatusEffectTemplate[]>
{
	public static StatusEffectTemplate GetStatusEffectTemplate(string id, int level)
	{
		StatusEffectTemplate[] array = SingletonDict<string, StatusEffectTemplate[]>.Get(id);
		int num = level - 1;
		if (array != null && array.Length > num && num >= 0)
		{
			return array[num];
		}
		return null;
	}
}

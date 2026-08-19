using Yaml.Util;

namespace Yaml;

public class AnimalYaml : SingletonDict<int, Animal>
{
	public static string GetName(int entityTypeId)
	{
		Animal animal = SingletonDict<int, Animal>.Get(entityTypeId);
		return (animal == null) ? entityTypeId.ToString() : animal.name.ToString();
	}

	public static string GetPortrait(int entityTypeId)
	{
		return SingletonDict<int, Animal>.Get(entityTypeId)?.portrait;
	}

	public static string GetId(int entityTypeId)
	{
		Animal animal = SingletonDict<int, Animal>.Get(entityTypeId);
		return (animal == null) ? entityTypeId.ToString() : animal.__name__;
	}
}

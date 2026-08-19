using Yaml.Util;

namespace Yaml;

public class AnimalYaml : SingletonDict<int, Animal>
{
	public static string GetName(int entityTypeId)
	{
		Animal animal = SingletonDict<int, Animal>.Get(entityTypeId);
		return (animal == null) ? entityTypeId.ToString() : animal.Name.ToString();
	}

	public static string GetPortrait(int entityTypeId)
	{
		return SingletonDict<int, Animal>.Get(entityTypeId)?.Portrait;
	}

	public static string GetPrefabPath(int entityTypeId)
	{
		Animal animal = SingletonDict<int, Animal>.Get(entityTypeId);
		return (animal == null) ? string.Empty : animal.PrefabPath;
	}

	public static string GetModelPath(int entityTypeId)
	{
		Animal animal = SingletonDict<int, Animal>.Get(entityTypeId);
		return (animal == null) ? string.Empty : animal.ModelPath;
	}
}

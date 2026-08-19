namespace Yaml.Util;

public class Singleton<T> : ISingletonable where T : class
{
	public static T Instance { get; private set; }

	public void Initialize(object inst)
	{
		Instance = inst as T;
	}
}

using System.Collections.Generic;
using Yaml.Util;

namespace Yaml;

public class RecipeDict : SingletonDict<string, Recipe>
{
	private static Dictionary<string, List<string[]>> _decorations;

	private static Dictionary<string, List<string[]>> Decorations
	{
		get
		{
			if (_decorations == null)
			{
				_decorations = new Dictionary<string, List<string[]>>();
				foreach (KeyValuePair<string, Recipe> item in SingletonDict<string, Recipe>.Instance)
				{
					foreach (KeyValuePair<string, string[]> item2 in item.Value.add_on)
					{
						if (!_decorations.ContainsKey(item2.Key))
						{
							_decorations.Add(item2.Key, new List<string[]>());
						}
						_decorations[item2.Key].Add(item2.Value);
					}
				}
			}
			return _decorations;
		}
	}

	public static List<string[]> GetDecorations(string blueprintId)
	{
		return Decorations.Get(blueprintId);
	}

	public static bool HasDecoration(string blueprintId)
	{
		return Decorations.ContainsKey(blueprintId);
	}
}

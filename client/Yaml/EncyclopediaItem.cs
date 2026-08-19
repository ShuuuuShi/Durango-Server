using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Yaml;

public class EncyclopediaItem
{
	[JsonProperty(PropertyName = "max_level")]
	public int MaxLevel;

	private KeyValuePair<int, KeyValuePair<string, float>[][]>[] _masteries;

	[JsonProperty(PropertyName = "masteries")]
	public Dictionary<int, List<Dictionary<string, float>>> Masteries
	{
		set
		{
			_masteries = new KeyValuePair<int, KeyValuePair<string, float>[][]>[KUtility.GetSize(value)];
			if (value != null)
			{
				int num = 0;
				foreach (KeyValuePair<int, List<Dictionary<string, float>>> item in value)
				{
					KeyValuePair<string, float>[][] array = new KeyValuePair<string, float>[KUtility.GetSize(item.Value)][];
					if (item.Value != null)
					{
						int num2 = 0;
						foreach (Dictionary<string, float> item2 in item.Value)
						{
							array[num2++] = item2.ToArray();
						}
					}
					ref KeyValuePair<int, KeyValuePair<string, float>[][]> reference = ref _masteries[num++];
					reference = new KeyValuePair<int, KeyValuePair<string, float>[][]>(item.Key, array);
				}
			}
			Array.Sort(_masteries, (KeyValuePair<int, KeyValuePair<string, float>[][]> m1, KeyValuePair<int, KeyValuePair<string, float>[][]> m2) => m1.Key - m2.Key);
		}
	}

	public KeyValuePair<int, KeyValuePair<string, float>[][]>[] GetMasteryModifiersList()
	{
		return _masteries;
	}

	public KeyValuePair<string, float>[][] GetMasteryModifiers(int lv)
	{
		if (_masteries != null)
		{
			KeyValuePair<int, KeyValuePair<string, float>[][]>[] masteries = _masteries;
			for (int i = 0; i < masteries.Length; i++)
			{
				KeyValuePair<int, KeyValuePair<string, float>[][]> keyValuePair = masteries[i];
				if (keyValuePair.Key == lv)
				{
					return keyValuePair.Value;
				}
			}
		}
		return null;
	}
}

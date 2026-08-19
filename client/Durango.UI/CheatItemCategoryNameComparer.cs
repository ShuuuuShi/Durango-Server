using System.Collections.Generic;
using Durango.Utils;

namespace Durango.UI;

public class CheatItemCategoryNameComparer : IComparer<Pair<string, string>>
{
	private readonly Dictionary<string, int> sortWeightsByName = new Dictionary<string, int>();

	public CheatItemCategoryNameComparer()
	{
		string[] array = Json.ReadFromFile<string[]>("cheat_item_categories_for_sort");
		for (int i = 0; i < array.Length; i++)
		{
			sortWeightsByName[array[i]] = i;
		}
	}

	public int Compare(Pair<string, string> x, Pair<string, string> y)
	{
		return GetWeight(x.Item2) - GetWeight(y.Item2);
	}

	private int GetWeight(string name)
	{
		int value;
		return (!sortWeightsByName.TryGetValue(name, out value)) ? int.MaxValue : value;
	}
}

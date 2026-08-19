using System.Collections.Generic;

public class CheatItemCategoryNameComparer : IComparer<KeyValuePair<string, string>>
{
	private readonly Dictionary<string, int> sortWeightsByName = new Dictionary<string, int>();

	public CheatItemCategoryNameComparer()
	{
		string[] array = KUtility.ParseJsonFile<string[]>("cheat_item_categories_for_sort");
		for (int i = 0; i < array.Length; i++)
		{
			sortWeightsByName[array[i]] = i;
		}
	}

	public int Compare(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
	{
		return GetWeight(x.Value) - GetWeight(y.Value);
	}

	private int GetWeight(string name)
	{
		int value;
		return (!sortWeightsByName.TryGetValue(name, out value)) ? int.MaxValue : value;
	}
}

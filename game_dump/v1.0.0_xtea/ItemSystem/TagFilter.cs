using System.Collections.Generic;

namespace ItemSystem;

public class TagFilter
{
	public string TagId { get; private set; }

	public int RequiredLevel { get; private set; }

	public TagFilter(string id, int level)
	{
		TagId = id;
		RequiredLevel = level;
	}

	public string GetName()
	{
		return TagData.GetTagName(TagId);
	}

	public static TagFilter[] CreateTagFilters(Dictionary<string, int> dictionary)
	{
		if (dictionary == null)
		{
			return new TagFilter[0];
		}
		if (dictionary.Count == 1 && dictionary.ContainsKey("bare_hands"))
		{
			return new TagFilter[0];
		}
		TagFilter[] array = new TagFilter[dictionary.Count];
		int num = 0;
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			TagFilter tagFilter = new TagFilter(item.Key, item.Value);
			array[num++] = tagFilter;
		}
		return array;
	}
}

using System.Collections.Generic;

namespace Durango.Logic.Item;

public class TagFilterComparer : IEqualityComparer<TagFilterBase>
{
	public bool Equals(TagFilterBase x, TagFilterBase y)
	{
		return CheckEqual(x, y);
	}

	public int GetHashCode(TagFilterBase obj)
	{
		return obj.GetName().GetHashCode();
	}

	public static bool CheckEqual(TagFilterBase x, TagFilterBase y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null || x.Count == 0)
		{
			if (y != null)
			{
				return y.Count == 0;
			}
			return true;
		}
		if (y == null || y.Count == 0)
		{
			return false;
		}
		if (x.Count != y.Count)
		{
			return false;
		}
		OrTagFilter orTagFilter = x as OrTagFilter;
		OrTagFilter orTagFilter2 = y as OrTagFilter;
		if (((object)orTagFilter == null) ^ ((object)orTagFilter2 == null))
		{
			return false;
		}
		if ((object)orTagFilter == null && (object)orTagFilter2 == null)
		{
			string text = x.FirstElementId();
			string text2 = y.FirstElementId();
			if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
			{
				return false;
			}
			if (string.Equals(text, text2))
			{
				return x.RequiredLevel() == y.RequiredLevel();
			}
			return false;
		}
		for (int i = 0; i < orTagFilter.Tags.Count; i++)
		{
			if (orTagFilter.Tags[i].Id == orTagFilter2.Tags[i].Id && orTagFilter.RequiredLevel() == orTagFilter2.RequiredLevel())
			{
				return true;
			}
		}
		return false;
	}
}

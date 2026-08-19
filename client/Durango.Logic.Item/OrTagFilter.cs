using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Durango.Logic.Item;

public class OrTagFilter : TagFilterBase
{
	[NotNull]
	public List<Tag> Tags { get; private set; }

	public override int Count => Tags.Count;

	public int Length => Tags.Count;

	public Tag this[int idx] => Tags[idx];

	public OrTagFilter()
	{
		Tags = new List<Tag>();
	}

	public OrTagFilter(List<Tag> tags)
	{
		Tags = tags;
		Tags.Sort();
	}

	public OrTagFilter(IDictionary<string, int> dictionary)
		: this()
	{
		if (dictionary == null || (dictionary.Count == 1 && dictionary.ContainsKey("bare_hands")))
		{
			return;
		}
		foreach (KeyValuePair<string, int> item in dictionary)
		{
			Tags.Add(new Tag(item.Key, item.Value));
		}
		Tags.Sort();
	}

	public OrTagFilter(string[] tags, int level = 0)
		: this()
	{
		for (int i = 0; i < KUtility.GetSize(tags); i++)
		{
			Tags.Add(new Tag(tags[i], level));
		}
	}

	public OrTagFilter(string id, int level)
		: this()
	{
		Tags.Add(new Tag(id, level));
	}

	public override bool Equals(object obj)
	{
		return TagFilterComparer.CheckEqual(this, (OrTagFilter)obj);
	}

	public override int GetHashCode()
	{
		return Tags.GetHashCode();
	}

	public static bool operator ==(OrTagFilter tagA, OrTagFilter tagB)
	{
		return TagFilterComparer.CheckEqual(tagA, tagB);
	}

	public static bool operator !=(OrTagFilter tagA, OrTagFilter tagB)
	{
		return !TagFilterComparer.CheckEqual(tagA, tagB);
	}

	public override string GetName()
	{
		return Util.LocalizedTagRequiredMsg(this);
	}

	public override int RequiredLevel()
	{
		return (Tags.Count > 0) ? Tags[0].Level : 0;
	}

	public override string[] GetIdArray()
	{
		return Tags.Select((Tag elem) => elem.Id).ToArray();
	}

	public override string FirstElementId()
	{
		return (Tags.Count <= 0) ? string.Empty : Tags.First().Id;
	}

	public static int GetLevelFromFirstTag(OrTagFilter orTags, OrTagFilter orMaterials)
	{
		int size = KUtility.GetSize(orTags.Tags);
		int size2 = KUtility.GetSize(orMaterials.Tags);
		return (size > 0) ? orTags.Tags[0].Level : ((size2 > 0) ? orMaterials.Tags[0].Level : 0);
	}
}

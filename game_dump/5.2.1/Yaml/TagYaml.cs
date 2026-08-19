using System.Collections.Generic;
using Yaml.Util;

namespace Yaml;

public class TagYaml : SingletonDict<string, Tag>
{
	private static HashSet<string> _petTags;

	protected override void OnInitalized()
	{
		base.OnInitalized();
		_petTags = new HashSet<string>();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, Tag> current = enumerator.Current;
			if (current.Value.RequiredPerformance == "animal_stat")
			{
				_petTags.Add(current.Key);
			}
		}
	}

	public static IEnumerable<string> GetPetTags()
	{
		return _petTags;
	}

	public static bool IsPetTag(string id)
	{
		if (_petTags != null)
		{
			return _petTags.Contains(id);
		}
		return false;
	}

	public static bool? IsTagImproved(string tagId, int beforeLevel, int afterLevel)
	{
		if (beforeLevel == afterLevel)
		{
			return null;
		}
		Tag tag = SingletonDict<string, Tag>.Instance.Get(tagId);
		return (tag == null || tag.Grade != 0) ? (beforeLevel < afterLevel) : (beforeLevel > afterLevel);
	}
}

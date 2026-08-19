using L10N;
using Yaml;
using Yaml.Util;

namespace ItemSystem;

public class TagData
{
	public enum DisplayType
	{
		Major,
		Minor
	}

	public enum VisibleType
	{
		ShowAll,
		HideLevel,
		Hide
	}

	private const string TypeNameMajor = "major";

	public string Id { get; private set; }

	public string Icon { get; private set; }

	public string Group { get; private set; }

	public int Level { get; private set; }

	public DisplayType Display { get; private set; }

	public VisibleType Visible { get; private set; }

	public string LocalizedName { get; private set; }

	public TagData(string id, int level, string name, string icon)
	{
		Id = id;
		Level = level;
		Icon = icon;
		Display = DisplayType.Major;
		Visible = VisibleType.Hide;
		LocalizedName = LocalizeSystem.Get(name);
	}

	private TagData(string id, int level, Tag tagData)
	{
		Id = id;
		Level = level;
		LocalizedName = tagData.name;
		Icon = tagData.icon;
		Group = tagData.group;
		Display = ((!(tagData.type == "major")) ? DisplayType.Minor : DisplayType.Major);
		Visible = ((!tagData.visible) ? VisibleType.Hide : ((!tagData.visible_level) ? VisibleType.HideLevel : VisibleType.ShowAll));
	}

	public string GetNameWithLevel()
	{
		return (Visible != 0) ? LocalizedName : GetNameWithLevel(LocalizedName, Level);
	}

	public static TagData Create(string id, int level)
	{
		Tag value;
		return (!SingletonDict<string, Tag>.TryGetValue(id, out value)) ? null : new TagData(id, level, value);
	}

	public static string GetTagName(string tagId)
	{
		Tag value;
		return (!SingletonDict<string, Tag>.TryGetValue(tagId, out value)) ? tagId : value.name.ToString();
	}

	public static string GetTagNameWithLevel(TagFilter tagFilter)
	{
		if (SingletonDict<string, Tag>.TryGetValue(tagFilter.TagId, out var value))
		{
			return GetNameWithLevel(value.name, tagFilter.RequiredLevel);
		}
		return tagFilter.TagId;
	}

	public static string GetNameWithLevel(string name, float level = 0f)
	{
		return (!(level > 0f)) ? name : T.Format("{0} {1:lv:}", name, (int)level);
	}
}

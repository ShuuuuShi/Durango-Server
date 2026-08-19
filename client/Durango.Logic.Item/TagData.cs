using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Item;

public class TagData
{
	public enum VisibleType
	{
		ShowAll,
		HideLevel,
		Hide
	}

	private static readonly Color[] GradeColor = new Color[4]
	{
		new Color32(128, 128, 128, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, 216, 91, byte.MaxValue),
		new Color32(19, 188, 103, byte.MaxValue)
	};

	public string Id { get; private set; }

	public string Group { get; private set; }

	public int Level { get; private set; }

	public VisibleType Visible { get; private set; }

	public TagGrade Grade { get; private set; }

	public TagData(string id, int level)
	{
		Id = id;
		Level = level;
		Visible = VisibleType.Hide;
		Grade = TagGrade.Neutral;
	}

	private TagData(string id, int level, Tag tagData)
	{
		Id = id;
		Level = level;
		Group = tagData.Group;
		Visible = ((!tagData.Visible) ? VisibleType.Hide : ((!tagData.VisibleLevel) ? VisibleType.HideLevel : VisibleType.ShowAll));
		Grade = tagData.Grade;
	}

	[CanBeNull]
	public static TagData Create(string id, int level)
	{
		Tag value;
		return (!SingletonDict<string, Tag>.TryGetValue(id, out value)) ? null : new TagData(id, level, value);
	}

	[NotNull]
	public static string GetTagName(string tagId)
	{
		Tag value;
		return (!SingletonDict<string, Tag>.TryGetValue(tagId, out value)) ? tagId : value.Name.ToString();
	}

	[NotNull]
	public static string GetTagIcon(string tagId)
	{
		Tag value;
		return (!SingletonDict<string, Tag>.TryGetValue(tagId, out value)) ? "icon_question" : value.Icon;
	}

	[NotNull]
	public static string GetTagNameAndPurpose(string tagId)
	{
		if (SingletonDict<string, Tag>.TryGetValue(tagId, out var value))
		{
			return (!Gettext.IsEmpty(value.Purpose)) ? $"{value.Name} ({value.Purpose})" : value.Name.ToString();
		}
		return tagId;
	}

	[NotNull]
	public static string GetTagNameWithLevel(string id, int level)
	{
		if (SingletonDict<string, Tag>.TryGetValue(id, out var value))
		{
			return GetNameWithLevel(value.Name, level);
		}
		return id;
	}

	[NotNull]
	public static string GetNameWithLevel(string name, float level = 0f)
	{
		return (!(level > 0f)) ? name : T._("{0} {1:lv:}", name, (int)level);
	}

	public static Color GetGradeColor(TagGrade grade)
	{
		return GradeColor.Get((int)grade, Color.white);
	}
}

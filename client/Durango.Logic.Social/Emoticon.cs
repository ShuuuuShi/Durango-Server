using System;
using Yaml;

namespace Durango.Logic.Social;

public class Emoticon : EmotionBase, IComparable<Emoticon>
{
	public readonly string Icon;

	public readonly string UIIcon;

	public Emoticon(Yaml.Emoticon value)
		: base(value.Id, value.Free, purchaseable: true)
	{
		Icon = value.Icon;
		UIIcon = ((!string.IsNullOrEmpty(Icon)) ? $"icon_{Icon}" : null);
	}

	public int CompareTo(Emoticon other)
	{
		if (base.FavoriteIndex.HasValue != other.FavoriteIndex.HasValue)
		{
			if (base.FavoriteIndex.HasValue)
			{
				return -1;
			}
			return 1;
		}
		if (base.FavoriteIndex.HasValue && other.FavoriteIndex.HasValue)
		{
			int num = base.FavoriteIndex.Value - other.FavoriteIndex.Value;
			if (num != 0)
			{
				return num;
			}
		}
		return string.Compare(Icon, other.Icon, StringComparison.OrdinalIgnoreCase);
	}
}

using UnityEngine;

public class UILabelPreProcesser
{
	public static string PreProcessText(UILabel label, string str)
	{
		if (!label.supportEncoding || Object.op_Implicit((Object)(object)((Component)label).GetComponent<UISpriteLabel>()))
		{
			return str;
		}
		UILabelStyleTable.CurrentLabel = label;
		UILabelStyleTable.CurrentSpriteLabel = null;
		return ResourceSingleton<UILabelStyleTable>.Instance().ReplaceStyle(str);
	}

	public static string PreProcessText(UISpriteLabel label, string str)
	{
		UILabelStyleTable.CurrentLabel = label.Label;
		UILabelStyleTable.CurrentSpriteLabel = label;
		return ResourceSingleton<UILabelStyleTable>.Instance().ReplaceStyle(str);
	}
}

using Durango.UI.Control;

public class UILabelPreProcesser
{
	public static string PreProcessText(UILabel label, string str)
	{
		if (!label.supportEncoding)
		{
			return str;
		}
		return ResourceSingleton<UILabelStyleTable>.Instance().ReplaceStyle(str, label is UISpriteLabel);
	}
}

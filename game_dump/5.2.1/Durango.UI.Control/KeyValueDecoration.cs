namespace Durango.UI.Control;

public class KeyValueDecoration : KeyValueLabel, ITextLinkWithValue, ITextLink
{
	void ITextLinkWithValue.SetPresetValue(string text)
	{
		ParamsDictionary paramsDictionary = ParamsDictionary.MakeParams(text);
		string text2;
		string text3;
		if (paramsDictionary != null)
		{
			text2 = paramsDictionary.Get("key");
			text3 = paramsDictionary.Get("value");
		}
		else
		{
			text2 = text;
			text3 = null;
		}
		Set(text2, text3);
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		SetFontSize(size);
		if (builder.Width < 1000000)
		{
			UpdateLayout(builder.Width);
		}
		else
		{
			UpdateLayout();
		}
		LinkLayoutOption result = default(LinkLayoutOption);
		result.IsSingle = true;
		return result;
	}
}

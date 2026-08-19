using System;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions;

public class DefaultFormatter : IFormatter
{
	private string[] names = new string[3]
	{
		"default",
		"d",
		string.Empty
	};

	public string[] Names
	{
		get
		{
			return names;
		}
		set
		{
			names = value;
		}
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		Format format = formattingInfo.Format;
		object obj = formattingInfo.CurrentValue;
		if (format != null && format.HasNested)
		{
			formattingInfo.Write(format, obj);
			return true;
		}
		if (obj == null)
		{
			obj = string.Empty;
		}
		string text = null;
		IFormatProvider provider = formattingInfo.FormatDetails.Provider;
		if (provider != null && provider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter customFormatter)
		{
			string format2 = format?.GetLiteralText();
			text = customFormatter.Format(format2, obj, provider);
		}
		else if (obj is IFormattable formattable)
		{
			string text2 = format?.ToString();
			text = formattable.ToString(text2, provider);
		}
		else
		{
			text = obj.ToString();
		}
		if (formattingInfo.Alignment > 0)
		{
			int num = formattingInfo.Alignment - text.Length;
			if (num > 0)
			{
				formattingInfo.Write(new string(' ', num));
			}
		}
		formattingInfo.Write(text);
		if (formattingInfo.Alignment < 0)
		{
			int num2 = -formattingInfo.Alignment - text.Length;
			if (num2 > 0)
			{
				formattingInfo.Write(new string(' ', num2));
			}
		}
		return true;
	}
}

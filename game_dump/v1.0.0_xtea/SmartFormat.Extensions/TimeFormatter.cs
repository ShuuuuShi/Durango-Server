using System;
using System.Globalization;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;

public class TimeFormatter : IFormatter
{
	private string[] names = new string[4]
	{
		"timespan",
		"time",
		"t",
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

	public TimeSpanFormatOptions DefaultFormatOptions { get; set; }

	public string DefaultTwoLetterISOLanguageName { get; set; }

	public TimeFormatter()
		: this(null)
	{
	}

	public TimeFormatter(string defaultTwoLetterLanguageName)
	{
		DefaultFormatOptions = TimeSpanUtility.DefaultFormatOptions;
		DefaultTwoLetterISOLanguageName = defaultTwoLetterLanguageName;
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		Format format = formattingInfo.Format;
		object currentValue = formattingInfo.CurrentValue;
		if (format != null && format.HasNested)
		{
			return false;
		}
		string text = ((formattingInfo.FormatterOptions != string.Empty) ? formattingInfo.FormatterOptions : ((format == null) ? string.Empty : format.GetLiteralText()));
		TimeSpan fromTime;
		if (currentValue is TimeSpan)
		{
			fromTime = (TimeSpan)currentValue;
		}
		else if (currentValue is DateTime && formattingInfo.FormatterOptions != string.Empty)
		{
			fromTime = DateTime.Now.Subtract((DateTime)currentValue);
		}
		else
		{
			if (!(currentValue is DateTime) || !text.StartsWith("timestring"))
			{
				return false;
			}
			text = text.Substring(10);
			fromTime = DateTime.Now.Subtract((DateTime)currentValue);
		}
		TimeTextInfo timeTextInfo = GetTimeTextInfo(formattingInfo.FormatDetails.Provider);
		if (timeTextInfo == null)
		{
			return false;
		}
		TimeSpanFormatOptions options = TimeSpanFormatOptionsConverter.Parse(text);
		string text2 = fromTime.ToTimeString(options, timeTextInfo);
		formattingInfo.Write(text2);
		return true;
	}

	private TimeTextInfo GetTimeTextInfo(IFormatProvider provider)
	{
		if (provider != null)
		{
			TimeTextInfo timeTextInfo = (TimeTextInfo)provider.GetFormat(typeof(TimeTextInfo));
			if (timeTextInfo != null)
			{
				return timeTextInfo;
			}
			if (provider is CultureInfo cultureInfo)
			{
				return CommonLanguagesTimeTextInfo.GetTimeTextInfo(cultureInfo.TwoLetterISOLanguageName);
			}
		}
		return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);
	}
}

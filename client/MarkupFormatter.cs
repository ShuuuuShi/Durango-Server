using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartFormat;
using SmartFormat.Core.Extensions;

public class MarkupFormatter : IFormatter
{
	private static readonly Dictionary<string, Dictionary<string, string>> Markups = new Dictionary<string, Dictionary<string, string>>
	{
		{
			"lv",
			// [แก้เอง] เดิมไม่มี "th" เลย ทั้งที่เกมรันเป็นภาษาไทย — เพิ่มเข้าไปให้ตรง locale จริง
			new Dictionary<string, string>
			{
				{ "en", "Lv.\u00a0{0}" },
				{ "es", "Nv.\u00a0{0}" },
				{ "pt", "Nvl.\u00a0{0}" },
				{ "ru", "Ур.\u00a0{0}" },
				{ "de", "St.\u00a0{0}" },
				{ "th", "Lv.\u00a0{0}" }
			}
		},
		{
			"pt",
			new Dictionary<string, string>
			{
				{ "en", "{0}\u00a0pt" },
				{ "es", "{0}\u00a0pts." },
				{ "pt", "{0}\u00a0pts" },
				{ "ru", "{0}\u00a0оч." },
				{ "de", "{0}\u00a0Pkt." },
				{ "fr", "{0}\u00a0pts" }
			}
		}
	};

	private string[] _names;

	private readonly SmartFormatter _formatter;

	public string[] Names
	{
		get
		{
			if (_names == null)
			{
				_names = Markups.Keys.ToArray();
			}
			return _names;
		}
		set
		{
			_names = value;
		}
	}

	public MarkupFormatter(SmartFormatter formatter)
	{
		_formatter = formatter;
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		string text = "en";
		IFormatProvider provider = formattingInfo.FormatDetails.Provider;
		if (provider is CultureInfo cultureInfo)
		{
			text = cultureInfo.TwoLetterISOLanguageName;
		}
		string formatterName = formattingInfo.Placeholder.FormatterName;
		if (Markups.TryGetValue(formatterName, out var value))
		{
			if (!value.ContainsKey(text) && text != "en")
			{
				text = "en";
			}
			if (value.TryGetValue(text, out var value2))
			{
				string text2 = _formatter.Format(value2, formattingInfo.CurrentValue);
				formattingInfo.Write(text2);
				return true;
			}
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;

public class PluralLocalizationFormatter : IFormatter
{
	private string[] names = new string[3]
	{
		"plural",
		"p",
		string.Empty
	};

	private PluralRules.PluralRuleDelegate defaultPluralRule;

	private string defaultTwoLetterISOLanguageName;

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

	public string DefaultTwoLetterISOLanguageName
	{
		get
		{
			return defaultTwoLetterISOLanguageName;
		}
		set
		{
			defaultTwoLetterISOLanguageName = value;
			defaultPluralRule = PluralRules.GetPluralRule(value);
		}
	}

	public PluralLocalizationFormatter(string defaultTwoLetterISOLanguageName)
	{
		DefaultTwoLetterISOLanguageName = defaultTwoLetterISOLanguageName;
	}

	private PluralRules.PluralRuleDelegate GetPluralRule(IFormattingInfo formattingInfo)
	{
		string formatterOptions = formattingInfo.FormatterOptions;
		if (formatterOptions.Length != 0)
		{
			return PluralRules.GetPluralRule(formatterOptions);
		}
		IFormatProvider provider = formattingInfo.FormatDetails.Provider;
		if (provider != null)
		{
			CustomPluralRuleProvider customPluralRuleProvider = (CustomPluralRuleProvider)provider.GetFormat(typeof(CustomPluralRuleProvider));
			if (customPluralRuleProvider != null)
			{
				return customPluralRuleProvider.GetPluralRule();
			}
		}
		if (provider is CultureInfo cultureInfo)
		{
			return PluralRules.GetPluralRule(cultureInfo.TwoLetterISOLanguageName);
		}
		if (defaultPluralRule != null)
		{
			return defaultPluralRule;
		}
		return null;
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		Format format = formattingInfo.Format;
		object currentValue = formattingInfo.CurrentValue;
		if (format == null || format.baseString[format.startIndex] == ':')
		{
			return false;
		}
		IList<Format> list = format.Split('|');
		if (list.Count == 1)
		{
			return false;
		}
		if (!(currentValue is byte) && !(currentValue is short) && !(currentValue is int) && !(currentValue is long) && !(currentValue is float) && !(currentValue is double) && !(currentValue is decimal))
		{
			return false;
		}
		decimal value = Convert.ToDecimal(currentValue);
		PluralRules.PluralRuleDelegate pluralRule = GetPluralRule(formattingInfo);
		if (pluralRule == null)
		{
			return false;
		}
		int count = list.Count;
		int num = pluralRule(value, count);
		if (num < 0 || list.Count <= num)
		{
			throw new FormattingException(format, "Invalid number of plural parameters", list.Last().endIndex);
		}
		Format format2 = list[num];
		formattingInfo.Write(format2, currentValue);
		return true;
	}
}

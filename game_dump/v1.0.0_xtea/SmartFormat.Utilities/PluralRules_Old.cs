namespace SmartFormat.Utilities;

public static class PluralRules_Old
{
	public static PluralRules.PluralRuleDelegate GetPluralRule(string twoLetterISOLanguageName)
	{
		switch (twoLetterISOLanguageName)
		{
		case "en":
		case "de":
		case "nl":
		case "sv":
		case "da":
		case "no":
		case "nn":
		case "nb":
		case "fo":
		case "es":
		case "pt":
		case "it":
		case "bg":
		case "el":
		case "fi":
		case "et":
		case "he":
		case "eo":
		case "hu":
		case "tr":
			return English_Special;
		case "fr":
			return French;
		case "lv":
			return Latvian;
		case "ga":
			return Irish;
		case "ro":
			return Romanian;
		case "lt":
			return Lithuanian;
		case "ru":
		case "uk":
		case "sr":
		case "hr":
			return Russian;
		case "cs":
		case "sk":
			return Czech;
		case "pl":
			return Polish;
		case "sl":
			return Slovenian;
		default:
			return null;
		}
	}

	public static int English_Special(decimal value, int pluralCount)
	{
		return pluralCount switch
		{
			2 => (!(value == 1m)) ? 1 : 0, 
			3 => (!(value == 0m)) ? ((value == 1m) ? 1 : 2) : 0, 
			4 => (!(value < 0m)) ? ((value == 0m) ? 1 : ((!(value == 1m)) ? 3 : 2)) : 0, 
			_ => -1, 
		};
	}

	public static int French(decimal value, int pluralCount)
	{
		if (pluralCount == 2)
		{
			return (!(value == 0m) && !(value == 1m)) ? 1 : 0;
		}
		return -1;
	}

	public static int Latvian(decimal value, int pluralCount)
	{
		if (pluralCount == 3)
		{
			return (!(value % 10m == 1m) || !(value % 100m != 11m)) ? ((value != 0m) ? 1 : 2) : 0;
		}
		return -1;
	}

	public static int Irish(decimal value, int pluralCount)
	{
		if (pluralCount == 3)
		{
			return (!(value == 1m)) ? ((value == 2m) ? 1 : 2) : 0;
		}
		return -1;
	}

	public static int Romanian(decimal value, int pluralCount)
	{
		if (pluralCount == 3)
		{
			return (!(value == 1m)) ? ((value == 0m || (value % 100m > 0m && value % 100m < 20m)) ? 1 : 2) : 0;
		}
		return -1;
	}

	public static int Lithuanian(decimal value, int pluralCount)
	{
		if (pluralCount == 3)
		{
			return (!(value % 10m == 1m) || !(value % 100m != 11m)) ? ((value % 10m >= 2m && (value % 100m < 10m || value % 100m >= 20m)) ? 1 : 2) : 0;
		}
		return -1;
	}

	public static int Russian(decimal value, int pluralCount)
	{
		if (pluralCount == 3)
		{
			return (!(value % 10m == 1m) || !(value % 100m != 11m)) ? ((value % 10m >= 2m && value % 10m <= 4m && (value % 100m < 10m || value % 100m >= 20m)) ? 1 : 2) : 0;
		}
		return -1;
	}

	public static int Czech(decimal value, int pluralCount)
	{
		if (pluralCount == 3)
		{
			return (!(value == 1m)) ? ((value >= 2m && value <= 4m) ? 1 : 2) : 0;
		}
		return -1;
	}

	public static int Polish(decimal value, int pluralCount)
	{
		if (pluralCount == 3)
		{
			return (!(value == 1m)) ? ((value % 10m >= 2m && value % 10m <= 4m && (value % 100m < 10m || value % 100m >= 20m)) ? 1 : 2) : 0;
		}
		return -1;
	}

	public static int Slovenian(decimal value, int pluralCount)
	{
		if (pluralCount == 4)
		{
			return (!(value % 100m == 1m)) ? ((value % 100m == 2m) ? 1 : ((!(value % 100m == 3m) && !(value % 100m == 4m)) ? 3 : 2)) : 0;
		}
		return -1;
	}
}

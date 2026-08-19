namespace SmartFormat.Utilities;

public static class PluralRules
{
	public delegate int PluralRuleDelegate(decimal value, int pluralCount);

	public static PluralRuleDelegate GetPluralRule(string twoLetterISOLanguageName)
	{
		switch (twoLetterISOLanguageName)
		{
		case "az":
		case "bm":
		case "bo":
		case "dz":
		case "fa":
		case "hu":
		case "id":
		case "ig":
		case "ii":
		case "ja":
		case "jv":
		case "ka":
		case "kde":
		case "kea":
		case "km":
		case "kn":
		case "ko":
		case "ms":
		case "my":
		case "root":
		case "sah":
		case "ses":
		case "sg":
		case "th":
		case "to":
		case "vi":
		case "wo":
		case "yo":
		case "zh":
			return (decimal n, int c) => 0;
		case "af":
		case "bem":
		case "bg":
		case "bn":
		case "brx":
		case "ca":
		case "cgg":
		case "chr":
		case "da":
		case "de":
		case "dv":
		case "ee":
		case "el":
		case "en":
		case "eo":
		case "es":
		case "et":
		case "eu":
		case "fi":
		case "fo":
		case "fur":
		case "fy":
		case "gl":
		case "gsw":
		case "gu":
		case "ha":
		case "haw":
		case "he":
		case "is":
		case "it":
		case "kk":
		case "kl":
		case "ku":
		case "lb":
		case "lg":
		case "lo":
		case "mas":
		case "ml":
		case "mn":
		case "mr":
		case "nah":
		case "nb":
		case "ne":
		case "nl":
		case "nn":
		case "no":
		case "nyn":
		case "om":
		case "or":
		case "pa":
		case "pap":
		case "ps":
		case "pt":
		case "rm":
		case "saq":
		case "so":
		case "sq":
		case "ssy":
		case "sw":
		case "sv":
		case "syr":
		case "ta":
		case "te":
		case "tk":
		case "tr":
		case "ur":
		case "wae":
		case "xog":
		case "zu":
			return (decimal n, int c) => c switch
			{
				2 => (!(n == 1m)) ? 1 : 0, 
				3 => (!(n == 0m)) ? ((n == 1m) ? 1 : 2) : 0, 
				4 => (!(n < 0m)) ? ((n == 0m) ? 1 : ((!(n == 1m)) ? 3 : 2)) : 0, 
				_ => -1, 
			};
		case "ak":
		case "am":
		case "bh":
		case "fil":
		case "guw":
		case "hi":
		case "ln":
		case "mg":
		case "nso":
		case "ti":
		case "tl":
		case "wa":
			return (decimal n, int c) => (!(n == 0m) && !(n == 1m)) ? 1 : 0;
		case "ff":
		case "fr":
		case "kab":
			return (decimal n, int c) => (!(n >= 0m) || !(n < 2m)) ? 1 : 0;
		case "ga":
		case "iu":
		case "ksh":
		case "kw":
		case "se":
		case "sma":
		case "smi":
		case "smj":
		case "smn":
		case "sms":
			return (decimal n, int c) => (!(n == 1m)) ? ((n == 2m) ? 1 : 2) : 0;
		case "be":
		case "bs":
		case "hr":
		case "ru":
		case "sh":
		case "sr":
		case "uk":
			return (decimal n, int c) => (!(n % 10m == 1m) || n % 100m == 11m) ? (((n % 10m).Between(2m, 4m) && !(n % 100m).Between(12m, 14m)) ? 1 : 2) : 0;
		case "ar":
			return (decimal n, int c) => (!(n == 0m)) ? ((n == 1m) ? 1 : ((n == 2m) ? 2 : ((n % 100m).Between(3m, 10m) ? 3 : ((!(n % 100m).Between(11m, 99m)) ? 5 : 4)))) : 0;
		case "br":
			return (decimal n, int c) => (!(n == 0m)) ? ((n == 1m) ? 1 : ((n == 2m) ? 2 : ((n == 3m) ? 3 : ((!(n == 6m)) ? 5 : 4)))) : 0;
		case "cs":
			return (decimal n, int c) => (!(n == 1m)) ? (n.Between(2m, 4m) ? 1 : 2) : 0;
		case "cy":
			return (decimal n, int c) => (!(n == 0m)) ? ((n == 1m) ? 1 : ((n == 2m) ? 2 : ((n == 3m) ? 3 : ((!(n == 6m)) ? 5 : 4)))) : 0;
		case "gv":
			return (decimal n, int c) => (!(n % 10m).Between(1m, 2m) && !(n % 20m == 0m)) ? 1 : 0;
		case "lag":
			return (decimal n, int c) => (!(n == 0m)) ? ((n > 0m && n < 2m) ? 1 : 2) : 0;
		case "lt":
			return (decimal n, int c) => (!(n % 10m == 1m) || (n % 100m).Between(11m, 19m)) ? (((n % 10m).Between(2m, 9m) && !(n % 100m).Between(11m, 19m)) ? 1 : 2) : 0;
		case "lv":
			return (decimal n, int c) => (!(n == 0m)) ? ((n % 10m == 1m && n % 100m != 11m) ? 1 : 2) : 0;
		case "mb":
			return (decimal n, int c) => (!(n % 10m == 1m) || !(n != 11m)) ? 1 : 0;
		case "mo":
			return (decimal n, int c) => (!(n == 1m)) ? ((n == 0m || (n != 1m && (n % 100m).Between(1m, 19m))) ? 1 : 2) : 0;
		case "mt":
			return (decimal n, int c) => (!(n == 1m)) ? ((n == 0m || (n % 100m).Between(2m, 10m)) ? 1 : ((!(n % 100m).Between(11m, 19m)) ? 3 : 2)) : 0;
		case "pl":
			return (decimal n, int c) => (!(n == 1m)) ? (((n % 10m).Between(2m, 4m) && !(n % 100m).Between(12m, 14m)) ? 1 : ((!(n % 10m).Between(0m, 1m) && !(n % 10m).Between(5m, 9m) && !(n % 100m).Between(12m, 14m)) ? 3 : 2)) : 0;
		case "ro":
			return (decimal n, int c) => (!(n == 1m)) ? ((n == 0m || (n % 100m).Between(1m, 19m)) ? 1 : 2) : 0;
		case "shi":
			return (decimal n, int c) => (!(n >= 0m) || !(n <= 1m)) ? (n.Between(2m, 10m) ? 1 : 2) : 0;
		case "sk":
			return (decimal n, int c) => (!(n == 1m)) ? (n.Between(2m, 4m) ? 1 : 2) : 0;
		case "sl":
			return (decimal n, int c) => (!(n % 100m == 1m)) ? ((n % 100m == 2m) ? 1 : ((!(n % 100m).Between(3m, 4m)) ? 3 : 2)) : 0;
		case "tzm":
			return (decimal n, int c) => (!n.Between(0m, 1m) && !n.Between(11m, 99m)) ? 1 : 0;
		default:
			return null;
		}
	}

	private static bool Between(this decimal value, decimal min, decimal max)
	{
		return value % 1m == 0m && value >= min && value <= max;
	}
}

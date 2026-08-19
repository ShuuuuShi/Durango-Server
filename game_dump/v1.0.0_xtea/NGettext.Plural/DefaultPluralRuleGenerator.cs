using System.Globalization;

namespace NGettext.Plural;

public class DefaultPluralRuleGenerator : IPluralRuleGenerator
{
	public virtual IPluralRule CreateRule(CultureInfo cultureInfo)
	{
		switch (cultureInfo.TwoLetterISOLanguageName)
		{
		case "az":
		case "bm":
		case "bo":
		case "dz":
		case "fa":
		case "id":
		case "ig":
		case "ii":
		case "hu":
		case "ja":
		case "jv":
		case "ka":
		case "kde":
		case "kea":
		case "km":
		case "kn":
		case "ko":
		case "lo":
		case "ms":
		case "my":
		case "sah":
		case "ses":
		case "sg":
		case "th":
		case "to":
		case "tr":
		case "vi":
		case "wo":
		case "yo":
		case "zh":
			return new PluralRule(1, (long number) => 0);
		case "ar":
			return new PluralRule(6, delegate(long number)
			{
				int result3;
				switch (number)
				{
				case 0L:
					result3 = 0;
					break;
				case 1L:
					result3 = 1;
					break;
				case 2L:
					result3 = 2;
					break;
				case 3L:
				case 4L:
				case 5L:
				case 6L:
				case 7L:
				case 8L:
				case 9L:
				case 10L:
					result3 = 3;
					break;
				default:
					result3 = ((number < 11 || number > 99) ? 5 : 4);
					break;
				}
				return result3;
			});
		case "asa":
		case "af":
		case "bem":
		case "bez":
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
		case "jmc":
		case "kaj":
		case "kcg":
		case "kk":
		case "kl":
		case "ksb":
		case "ku":
		case "lb":
		case "lg":
		case "mas":
		case "ml":
		case "mn":
		case "mr":
		case "nah":
		case "nb":
		case "nd":
		case "ne":
		case "nl":
		case "nn":
		case "no":
		case "nr":
		case "ny":
		case "nyn":
		case "om":
		case "or":
		case "pa":
		case "pap":
		case "ps":
		case "pt":
		case "rof":
		case "rm":
		case "rwk":
		case "saq":
		case "seh":
		case "sn":
		case "so":
		case "sq":
		case "ss":
		case "ssy":
		case "st":
		case "sv":
		case "sw":
		case "syr":
		case "ta":
		case "te":
		case "teo":
		case "tig":
		case "tk":
		case "tn":
		case "ts":
		case "ur":
		case "wae":
		case "ve":
		case "vun":
		case "xh":
		case "xog":
		case "zu":
			return new PluralRule(2, (long number) => (number != 1) ? 1 : 0);
		case "ak":
		case "am":
		case "bh":
		case "fil":
		case "ff":
		case "fr":
		case "guw":
		case "hi":
		case "kab":
		case "ln":
		case "mg":
		case "nso":
		case "ti":
		case "wa":
			return new PluralRule(2, (long number) => (number != 0L && number != 1) ? 1 : 0);
		case "lv":
			return new PluralRule(3, (long number) => (number != 0L) ? ((number % 10 == 1 && number % 100 != 11) ? 1 : 2) : 0);
		case "iu":
		case "kw":
		case "naq":
		case "se":
		case "sma":
		case "smi":
		case "smj":
		case "smn":
		case "sms":
			return new PluralRule(3, (long number) => number switch
			{
				1L => 0, 
				2L => 1, 
				_ => 2, 
			});
		case "ga":
			return new PluralRule(5, delegate(long number)
			{
				int result;
				switch (number)
				{
				case 1L:
					result = 0;
					break;
				case 2L:
					result = 1;
					break;
				case 3L:
				case 4L:
				case 5L:
				case 6L:
					result = 2;
					break;
				default:
					result = ((number < 7 || number > 10) ? 4 : 3);
					break;
				}
				return result;
			});
		case "ro":
		case "mo":
			return new PluralRule(3, (long number) => (number != 1) ? ((number == 0L || (number % 100 > 0 && number % 100 < 20)) ? 1 : 2) : 0);
		case "lt":
			return new PluralRule(3, (long number) => (number % 10 != 1 || number % 100 == 11) ? ((number % 10 >= 2 && (number % 100 < 10 || number % 100 >= 20)) ? 1 : 2) : 0);
		case "be":
		case "bs":
		case "hr":
		case "ru":
		case "sh":
		case "sr":
		case "uk":
			return new PluralRule(3, (long number) => (number % 10 != 1 || number % 100 == 11) ? ((number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 10 || number % 100 >= 20)) ? 1 : 2) : 0);
		case "cs":
		case "sk":
			return new PluralRule(3, delegate(long number)
			{
				int result2;
				switch (number)
				{
				case 1L:
					result2 = 0;
					break;
				case 2L:
				case 3L:
				case 4L:
					result2 = 1;
					break;
				default:
					result2 = 2;
					break;
				}
				return result2;
			});
		case "pl":
			return new PluralRule(3, (long number) => (number != 1) ? ((number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 12 || number % 100 > 14)) ? 1 : 2) : 0);
		case "sl":
			return new PluralRule(4, (long number) => (number % 100 != 1) ? ((number % 100 == 2) ? 1 : ((number % 100 != 3 && number % 100 != 4) ? 3 : 2)) : 0);
		case "mt":
			return new PluralRule(4, (long number) => (number != 1) ? ((number == 0L || (number % 100 > 1 && number % 100 < 11)) ? 1 : ((number % 100 <= 10 || number % 100 >= 20) ? 3 : 2)) : 0);
		case "mk":
			return new PluralRule(2, (long number) => (number % 10 != 1 || number == 11) ? 1 : 0);
		case "cy":
			return new PluralRule(6, (long number) => number switch
			{
				0L => 0, 
				1L => 1, 
				2L => 2, 
				3L => 3, 
				6L => 4, 
				_ => 5, 
			});
		case "lag":
		case "ksh":
			return new PluralRule(3, (long number) => number switch
			{
				0L => 0, 
				1L => 1, 
				_ => 2, 
			});
		case "shi":
			return new PluralRule(3, (long number) => (number != 0L || number != 1) ? ((number >= 2 && number <= 10) ? 1 : 2) : 0);
		case "tzm":
			return new PluralRule(2, delegate(long number)
			{
				int result4;
				switch (number)
				{
				case 0L:
				case 1L:
				case 11L:
				case 12L:
				case 13L:
				case 14L:
				case 15L:
				case 16L:
				case 17L:
				case 18L:
				case 19L:
				case 20L:
				case 21L:
				case 22L:
				case 23L:
				case 24L:
				case 25L:
				case 26L:
				case 27L:
				case 28L:
				case 29L:
				case 30L:
				case 31L:
				case 32L:
				case 33L:
				case 34L:
				case 35L:
				case 36L:
				case 37L:
				case 38L:
				case 39L:
				case 40L:
				case 41L:
				case 42L:
				case 43L:
				case 44L:
				case 45L:
				case 46L:
				case 47L:
				case 48L:
				case 49L:
				case 50L:
				case 51L:
				case 52L:
				case 53L:
				case 54L:
				case 55L:
				case 56L:
				case 57L:
				case 58L:
				case 59L:
				case 60L:
				case 61L:
				case 62L:
				case 63L:
				case 64L:
				case 65L:
				case 66L:
				case 67L:
				case 68L:
				case 69L:
				case 70L:
				case 71L:
				case 72L:
				case 73L:
				case 74L:
				case 75L:
				case 76L:
				case 77L:
				case 78L:
				case 79L:
				case 80L:
				case 81L:
				case 82L:
				case 83L:
				case 84L:
				case 85L:
				case 86L:
				case 87L:
				case 88L:
				case 89L:
				case 90L:
				case 91L:
				case 92L:
				case 93L:
				case 94L:
				case 95L:
				case 96L:
				case 97L:
				case 98L:
				case 99L:
					result4 = 0;
					break;
				default:
					result4 = 1;
					break;
				}
				return result4;
			});
		case "gv":
			return new PluralRule(2, (long number) => (number % 10 != 1 && number % 10 != 2 && number % 20 != 0L) ? 1 : 0);
		case "gd":
			return new PluralRule(4, (long number) => (number != 1 && number != 11) ? ((number == 2 || number == 12) ? 1 : (((number < 3 || number > 10) && (number < 13 || number > 19)) ? 3 : 2)) : 0);
		default:
			return PluralRule.Default;
		}
	}
}

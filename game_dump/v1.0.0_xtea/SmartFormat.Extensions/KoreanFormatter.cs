using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using SmartFormat.Utilities;

namespace SmartFormat.Extensions;

public class KoreanFormatter : IFormatter
{
	internal class SyllableInfo
	{
		public bool HasCoda;

		public bool HasRieulCoda;

		public SyllableInfo(char hangulChar)
		{
			int num = (hangulChar - 44032) % 28;
			HasCoda = num != 0;
			HasRieulCoda = num == 8;
		}
	}

	private string[] names = new string[2]
	{
		"ko",
		string.Empty
	};

	private readonly SmartFormatter _formatter;

	private Hangul _hangul = new Hangul();

	private readonly Regex _filterPattern = new Regex("\\(.*[^\\(]?\\)|[!@#$%^$*?,.:;'\"\\[\\]{}<>]+");

	private readonly string[] _simpleParticles = new string[5] { "을를", "아야", "이가", "은는", "과와" };

	private readonly string[] _idaExcepts = new string[2] { "여", "시여" };

	private readonly Regex _invariantParticlePattern = new Regex("^((의|도|만|보다|부터|까지|마저|조차)$|에|께|하)");

	private readonly Regex _euroPattern = new Regex("^(으|\\(으\\))?로");

	private readonly Regex _idaPrefixPattern = new Regex("^이|\\(이\\)");

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

	public KoreanFormatter(SmartFormatter formatter)
	{
		_formatter = formatter;
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		if (formattingInfo.Format == null || string.IsNullOrEmpty(formattingInfo.Format.RawText))
		{
			return false;
		}
		string text = null;
		text = ((!(formattingInfo.CurrentValue is string)) ? formattingInfo.CurrentValue.ToString() : ((string)formattingInfo.CurrentValue));
		SyllableInfo syllableInfo = EvaluateSyllable(text);
		string text2 = formattingInfo.FormatterOptions;
		bool flag = string.IsNullOrEmpty(text2);
		bool flag2 = false;
		if (flag)
		{
			text2 = formattingInfo.Format.RawText;
			flag2 = text2[0] == '-';
			if (flag2)
			{
				text2 = text2.Substring(1);
			}
		}
		string text3 = ParticleConverter(text2, syllableInfo);
		if (string.IsNullOrEmpty(text3))
		{
			return false;
		}
		if (flag2)
		{
			formattingInfo.Write(text3);
		}
		else if (flag)
		{
			formattingInfo.Write(text);
			formattingInfo.Write(text3);
		}
		else
		{
			Format format = _formatter.Parser.ParseFormat(formattingInfo.Format.RawText + text3);
			formattingInfo.Write(format, formattingInfo.CurrentValue);
		}
		return true;
	}

	private bool TryParseIda(string format, SyllableInfo syllableInfo, out string result)
	{
		string text = _idaPrefixPattern.Replace(format, string.Empty);
		if (string.IsNullOrEmpty(text))
		{
			result = null;
			return false;
		}
		if (!_idaExcepts.Contains(text))
		{
			char[] array = _hangul.SplitPhonemes(text[0]);
			if (array == null)
			{
				result = null;
				return false;
			}
			char c = array[0];
			char c2 = array[1];
			char coda = array[2];
			if (c == 'ㅇ')
			{
				if (c2 == 'ㅣ')
				{
					result = text;
					return true;
				}
				bool flag = syllableInfo?.HasCoda ?? true;
				char c3 = '\0';
				if (!flag && (c2 == 'ㅓ' || c2 == 'ㅔ'))
				{
					c3 = ((c2 != 'ㅓ') ? 'ㅖ' : 'ㅕ');
				}
				else if (flag && (c2 == 'ㅕ' || c2 == 'ㅖ'))
				{
					c3 = ((c2 != 'ㅕ') ? 'ㅔ' : 'ㅓ');
				}
				if (c3 != 0)
				{
					char c4 = _hangul.JoinPhonemes('ㅇ', c3, coda);
					text = c4 + text.Substring(1);
				}
			}
		}
		if (syllableInfo == null)
		{
			result = "(이)" + text;
		}
		else
		{
			result = ((!syllableInfo.HasCoda) ? text : ('이' + text));
		}
		return true;
	}

	private SyllableInfo EvaluateSyllable(string value)
	{
		string text = _filterPattern.Replace(value, string.Empty);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		char c = text[text.Length - 1];
		if (Hangul.IsNumericChar(c))
		{
			c = _hangul.PickLastHangulCharacterFromNumber(value);
		}
		if ('가' > c || c > '힣')
		{
			return null;
		}
		return new SyllableInfo(c);
	}

	private string ParticleConverter(string josaFormat, SyllableInfo syllableInfo)
	{
		if (josaFormat.Length == 1)
		{
			char c = josaFormat[0];
			string[] simpleParticles = _simpleParticles;
			foreach (string text in simpleParticles)
			{
				if (c == text[0] || c == text[1])
				{
					if (syllableInfo == null)
					{
						return $"{text[0]}({text[1]})";
					}
					int index = ((!syllableInfo.HasCoda) ? 1 : 0);
					return text[index].ToString();
				}
			}
		}
		Match match = _euroPattern.Match(josaFormat);
		if (match.Success)
		{
			string text2 = josaFormat.Substring(match.Value.Length);
			if (syllableInfo == null)
			{
				return $"(으)로{text2}";
			}
			return (syllableInfo.HasCoda && !syllableInfo.HasRieulCoda) ? ("으로" + text2) : ('로' + text2);
		}
		if (_invariantParticlePattern.IsMatch(josaFormat))
		{
			return josaFormat;
		}
		string result = null;
		if (TryParseIda(josaFormat, syllableInfo, out result))
		{
			return result;
		}
		return null;
	}
}

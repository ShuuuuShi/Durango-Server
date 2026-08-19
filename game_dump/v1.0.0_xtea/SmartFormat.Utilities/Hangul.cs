using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartFormat.Utilities;

public class Hangul
{
	private readonly char[] _onsets = new char[19]
	{
		'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ',
		'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'
	};

	private readonly char[] _nucleuses = new char[21]
	{
		'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ',
		'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ',
		'ㅣ'
	};

	private readonly char[] _codas = new char[28]
	{
		'\0', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ',
		'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ',
		'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ'
	};

	private readonly string _hangulDigits = "영일이삼사오육칠팔구";

	private readonly Dictionary<int, char> _hangul10Digits = new Dictionary<int, char>
	{
		{ 1, '십' },
		{ 2, '백' },
		{ 3, '천' },
		{ 4, '만' },
		{ 8, '억' },
		{ 12, '조' },
		{ 16, '경' },
		{ 20, '해' },
		{ 24, '자' },
		{ 28, '양' },
		{ 32, '구' },
		{ 36, '간' },
		{ 40, '정' },
		{ 44, '재' },
		{ 48, '극' },
		{ 52, '사' },
		{ 56, '기' },
		{ 60, '타' },
		{ 64, '의' },
		{ 68, '수' },
		{ 72, '겁' },
		{ 76, '업' }
	};

	public char JoinPhonemes(char onset, char nucleus, char coda = '\0')
	{
		return (char)((Array.IndexOf(_onsets, onset) * _nucleuses.Length + Array.IndexOf(_nucleuses, nucleus)) * _codas.Length + Array.IndexOf(_codas, coda) + 44032);
	}

	public char[] SplitPhonemes(char letter, bool onset = true, bool nucleus = true, bool coda = true)
	{
		if ('가' > letter || letter > '힣')
		{
			return null;
		}
		char[] array = new char[3];
		int num = letter - 44032;
		if (onset)
		{
			array[0] = _onsets[num / (_nucleuses.Length * _codas.Length)];
		}
		if (nucleus)
		{
			array[1] = _nucleuses[num / _codas.Length % _nucleuses.Length];
		}
		if (coda)
		{
			array[2] = _codas[num % _codas.Length];
		}
		return array;
	}

	public static bool IsNumericChar(char c)
	{
		return '0' <= c && c <= '9';
	}

	public char PickLastHangulCharacterFromNumber(string value)
	{
		int num = value.Length;
		int num2 = value.Length - 1;
		while (num2 >= 0 && IsNumericChar(value[num2]))
		{
			num = num2;
			if (value[num2] != '0')
			{
				break;
			}
			num2--;
		}
		int num3 = value.Length - num;
		if (num3 == 1)
		{
			int index = value[num] - 48;
			return _hangulDigits[index];
		}
		int num4 = -1;
		foreach (int key in _hangul10Digits.Keys)
		{
			if (key == num3 - 1)
			{
				return _hangul10Digits[key];
			}
			if (key > num3 - 1)
			{
				break;
			}
			num4 = key;
		}
		if (num4 == -1)
		{
			return _hangul10Digits.Last().Value;
		}
		return _hangul10Digits[num4];
	}
}

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NCalc;

public static class ExpressionParser
{
	private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
	{
		{ "math.log", "Log10" },
		{ "math.pow", "Pow" }
	};

	private static Regex _regex;

	private static Regex Regex
	{
		get
		{
			if (_regex == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				int num = 0;
				foreach (KeyValuePair<string, string> item in Map)
				{
					if (num > 0)
					{
						stringBuilder.Append("|");
					}
					stringBuilder.Append(item.Key);
					num++;
				}
				_regex = new Regex(stringBuilder.ToString());
			}
			return _regex;
		}
	}

	private static string ReplaceFunction(string str)
	{
		str = Regex.Replace(str, Evaluator);
		return str;
	}

	private static string Evaluator(Match match)
	{
		return Map.Get(match.Value, match.Value);
	}

	public static Expression Parse(string str)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		try
		{
			if (!string.IsNullOrEmpty(str))
			{
				return new Expression(ReplaceFunction(str));
			}
		}
		catch
		{
		}
		return null;
	}
}

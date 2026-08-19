using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using NCalc;

namespace Durango.Utils;

public static class ExpressionParser
{
	private static readonly Dictionary<string, string> Map = new Dictionary<string, string>
	{
		{ "log(", "Log(" },
		{ "log10(", "Log10(" },
		{ "pow(", "Pow(" },
		{ "sqrt(", "Sqrt(" },
		{ "exp(", "Exp(" },
		{ "int(", "Floor(" },
		{ "max(", "Max(" },
		{ "min(", "Min(" },
		{ "round(", "Round(" },
		{ "ceil(", "Ceiling(" }
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
					string value = item.Key.Replace("(", "\\(");
					stringBuilder.Append(value);
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
		try
		{
			if (!string.IsNullOrEmpty(str))
			{
				return new Expression(ReplaceFunction(str));
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public static double ToDouble(this Expression expression, double defaultValue = 0.0)
	{
		try
		{
			return Convert.ToDouble(expression.Evaluate());
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			return defaultValue;
		}
	}

	public static float ToSingle(this Expression expression, float defaultValue = 0f)
	{
		return (float)expression.ToDouble(defaultValue);
	}

	public static long ToInt64(this Expression expression, long defaultValue = 0L)
	{
		return (long)expression.ToDouble(defaultValue);
	}

	public static int ToInt32(this Expression expression, int defaultValue = 0)
	{
		return (int)expression.ToDouble(defaultValue);
	}
}

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions;

public class ConditionalFormatter : IFormatter
{
	private string[] names = new string[3]
	{
		"conditional",
		"cond",
		string.Empty
	};

	private static readonly Regex complexConditionPattern = new Regex("^  (?:   ([&/]?)   ([<>=!]=?)   ([0-9.-]+)   )+   \\?", RegexOptions.IgnorePatternWhitespace);

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

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		Format format = formattingInfo.Format;
		object currentValue = formattingInfo.CurrentValue;
		if (format == null)
		{
			return false;
		}
		if (format.baseString[format.startIndex] == ':')
		{
			format = format.Substring(1);
		}
		IList<Format> list = format.Split('|');
		if (list.Count == 1)
		{
			return false;
		}
		bool flag = currentValue is byte || currentValue is short || currentValue is int || currentValue is long || currentValue is float || currentValue is double || currentValue is decimal;
		if (!flag && currentValue != null && currentValue.GetType().IsEnum)
		{
			flag = true;
		}
		decimal num = ((!flag) ? 0m : Convert.ToDecimal(currentValue));
		int num2;
		if (flag)
		{
			num2 = -1;
			while (true)
			{
				num2++;
				if (num2 == list.Count)
				{
					return true;
				}
				if (!TryEvaluateCondition(list[num2], num, out var conditionResult, out var outputItem))
				{
					if (num2 == 0)
					{
						break;
					}
					conditionResult = true;
				}
				if (conditionResult)
				{
					formattingInfo.Write(outputItem, currentValue);
					return true;
				}
			}
		}
		int count = list.Count;
		if (flag)
		{
			num2 = ((!(num < 0m)) ? Math.Min((int)Math.Floor(num), count - 1) : (count - 1));
		}
		else if (currentValue is bool)
		{
			num2 = ((!(bool)currentValue) ? 1 : 0);
		}
		else if (currentValue is DateTime dateTime)
		{
			num2 = ((count == 3 && dateTime.Date == DateTime.Today) ? 1 : ((!(dateTime <= DateTime.Now)) ? (count - 1) : 0));
		}
		else if (currentValue is TimeSpan timeSpan)
		{
			num2 = ((count == 3 && timeSpan == TimeSpan.Zero) ? 1 : ((timeSpan.CompareTo(TimeSpan.Zero) > 0) ? (count - 1) : 0));
		}
		else if (currentValue is string)
		{
			string value = (string)currentValue;
			num2 = (string.IsNullOrEmpty(value) ? 1 : 0);
		}
		else
		{
			object obj = currentValue;
			num2 = ((obj == null) ? 1 : 0);
		}
		Format format2 = list[num2];
		formattingInfo.Write(format2, currentValue);
		return true;
	}

	private static bool TryEvaluateCondition(Format parameter, decimal value, out bool conditionResult, out Format outputItem)
	{
		conditionResult = false;
		string input = parameter.baseString.Substring(parameter.startIndex, parameter.endIndex - parameter.startIndex);
		Match match = complexConditionPattern.Match(input);
		if (!match.Success)
		{
			outputItem = parameter;
			return false;
		}
		CaptureCollection captures = match.Groups[1].Captures;
		CaptureCollection captures2 = match.Groups[2].Captures;
		CaptureCollection captures3 = match.Groups[3].Captures;
		for (int i = 0; i < captures.Count; i++)
		{
			decimal num = decimal.Parse(captures3[i].Value);
			bool flag = false;
			switch (captures2[i].Value)
			{
			case ">":
				flag = value > num;
				break;
			case "<":
				flag = value < num;
				break;
			case "=":
			case "==":
				flag = value == num;
				break;
			case "<=":
				flag = value <= num;
				break;
			case ">=":
				flag = value >= num;
				break;
			case "!":
			case "!=":
				flag = value != num;
				break;
			}
			if (i == 0)
			{
				conditionResult = flag;
			}
			else if (captures[i].Value == "/")
			{
				conditionResult |= flag;
			}
			else
			{
				conditionResult &= flag;
			}
		}
		int startIndex = parameter.startIndex + match.Index + match.Length - parameter.startIndex;
		outputItem = parameter.Substring(startIndex);
		return true;
	}
}

using System;

namespace Durango.Prologue;

public static class ConditionalText
{
	public class ConditionalExpr
	{
		public string Condition;

		public string ResultIfTrue;

		public string ResultIfFalse;
	}

	private static ConditionalExpr ExtractConditionalExpress(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		int num = str.IndexOf("?");
		if (num > 0)
		{
			string condition = str.Substring(0, num).Trim();
			string text = str.Substring(num + 1);
			int num2 = text.IndexOf(":");
			string resultIfTrue = text.Substring(0, num2).Trim();
			string resultIfFalse = text.Substring(num2 + 1).Trim();
			return new ConditionalExpr
			{
				Condition = condition,
				ResultIfTrue = resultIfTrue,
				ResultIfFalse = resultIfFalse
			};
		}
		return null;
	}

	public static string Format(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return string.Empty;
		}
		if (str[0] == '#')
		{
			str = LocalizeSystem.Get(str);
		}
		int num = str.IndexOf('{');
		int num2 = str.IndexOf('}');
		if (num >= 0 && num2 >= 0 && num2 - num >= 1)
		{
			string text = str.Substring(0, num);
			string text2 = str.Substring(num + 1, num2 - num - 1);
			ConditionalExpr conditionalExpr = ExtractConditionalExpress(text2);
			if (conditionalExpr != null)
			{
				if (conditionalExpr.Condition.ToLower() == "male")
				{
					text += ((!PlayerBehavior.LocalPlayer.IsMale) ? conditionalExpr.ResultIfFalse : conditionalExpr.ResultIfTrue);
				}
			}
			else
			{
				switch (text2.ToLower())
				{
				case "year":
					text += DateTime.Now.ToString("yyyy");
					break;
				case "month":
					text += DateTime.Now.ToString("MM");
					break;
				case "day":
					text += DateTime.Now.ToString("dd");
					break;
				}
			}
			return text + str.Substring(num2 + 1);
		}
		return str;
	}
}

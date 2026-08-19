using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions;

public class ChooseFormatter : IFormatter
{
	private string[] names = new string[2] { "choose", "c" };

	private char splitChar = '|';

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

	public char SplitChar
	{
		get
		{
			return splitChar;
		}
		set
		{
			splitChar = value;
		}
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		if (formattingInfo.FormatterOptions == string.Empty)
		{
			return false;
		}
		string[] chooseOptions = formattingInfo.FormatterOptions.Split(splitChar);
		IList<Format> list = formattingInfo.Format.Split(splitChar);
		if (list.Count < 2)
		{
			return false;
		}
		Format format = DetermineChosenFormat(formattingInfo, list, chooseOptions);
		formattingInfo.Write(format, formattingInfo.CurrentValue);
		return true;
	}

	private static Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> choiceFormats, string[] chooseOptions)
	{
		object currentValue = formattingInfo.CurrentValue;
		string text = ((currentValue != null) ? currentValue.ToString() : "null");
		int num = Array.IndexOf(chooseOptions, text);
		if (choiceFormats.Count < chooseOptions.Length)
		{
			throw formattingInfo.FormattingException("You must specify at least " + chooseOptions.Length + " choices");
		}
		if (choiceFormats.Count > chooseOptions.Length + 1)
		{
			throw formattingInfo.FormattingException("You cannot specify more than " + (chooseOptions.Length + 1) + " choices");
		}
		if (num == -1 && choiceFormats.Count == chooseOptions.Length)
		{
			throw formattingInfo.FormattingException("\"" + text + "\" is not a valid choice, and a \"default\" choice was not supplied");
		}
		if (num == -1)
		{
			num = choiceFormats.Count - 1;
		}
		return choiceFormats[num];
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions;

public class ListFormatter : IFormatter, ISource
{
	private string[] names = new string[3]
	{
		"list",
		"l",
		string.Empty
	};

	private static int CollectionIndex = -1;

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

	public ListFormatter(SmartFormatter formatter)
	{
		formatter.Parser.AddOperators("[]()");
	}

	public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
	{
		object currentValue = selectorInfo.CurrentValue;
		string selectorText = selectorInfo.SelectorText;
		IList list = currentValue as IList;
		if ((selectorInfo.SelectorIndex != 0 || selectorInfo.SelectorOperator.Length != 0) && list != null && int.TryParse(selectorText, out var result) && result < list.Count)
		{
			selectorInfo.Result = list[result];
			return true;
		}
		if (selectorText.Equals("index", StringComparison.OrdinalIgnoreCase))
		{
			if (selectorInfo.SelectorIndex == 0)
			{
				selectorInfo.Result = CollectionIndex;
				return true;
			}
			if (list != null && 0 <= CollectionIndex && CollectionIndex < list.Count)
			{
				selectorInfo.Result = list[CollectionIndex];
				return true;
			}
		}
		return false;
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		Format format = formattingInfo.Format;
		object currentValue = formattingInfo.CurrentValue;
		if (!(currentValue is IEnumerable enumerable))
		{
			return false;
		}
		if (currentValue is string)
		{
			return false;
		}
		if (currentValue is IFormattable)
		{
			return false;
		}
		if (format == null)
		{
			return false;
		}
		IList<Format> list = format.Split('|', 4);
		if (list.Count < 2)
		{
			return false;
		}
		Format format2 = list[0];
		string text = ((list.Count < 2) ? string.Empty : list[1].GetLiteralText());
		string text2 = ((list.Count < 3) ? text : list[2].GetLiteralText());
		string text3 = ((list.Count < 4) ? text2 : list[3].GetLiteralText());
		if (!format2.HasNested)
		{
			Format format3 = new Format(format2.baseString);
			format3.startIndex = format2.startIndex;
			format3.endIndex = format2.endIndex;
			format3.HasNested = true;
			Placeholder placeholder = new Placeholder(format3, format2.startIndex, 0);
			placeholder.Format = format2;
			placeholder.endIndex = format2.endIndex;
			format3.Items.Add(placeholder);
			format2 = format3;
		}
		ICollection collection = currentValue as ICollection;
		if (collection == null)
		{
			List<object> list2 = new List<object>();
			foreach (object item in enumerable)
			{
				list2.Add(item);
			}
			collection = list2;
		}
		int collectionIndex = CollectionIndex;
		CollectionIndex = -1;
		foreach (object item2 in collection)
		{
			CollectionIndex++;
			if (text != null && CollectionIndex != 0)
			{
				if (CollectionIndex < collection.Count - 1)
				{
					formattingInfo.Write(text);
				}
				else if (CollectionIndex == 1)
				{
					formattingInfo.Write(text3);
				}
				else
				{
					formattingInfo.Write(text2);
				}
			}
			formattingInfo.Write(format2, item2);
		}
		CollectionIndex = collectionIndex;
		return true;
	}
}

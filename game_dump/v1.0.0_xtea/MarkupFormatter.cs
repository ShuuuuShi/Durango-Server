using System.Collections.Generic;
using System.Linq;
using SmartFormat;
using SmartFormat.Core.Extensions;

public class MarkupFormatter : IFormatter
{
	private static readonly Dictionary<string, string> Markups = new Dictionary<string, string> { { "lv", "Lv.\u00a0{0}" } };

	private string[] _names;

	private readonly SmartFormatter _formatter;

	public string[] Names
	{
		get
		{
			if (_names == null)
			{
				_names = Markups.Keys.ToArray();
			}
			return _names;
		}
		set
		{
			_names = value;
		}
	}

	public MarkupFormatter(SmartFormatter formatter)
	{
		_formatter = formatter;
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		string formatterName = formattingInfo.Placeholder.FormatterName;
		if (Markups.TryGetValue(formatterName, out var value))
		{
			string text = _formatter.Format(value, formattingInfo.CurrentValue);
			formattingInfo.Write(text);
			return true;
		}
		return false;
	}
}

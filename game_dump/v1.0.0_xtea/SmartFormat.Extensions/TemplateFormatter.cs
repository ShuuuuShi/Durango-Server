using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions;

public class TemplateFormatter : IFormatter
{
	private readonly SmartFormatter formatter;

	private readonly IDictionary<string, Format> templates;

	private string[] names = new string[2] { "template", "t" };

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

	public TemplateFormatter(SmartFormatter formatter)
	{
		this.formatter = formatter;
		templates = new Dictionary<string, Format>((formatter.Settings.CaseSensitivity != 0) ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture);
	}

	public void Register(string templateName, string template)
	{
		Format value = formatter.Parser.ParseFormat(template);
		templates.Add(templateName, value);
	}

	public bool Remove(string templateName)
	{
		return templates.Remove(templateName);
	}

	public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		string text = formattingInfo.FormatterOptions;
		if (text == string.Empty)
		{
			if (formattingInfo.Format.HasNested)
			{
				return false;
			}
			text = formattingInfo.Format.RawText;
		}
		if (!templates.TryGetValue(text, out var value))
		{
			return false;
		}
		formattingInfo.Write(value, formattingInfo.CurrentValue);
		return true;
	}
}

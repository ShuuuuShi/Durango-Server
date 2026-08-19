using System.Collections.Generic;
using System.Xml.Linq;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Extensions;

public class XElementFormatter : IFormatter
{
	private string[] names = new string[4]
	{
		"xelement",
		"xml",
		"x",
		string.Empty
	};

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
		XElement xElement = null;
		if (format != null && format.HasNested)
		{
			return false;
		}
		if (currentValue is IList<XElement> { Count: >0 } list)
		{
			xElement = list[0];
		}
		XElement xElement2 = xElement ?? (currentValue as XElement);
		if (xElement2 != null)
		{
			formattingInfo.Write(xElement2.Value);
			return true;
		}
		return false;
	}
}

using System.Collections.Generic;
using System.Text;

namespace SmartFormat.Core.Parsing;

public class Placeholder : FormatItem
{
	public readonly Format parent;

	public int NestedDepth { get; set; }

	public List<Selector> Selectors { get; private set; }

	public int Alignment { get; set; }

	public string FormatterName { get; set; }

	public string FormatterOptions { get; set; }

	public Format Format { get; set; }

	public Placeholder(Format parent, int startIndex, int nestedDepth)
		: base(parent, startIndex)
	{
		this.parent = parent;
		Selectors = new List<Selector>();
		NestedDepth = nestedDepth;
		FormatterName = string.Empty;
		FormatterOptions = string.Empty;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(endIndex - startIndex);
		stringBuilder.Append('{');
		foreach (Selector selector in Selectors)
		{
			stringBuilder.Append(selector.baseString, selector.operatorStart, selector.endIndex - selector.operatorStart);
		}
		if (Alignment != 0)
		{
			stringBuilder.Append(',');
			stringBuilder.Append(Alignment);
		}
		if (FormatterName != string.Empty)
		{
			stringBuilder.Append(':');
			stringBuilder.Append(FormatterName);
			if (FormatterOptions != string.Empty)
			{
				stringBuilder.Append('(');
				stringBuilder.Append(FormatterOptions);
				stringBuilder.Append(')');
			}
		}
		if (Format != null)
		{
			stringBuilder.Append(':');
			stringBuilder.Append(Format.ToString());
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}
}

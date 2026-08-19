using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Formatting;

public class FormattingInfo : IFormattingInfo, ISelectorInfo
{
	public FormattingInfo Parent { get; private set; }

	public FormatDetails FormatDetails { get; private set; }

	public Selector Selector { get; set; }

	public string SelectorText => Selector.RawText;

	public int SelectorIndex => Selector.SelectorIndex;

	public string SelectorOperator => Selector.Operator;

	public object Result { get; set; }

	public object CurrentValue { get; set; }

	public Placeholder Placeholder { get; private set; }

	public int Alignment => Placeholder.Alignment;

	public string FormatterOptions => Placeholder.FormatterOptions;

	public Format Format { get; private set; }

	public FormattingInfo(FormatDetails formatDetails, Format format, object currentValue)
		: this(null, formatDetails, format, currentValue)
	{
	}

	public FormattingInfo(FormattingInfo parent, FormatDetails formatDetails, Format format, object currentValue)
	{
		Parent = parent;
		CurrentValue = currentValue;
		Format = format;
		FormatDetails = formatDetails;
	}

	public FormattingInfo(FormattingInfo parent, FormatDetails formatDetails, Placeholder placeholder, object currentValue)
	{
		Parent = parent;
		FormatDetails = formatDetails;
		Placeholder = placeholder;
		Format = placeholder.Format;
		CurrentValue = currentValue;
	}

	private FormattingInfo CreateChild(Format format, object currentValue)
	{
		return new FormattingInfo(this, FormatDetails, format, currentValue);
	}

	public FormattingInfo CreateChild(Placeholder placeholder)
	{
		return new FormattingInfo(this, FormatDetails, placeholder, CurrentValue);
	}

	public void Write(string text)
	{
		FormatDetails.Output.Write(text, this);
	}

	public void Write(string text, int startIndex, int length)
	{
		FormatDetails.Output.Write(text, startIndex, length, this);
	}

	public void Write(Format format, object value)
	{
		FormattingInfo formattingInfo = CreateChild(format, value);
		FormatDetails.Formatter.Format(formattingInfo);
	}

	public FormattingException FormattingException(string issue, FormatItem problemItem = null, int startIndex = -1)
	{
		if (problemItem == null)
		{
			problemItem = Format;
		}
		if (startIndex == -1)
		{
			startIndex = problemItem.startIndex;
		}
		return new FormattingException(problemItem, issue, startIndex);
	}
}

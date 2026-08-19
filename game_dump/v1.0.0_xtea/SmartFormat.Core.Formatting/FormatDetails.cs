using System;
using SmartFormat.Core.Output;
using SmartFormat.Core.Parsing;
using SmartFormat.Core.Settings;

namespace SmartFormat.Core.Formatting;

public class FormatDetails
{
	public SmartFormatter Formatter { get; private set; }

	public Format OriginalFormat { get; private set; }

	public object[] OriginalArgs { get; private set; }

	public FormatCache FormatCache { get; private set; }

	public IFormatProvider Provider { get; private set; }

	public IOutput Output { get; private set; }

	public FormattingException FormattingException { get; set; }

	public SmartSettings Settings => Formatter.Settings;

	public FormatDetails(SmartFormatter formatter, Format originalFormat, object[] originalArgs, FormatCache formatCache, IFormatProvider provider, IOutput output)
	{
		Formatter = formatter;
		OriginalFormat = originalFormat;
		OriginalArgs = originalArgs;
		FormatCache = formatCache;
		Provider = provider;
		Output = output;
	}
}

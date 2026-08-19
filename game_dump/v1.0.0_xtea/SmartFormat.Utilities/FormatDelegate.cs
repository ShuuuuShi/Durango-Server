using System;

namespace SmartFormat.Utilities;

public class FormatDelegate : IFormattable
{
	private readonly Func<string, string> getFormat1;

	private readonly Func<string, IFormatProvider, string> getFormat2;

	public FormatDelegate(Func<string, string> getFormat)
	{
		getFormat1 = getFormat;
	}

	public FormatDelegate(Func<string, IFormatProvider, string> getFormat)
	{
		getFormat2 = getFormat;
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return (getFormat1 == null) ? getFormat2(format, formatProvider) : getFormat1(format);
	}
}

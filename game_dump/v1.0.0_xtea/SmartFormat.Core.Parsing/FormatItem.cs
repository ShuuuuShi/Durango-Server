using System;

namespace SmartFormat.Core.Parsing;

public abstract class FormatItem
{
	public readonly string baseString;

	public int startIndex;

	public int endIndex;

	[Obsolete("Please use RawText instead")]
	public string Text => RawText;

	public string RawText => baseString.Substring(startIndex, endIndex - startIndex);

	protected FormatItem(FormatItem parent, int startIndex)
		: this(parent.baseString, startIndex, parent.baseString.Length)
	{
	}

	protected FormatItem(string baseString, int startIndex, int endIndex)
	{
		this.baseString = baseString;
		this.startIndex = startIndex;
		this.endIndex = endIndex;
	}

	public override string ToString()
	{
		if (endIndex <= startIndex)
		{
			return $"Empty ({baseString.Substring(startIndex)})";
		}
		return $"{baseString.Substring(startIndex, endIndex - startIndex)}";
	}
}

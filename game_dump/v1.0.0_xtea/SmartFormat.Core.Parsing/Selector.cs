namespace SmartFormat.Core.Parsing;

public class Selector : FormatItem
{
	internal readonly int operatorStart;

	public int SelectorIndex { get; private set; }

	public string Operator => baseString.Substring(operatorStart, startIndex - operatorStart);

	public Selector(string baseString, int startIndex, int endIndex, int operatorStart, int selectorIndex)
		: base(baseString, startIndex, endIndex)
	{
		SelectorIndex = selectorIndex;
		this.operatorStart = operatorStart;
	}
}

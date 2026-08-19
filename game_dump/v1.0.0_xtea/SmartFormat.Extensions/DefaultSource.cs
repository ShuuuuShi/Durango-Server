using SmartFormat.Core.Extensions;
using SmartFormat.Core.Formatting;

namespace SmartFormat.Extensions;

public class DefaultSource : ISource
{
	public DefaultSource(SmartFormatter formatter)
	{
		formatter.Parser.AddOperators(",");
		formatter.Parser.AddAdditionalSelectorChars("-");
	}

	public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
	{
		object currentValue = selectorInfo.CurrentValue;
		string selectorText = selectorInfo.SelectorText;
		FormatDetails formatDetails = selectorInfo.FormatDetails;
		if (int.TryParse(selectorText, out var result))
		{
			if (selectorInfo.SelectorIndex == 0 && result < formatDetails.OriginalArgs.Length && selectorInfo.SelectorOperator == string.Empty)
			{
				selectorInfo.Result = formatDetails.OriginalArgs[result];
				return true;
			}
			if (selectorInfo.SelectorOperator == ",")
			{
				selectorInfo.Placeholder.Alignment = result;
				selectorInfo.Result = currentValue;
				return true;
			}
		}
		return false;
	}
}

using System.ComponentModel;
using SmartFormat.Core.Formatting;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Extensions;

public interface ISelectorInfo
{
	object CurrentValue { get; }

	string SelectorText { get; }

	int SelectorIndex { get; }

	string SelectorOperator { get; }

	object Result { set; }

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	Placeholder Placeholder { get; }

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	FormatDetails FormatDetails { get; }
}

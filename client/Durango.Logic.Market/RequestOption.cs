using Messages;

namespace Durango.Logic.Market;

public class RequestOption
{
	public ProductType Type;

	public SortCondition Condition;

	public int Index;

	public bool NoMore;

	public bool IsLoading;
}

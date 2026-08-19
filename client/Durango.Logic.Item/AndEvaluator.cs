namespace Durango.Logic.Item;

public class AndEvaluator : IItemEvaluator
{
	private IItemEvaluator _left;

	private IItemEvaluator _right;

	public AndEvaluator(IItemEvaluator left, IItemEvaluator right)
	{
		_left = left;
		_right = right;
	}

	public bool Evaluate(ItemData data)
	{
		return _left.Evaluate(data) && _right.Evaluate(data);
	}
}

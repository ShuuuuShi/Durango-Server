namespace Durango.Logic.Item;

public class OrEvaluator : IItemEvaluator
{
	private IItemEvaluator _left;

	private IItemEvaluator _right;

	public OrEvaluator(IItemEvaluator left, IItemEvaluator right)
	{
		_left = left;
		_right = right;
	}

	public bool Evaluate(ItemData data)
	{
		if (!_left.Evaluate(data))
		{
			return _right.Evaluate(data);
		}
		return true;
	}
}

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
		if (_left.Evaluate(data))
		{
			return _right.Evaluate(data);
		}
		return false;
	}
}

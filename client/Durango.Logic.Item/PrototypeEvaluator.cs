namespace Durango.Logic.Item;

public class PrototypeEvaluator : ItemEvaluator
{
	public PrototypeEvaluator(string text)
		: base(text, (ItemData data, string prototype) => data.PrototypeName == prototype)
	{
	}
}

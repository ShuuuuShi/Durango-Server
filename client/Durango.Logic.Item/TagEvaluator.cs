namespace Durango.Logic.Item;

public class TagEvaluator : ItemEvaluator
{
	public TagEvaluator(string text)
		: base(text, (ItemData data, string tag) => string.IsNullOrEmpty(tag) || data.HasTag(tag))
	{
	}
}

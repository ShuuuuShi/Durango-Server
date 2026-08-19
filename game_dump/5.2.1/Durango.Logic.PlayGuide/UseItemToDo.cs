using Durango.Logic.Item;

namespace Durango.Logic.PlayGuide;

public class UseItemToDo : ToDoBase
{
	private readonly TagEvaluator _tag;

	private int _useCount;

	public UseItemToDo(string tag)
	{
		_tag = new TagEvaluator(tag);
		base.LocalText = TagData.GetTagName(tag);
	}

	protected void UseItemSucceed(ItemData item)
	{
		if (_tag.Evaluate(item))
		{
			_useCount++;
			CallProgressChange(_useCount);
		}
	}

	public override void OnAddItem()
	{
		GameSystem<InventorySystem>.Instance().UseItemSucceed += UseItemSucceed;
	}

	public override void OnRemoveItem()
	{
		GameSystem<InventorySystem>.Instance().UseItemSucceed -= UseItemSucceed;
	}
}

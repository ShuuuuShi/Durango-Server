using ItemSystem;

namespace PlayGuide;

public class UseItemToDo : ToDoBase
{
	private readonly TagEvaluator _tag;

	private int _useCount;

	public UseItemToDo(string tag)
	{
		_tag = new TagEvaluator(tag);
		base.LocalText = TagData.GetTagName(tag);
	}

	protected void OnUseItemSucceed(ItemData item)
	{
		if (_tag.Evaluate(item))
		{
			_useCount++;
			CallProgressChange(_useCount);
		}
	}

	public override void OnAddItem()
	{
		GameSystem<InventorySystem>.Instance().OnUseItemSucceed += OnUseItemSucceed;
	}

	public override void OnRemoveItem()
	{
		GameSystem<InventorySystem>.Instance().OnUseItemSucceed -= OnUseItemSucceed;
	}
}

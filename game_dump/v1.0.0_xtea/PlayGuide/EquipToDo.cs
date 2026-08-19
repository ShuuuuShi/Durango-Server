using ItemSystem;

namespace PlayGuide;

public class EquipToDo : ToDoBase
{
	private readonly string[] _slots;

	private readonly TagEvaluator _tag;

	public EquipToDo(string slots, string tag)
	{
		_slots = Split(slots);
		_tag = ((!string.IsNullOrEmpty(tag)) ? new TagEvaluator(tag) : null);
	}

	private void OnUpdateEquipments()
	{
		ItemData itemData = GameSystem<EquipSystem>.Instance().FindEquipItem(_slots);
		if (itemData != null && (_tag == null || _tag.Evaluate(itemData)))
		{
			CallComplete();
		}
	}

	public override void OnAddItem()
	{
		GameSystem<EquipSystem>.Instance().OnUpdateEquipments += OnUpdateEquipments;
		OnUpdateEquipments();
	}

	public override void OnRemoveItem()
	{
		GameSystem<EquipSystem>.Instance().OnUpdateEquipments -= OnUpdateEquipments;
	}
}

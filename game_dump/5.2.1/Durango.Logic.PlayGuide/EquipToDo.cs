using Durango.Logic.Item;
using Durango.Utils.Extensions;

namespace Durango.Logic.PlayGuide;

public class EquipToDo : ToDoBase
{
	private readonly string[] _slots;

	private readonly TagEvaluator _tag;

	public EquipToDo(string slots, string tag)
	{
		_slots = ((!string.IsNullOrEmpty(slots)) ? slots.SplitAndTrim('|') : null);
		_tag = ((!string.IsNullOrEmpty(tag)) ? new TagEvaluator(tag) : null);
	}

	private void EquipmentsUpdated()
	{
		ItemData itemData = GameSystem<EquipSystem>.Instance().FindEquippedItem(GameSystem<EquipSystem>.Instance().CurrentEquipPreset, _slots);
		if (itemData != null && (_tag == null || _tag.Evaluate(itemData)))
		{
			CallComplete();
		}
	}

	public override void OnAddItem()
	{
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated += EquipmentsUpdated;
		EquipmentsUpdated();
	}

	public override void OnRemoveItem()
	{
		GameSystem<EquipSystem>.Instance().EquipmentsUpdated -= EquipmentsUpdated;
	}
}

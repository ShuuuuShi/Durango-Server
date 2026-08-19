using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.PlayGuide;
using Durango.Utils.Extensions;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorEquip : LocatorMenu
{
	private CharacterInfoGroup _characterGroup;

	private EquipSystem.Slot _slot;

	private TagEvaluator _tag;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_characterGroup = UIManager.FindScript<CharacterInfoGroup>();
		SetMenuType(MenuType.Character);
		Parameter parameter = Parameters.Get("select_slot");
		if (parameter != null)
		{
			_slot = parameter.id.ToEnum(EquipSystem.Slot.Precious);
		}
		Parameter parameter2 = Parameters.Get("select_item");
		if (parameter2 != null && !string.IsNullOrEmpty(parameter2.param))
		{
			_tag = new TagEvaluator(parameter2.param);
		}
	}

	protected override string SelectPhase()
	{
		if (_characterGroup != null && _characterGroup.IsOpened)
		{
			if (_characterGroup.Equip.SelectedSlot == _slot)
			{
				ItemData lastSelected = _characterGroup.Equip.LastSelected;
				if (lastSelected != null && (_tag == null || _tag.Evaluate(lastSelected)) && !lastSelected.IsEquipments)
				{
					return "equip_button";
				}
				return "select_item";
			}
			return "select_slot";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "select_slot":
			base.TargetTransform = _characterGroup.Equip.GetSlotTransform(_slot);
			break;
		case "select_item":
			base.TargetTransform = _characterGroup.Equip.GetItemTransform(_tag);
			break;
		case "equip_button":
			base.TargetTransform = _characterGroup.Equip.GetEquipButtonTransform();
			base.CurrentParameter.rotate = 90f;
			break;
		default:
			base.UpdateTargetTransform();
			break;
		}
	}
}

using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EquipSlotBase : SelectableWidget
{
	[SerializeField]
	private ItemIconTex _iconSprite;

	[SerializeField]
	private ItemGradeViewer _gradeViewer;

	[SerializeField]
	private ItemModifiedCountViewer _modifiedCountViewer;

	[SerializeField]
	private UISprite _bgIconSprite;

	[SerializeField]
	private UISprite _iconDurability;

	public EquipSystem.Slot Slot { get; private set; }

	public ItemData Item { get; private set; }

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
		SetDurabilityState(DurabilityState.Good);
	}

	public void Set(EquipSystem.Slot slot)
	{
		Slot = slot;
		_bgIconSprite.spriteName = IconMap.Get($"{slot}_bg".ToLower());
	}

	public void SetItem(ItemData item)
	{
		Item = item;
		if (item == null)
		{
			_iconSprite.gameObject.SetActive(value: false);
			_gradeViewer.gameObject.SetActive(value: false);
			_modifiedCountViewer.gameObject.SetActive(value: false);
		}
		else
		{
			_iconSprite.SetIcon(item);
			_iconSprite.gameObject.SetActive(value: true);
			_gradeViewer.Set(item);
			_gradeViewer.gameObject.SetActive(value: true);
			_modifiedCountViewer.Set(item.ModifiedCount);
			_modifiedCountViewer.gameObject.SetActive(value: true);
		}
		RefreshDurabilityInfo();
	}

	private void RefreshDurabilityInfo()
	{
		if (Item != null && Item.GetDurabilityState(out var state, out var _))
		{
			SetDurabilityState(state);
		}
		else
		{
			SetDurabilityState(DurabilityState.Good);
		}
	}

	private void SetDurabilityState(DurabilityState state)
	{
		_iconDurability.gameObject.SetActive(state != DurabilityState.Good);
		_iconDurability.color = ((state != DurabilityState.Destroyed) ? ItemIconWidget.DurabilityWarningColor : ItemIconWidget.DurabilityDestroyedColor);
	}
}

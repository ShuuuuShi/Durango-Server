using ItemSystem;
using UnityEngine;

public class EquipSlot : Selectable
{
	[SerializeField]
	private ItemIconTex _iconSprite;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _bgIconSprite;

	[SerializeField]
	private Color _equipBgColor;

	[SerializeField]
	private UIWidget _selector;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public EquipSystem.Slot Slot { get; private set; }

	public ItemData Item { get; private set; }

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
			((Component)_iconSprite).gameObject.SetActive(false);
			return;
		}
		_iconSprite.SetIcon(item);
		((Component)_iconSprite).gameObject.SetActive(true);
	}

	protected override void OnInit()
	{
	}

	protected override void Refresh(bool select)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_background.color = ((Item != null) ? _equipBgColor : Color.clear);
		Widget.alpha = ((!base.Disable) ? 1f : 0.5f);
		((Component)_selector).gameObject.SetActive(base.Select);
	}

	private void OnPress(bool press)
	{
		Refresh(press || base.Select);
	}
}

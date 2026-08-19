using System;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class InteractionBottomSlotWidget : UIWidget
{
	[SerializeField]
	private ItemIconTex _itemTexture;

	[SerializeField]
	private GameObject _emptyIcon;

	private Item? _item;

	public event Action<Item?> Clicked;

	public void SetItem(Item? item)
	{
		_item = item;
		Refresh();
	}

	private void Refresh()
	{
		Item? item = _item;
		bool flag = !item.HasValue;
		_itemTexture.gameObject.SetActive(!flag);
		_emptyIcon.gameObject.SetActive(flag);
		Item? item2 = _item;
		if (item2.HasValue)
		{
			ItemData icon = new ItemData(_item.Value);
			_itemTexture.SetIcon(icon);
		}
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked(_item);
		}
	}
}

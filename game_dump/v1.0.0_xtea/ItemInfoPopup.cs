using ItemSystem;
using UnityEngine;

public class ItemInfoPopup : TooltipBase
{
	[SerializeField]
	private ItemInfoContainer _itemInfo;

	private ItemData _item;

	protected override void OnAwake()
	{
	}

	public void Set(ItemData item)
	{
		_item = item;
	}

	protected override void FillData()
	{
		_itemInfo.Show(_item);
	}

	protected override void UpdateLayout()
	{
	}
}

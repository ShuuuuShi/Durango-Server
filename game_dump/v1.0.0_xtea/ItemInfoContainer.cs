using ItemSystem;
using UnityEngine;

public class ItemInfoContainer : MonoBehaviour
{
	[SerializeField]
	private ItemInfoWidget _itemInfoBase;

	[SerializeField]
	private GameObject _noSelectWidget;

	[SerializeField]
	private int _bottomMargin;

	[SerializeField]
	private bool _enableCraftLink;

	private ItemInfoWidget _infoWidget;

	private bool _isInit;

	public ItemInfoWidget InfoWidget => _infoWidget;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			if ((Object)(object)_infoWidget == (Object)null)
			{
				_infoWidget = ((Component)this).gameObject.AddChild(((Component)_itemInfoBase).gameObject).GetComponent<ItemInfoWidget>();
				_infoWidget.Init(_bottomMargin, _enableCraftLink);
				((Component)_infoWidget).gameObject.SetActive(false);
			}
		}
	}

	private void OnLayout(Point2 size)
	{
		if (!((Object)(object)_infoWidget == (Object)null))
		{
			_infoWidget.UpdateLayout((!UIManager.IsPortraitMode) ? _bottomMargin : 0);
		}
	}

	public void Show(ItemData item)
	{
		Init();
		_infoWidget.SetItemData(item);
		_infoWidget.Open();
		if ((Object)(object)_noSelectWidget != (Object)null)
		{
			_noSelectWidget.SetActive(false);
		}
	}

	public void Hide()
	{
		if ((Object)(object)_infoWidget != (Object)null)
		{
			_infoWidget.Close();
		}
		if ((Object)(object)_noSelectWidget != (Object)null)
		{
			_noSelectWidget.SetActive(true);
		}
	}
}

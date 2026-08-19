using UnityEngine;

public class VerticalInventory : MonoBehaviour
{
	[SerializeField]
	private UIWidget _inventoryContainer;

	[SerializeField]
	private UIWidget _tabConatiner;

	[SerializeField]
	private SimpleContainer _playerTab;

	private SimpleContainer _targetTab;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private GameObject _buttonContainer;

	[SerializeField]
	private SortOptionContainer _sortOption;

	[SerializeField]
	private GameObject _multipleSelectBtn;

	[SerializeField]
	private DefaultSelectableButton _removeBtn;

	[SerializeField]
	private DefaultSelectableButton _useBtn;

	[SerializeField]
	private DefaultSelectableButton _likeBtn;

	[SerializeField]
	private DefaultSelectableButton _cheatDuplicateBtn;

	[SerializeField]
	private UISprite _usableListPopupArrow;

	[SerializeField]
	private UIWidget _usableListPopup;

	[SerializeField]
	private SimpleContainer _usableActionBtnBase;

	[SerializeField]
	private ItemList _itemList;

	[SerializeField]
	private GameObject _sortOptionContainer;

	[SerializeField]
	private UISprite _inventorySizeIcon;

	[SerializeField]
	private UILabel _textInventorySize;

	private SimpleContainer PlayerTab => _playerTab;

	private SimpleContainer TargetTab
	{
		get
		{
			if ((Object)(object)_targetTab == (Object)null)
			{
				_targetTab = ((Component)((Component)_playerTab).transform.parent).gameObject.AddChild(((Component)_playerTab).gameObject).GetComponent<SimpleContainer>();
			}
			return _targetTab;
		}
	}
}

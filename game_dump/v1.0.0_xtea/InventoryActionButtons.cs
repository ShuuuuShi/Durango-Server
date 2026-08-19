using System;
using System.Collections.Generic;
using ItemSystem;
using L10N;
using UnityEngine;

public class InventoryActionButtons : MonoBehaviour
{
	[SerializeField]
	private DefaultSelectableButton _removeBtn;

	[SerializeField]
	private DefaultSelectableButton _useBtn;

	[SerializeField]
	private DefaultSelectableButton _lockBtn;

	[SerializeField]
	private UISprite _usableListPopupArrow;

	[SerializeField]
	private AnimationWidget _usableListPopup;

	[SerializeField]
	private ListObjectPool _usableActionButtons;

	private UIWidget _widget;

	private readonly List<UseType> _usableList = new List<UseType>();

	private Dictionary<UseType, int> _useTypeDict = new Dictionary<UseType, int>();

	private UseType _useType;

	private UIWidget[] _buttons;

	private Inventory _inventory;

	private Inventory.InventoryMode _inventoryMode;

	private List<ItemIcon2> _selectedItems;

	private bool _isShowActionPopup;

	public Func<UseType, string> UseTypeToString { get; set; }

	public UseType UseType
	{
		get
		{
			return _useType;
		}
		set
		{
			_useType = value;
		}
	}

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public event Action<UseType> OnSelectUseType;

	public event Action OnUse;

	public event Action OnRemove;

	public event Action OnLock;

	private void Start()
	{
		_usableListPopup.SetAlpha(0f, useTween: false);
		((Component)_usableListPopup).gameObject.SetActive(false);
		_removeBtn.Clicked = OnRemoveClick;
		_useBtn.Clicked = OnUseClick;
		_lockBtn.Clicked = OnLockClick;
		UIEventListener.Get(((Component)_usableListPopupArrow).gameObject).onClick = OnClickUsablePopupArrow;
		_usableActionButtons.Init(delegate(GameObject o)
		{
			UIEventListener.Get(o).onClick = OnClickUsableActionBtn;
		});
	}

	private void OnRemoveClick()
	{
		if (!Selectable.Current.Disable && this.OnRemove != null)
		{
			this.OnRemove();
		}
	}

	private void OnUseClick()
	{
		if (!Selectable.Current.Disable && this.OnUse != null)
		{
			this.OnUse();
		}
	}

	private void OnLockClick()
	{
		if (!Selectable.Current.Disable && this.OnLock != null)
		{
			this.OnLock();
		}
	}

	private void OnClickUsablePopupArrow(GameObject obj)
	{
		if (_usableList.Count > 1)
		{
			PopupUsableActionList();
		}
		else
		{
			OnUseClick();
		}
	}

	public Transform GetUseButton()
	{
		return ((Component)_useBtn).transform;
	}

	public void UpdateUseButtonAction(Inventory inventory, Inventory.InventoryMode mode, List<ItemIcon2> selectedItems)
	{
		_inventory = inventory;
		_inventoryMode = mode;
		_selectedItems = selectedItems;
		bool flag = false;
		_useTypeDict.Clear();
		for (int i = 0; i < selectedItems.Count; i++)
		{
			Inventory.GetUsableActions(inventory, selectedItems[i].Item, mode, ref _useTypeDict);
		}
		_usableList.Clear();
		foreach (KeyValuePair<UseType, int> item in _useTypeDict)
		{
			if (item.Value == selectedItems.Count)
			{
				_usableList.Add(item.Key);
			}
		}
		_usableList.Sort();
		if (_usableList.Count == 0)
		{
			_useType = UseType.None;
			flag = true;
		}
		else if (!_usableList.Contains(_useType))
		{
			_useType = _usableList[0];
		}
		if (_usableList.Count > 1)
		{
			((Component)_usableListPopupArrow).gameObject.SetActive(true);
			_useBtn.TextLabel.rightAnchor.absolute = -_usableListPopupArrow.width;
			_useBtn.TextLabel.ResetAndUpdateAnchors();
		}
		else
		{
			((Component)_usableListPopupArrow).gameObject.SetActive(false);
			_useBtn.TextLabel.rightAnchor.absolute = 0;
			_useBtn.TextLabel.ResetAndUpdateAnchors();
		}
		flag |= Inventory.CheckDisableUseType(selectedItems, _useType);
		_useBtn.Text = GetUseTypeName(_useType);
		_useBtn.Disable = flag;
		PopupUsableActionList(show: false);
	}

	private string GetUseTypeName(UseType type)
	{
		return (UseTypeToString != null) ? UseTypeToString(type) : type.GetName();
	}

	private void PopupUsableActionList()
	{
		PopupUsableActionList(!_isShowActionPopup);
	}

	private void PopupUsableActionList(bool show)
	{
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		_isShowActionPopup = show;
		if (show)
		{
			((Component)_usableListPopup).gameObject.SetActive(true);
			_usableListPopup.Alpha = 1f;
			int width = Widget.width;
			_usableListPopup.Widget.width = width;
			((Component)_usableListPopupArrow).transform.localEulerAngles = Vector3.forward * 0f;
			_usableActionButtons.Set(_usableList.Count);
			for (int i = 0; i < _usableActionButtons.Count; i++)
			{
				UIWidget component = _usableActionButtons[i].GetComponent<UIWidget>();
				component.width = width;
				UILabel component2 = ((Component)((Component)component).transform.FindChild("Text")).GetComponent<UILabel>();
				component2.text = GetUseTypeName(_usableList[i]);
			}
			float num = _usableActionButtons.Reposition(Vector3.up);
			_usableListPopup.Widget.height = (int)num;
			_usableListPopup.Widget.SetPosition(Widget.localCorners[1], 0f, 0f);
			UIUtility.UpdateAnchors(((Component)_usableListPopup).transform);
		}
		else
		{
			((Component)_usableListPopupArrow).transform.localEulerAngles = Vector3.forward * 180f;
			_usableListPopup.Alpha = 0f;
		}
	}

	private void OnClickUsableActionBtn(GameObject go)
	{
		int num = _usableActionButtons.IndexOf(go);
		if (num != -1)
		{
			_useType = _usableList[num];
			UpdateUseButtonAction(_inventory, _inventoryMode, _selectedItems);
			if (this.OnSelectUseType != null)
			{
				this.OnSelectUseType(_usableList[num]);
			}
		}
	}

	public void SetBottomButtonLayout(params float[] weight)
	{
		if (_buttons == null)
		{
			_buttons = new UIWidget[3] { _useBtn.Widget, _lockBtn.Widget, _removeBtn.Widget };
		}
		float num = 0f;
		int num2 = 0;
		int i = 0;
		for (int num3 = _buttons.Length; i < num3; i++)
		{
			bool flag = Math.Abs(weight[i]) > float.Epsilon;
			num += weight[i];
			((Component)_buttons[i]).gameObject.SetActive(flag);
			if (flag)
			{
				num2++;
			}
		}
		if (num2 == 0)
		{
			return;
		}
		UIWidget widget = Widget;
		float num4 = 0f;
		int j = 0;
		for (int num5 = _buttons.Length; j < num5; j++)
		{
			UIWidget uIWidget = _buttons[j];
			if (((Component)uIWidget).gameObject.activeSelf)
			{
				float num6 = weight[j] / num;
				uIWidget.SetAnchor(((Component)widget).gameObject, 0, 0, 0, 0);
				uIWidget.updateAnchors = UIRect.AnchorUpdate.OnEnable;
				uIWidget.leftAnchor.absolute = 5;
				uIWidget.leftAnchor.relative = num4;
				num4 += num6;
				uIWidget.rightAnchor.absolute = -5;
				uIWidget.rightAnchor.relative = num4;
				uIWidget.UpdateAnchors();
			}
		}
	}
}

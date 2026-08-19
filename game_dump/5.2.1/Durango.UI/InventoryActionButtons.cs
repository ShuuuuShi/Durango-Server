using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Render.Camera;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class InventoryActionButtons : MonoBehaviour
{
	[SerializeField]
	private SelectableButton _useButton;

	private UseType _useType;

	private UIWidget _widget;

	private List<UseType> _usableList;

	private readonly List<string> _usableListNames = new List<string>();

	private List<ItemData> _selectedItems;

	private StringSelector _stringSelector;

	private float _buttonLockedTime;

	public Func<UseType, string> UseTypeToString { get; set; }

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public event Action<UseType> OnUse;

	private void Start()
	{
		_useButton.Init();
		_useButton.Clicked = ButtonClicked;
		_useButton.CanClickWhenDisabled = true;
		if (_useButton.SubButton != null)
		{
			_useButton.SubButton.CanClickWhenDisabled = true;
		}
		_useButton.SubClicked = delegate
		{
			PopupUsableActionList(_stringSelector == null);
		};
	}

	private void ButtonClicked()
	{
		if (_useButton.Disabled)
		{
			switch (_useType)
			{
			case UseType.Taming:
				UIManager.SystemMsg(T._("포획된 동물은 길들이기 축사에서  길들일 수 있습니다."));
				break;
			case UseType.Imprint:
				if (_selectedItems.Count == 1)
				{
					ItemData itemData = _selectedItems[0];
					UIManager.SystemMsg(T._("생존 레벨이 {0} 이상 되어야 귀속할 수 있습니다.", itemData.Level));
				}
				break;
			}
		}
		else
		{
			Submit();
		}
	}

	private void Submit()
	{
		float time = Time.time;
		if (!(time < _buttonLockedTime))
		{
			_buttonLockedTime = time + 0.5f;
			if (this.OnUse != null)
			{
				this.OnUse(_useType);
			}
		}
	}

	public Transform GetUseButton()
	{
		return _useButton.transform;
	}

	public void UpdateUseButtonAction(List<UseType> list, List<ItemData> selectedItems)
	{
		_usableList = list;
		_selectedItems = selectedItems;
		int size = KUtility.GetSize(_usableList);
		_useType = ((size != 0) ? _usableList[0] : UseType.None);
		_useButton.SetStyle((size > 1) ? PresetButton.Style.SolidPopup : PresetButton.Style.Solid);
		UIUtility.UpdateAnchors(_useButton.transform);
		UpdateUseButton();
	}

	private void UpdateUseButton()
	{
		_useButton.Disabled = _useType == UseType.None || !Inventory.CheckEnableUseType(_selectedItems, _useType);
		_useButton.Text = GetUseTypeName(_useType);
	}

	private string GetUseTypeName(UseType type)
	{
		if (UseTypeToString == null)
		{
			return type.GetName();
		}
		return UseTypeToString(type);
	}

	public void PopupUsableActionList(bool show, int width = 0)
	{
		if (show && _usableList.Count > 0)
		{
			_usableListNames.Clear();
			int i = 0;
			for (int size = KUtility.GetSize(_usableList); i < size; i++)
			{
				_usableListNames.Add(GetUseTypeName(_usableList[i]));
			}
			StringSelector stringSelector = UIManager.Popup.Tooltip<StringSelector>();
			stringSelector.Set(_usableListNames, OnSelectUsableAction);
			stringSelector.AddOnFinished(SelectorHided);
			if (width != 0)
			{
				stringSelector.MinWidth = width;
				stringSelector.MaxWidth = width;
			}
			else
			{
				stringSelector.MinWidth = Widget.width;
				stringSelector.MaxWidth = width;
			}
			stringSelector.Show();
			if (width != 0)
			{
				Vector3 pos = MainCamera.ScreenPosToNGUIPos(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0f)) + Vector3.down * 5f;
				stringSelector.Widget.SetPosition(pos, 0f, 1f);
			}
			else
			{
				stringSelector.Widget.SetPosition(stringSelector.transform.parent.InverseTransformPoint(base.transform.TransformPoint(Widget.localCorners[1])), 0f, 0f);
			}
			stringSelector.IntoSafeArea();
			_stringSelector = stringSelector;
		}
		else if (_stringSelector != null)
		{
			_stringSelector.Hide();
			_stringSelector = null;
		}
	}

	private void SelectorHided()
	{
		_stringSelector = null;
		PopupUsableActionList(show: false);
	}

	private void OnSelectUsableAction(int index)
	{
		_useType = _usableList[index];
		UpdateUseButton();
		Submit();
	}
}

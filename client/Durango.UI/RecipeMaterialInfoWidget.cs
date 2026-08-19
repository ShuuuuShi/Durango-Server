using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class RecipeMaterialInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _pinButton;

	[SerializeField]
	private RecipeMaterialInfoItem _materialBaseItem;

	private UIWidget _widget;

	private ListObjectPool<RecipeMaterialInfoItem> _materialItems;

	private IList<RecipeInfoWidget.SlotStruct> _slots;

	private bool _isInit;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public event Action PinClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_materialItems = new ListObjectPool<RecipeMaterialInfoItem>();
		_materialItems.BaseObject = _materialBaseItem;
		_materialItems.Init(delegate(RecipeMaterialInfoItem o)
		{
			UIUtility.ResetAndUpdateAnchors(o.transform);
			o.Clicked = (Action<RecipeMaterialInfoItem>)Delegate.Combine(o.Clicked, new Action<RecipeMaterialInfoItem>(OnClickMaterialItem));
		});
		Selectable pinButton = _pinButton;
		pinButton.Clicked = (Action)Delegate.Combine(pinButton.Clicked, (Action)delegate
		{
			if (this.PinClicked != null)
			{
				this.PinClicked();
			}
		});
	}

	public void Set(string title, IList<RecipeInfoWidget.SlotStruct> list)
	{
		Init();
		_slots = list;
		_titleLabel.text = title;
		_materialItems.Set(list.Count);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			RecipeMaterialInfoItem recipeMaterialInfoItem = _materialItems[i];
			recipeMaterialInfoItem.Set(list[i]);
		}
		_titleWidget.SetPosition(Vector3.zero, 0.5f, 1f);
		Vector3 position = _titleWidget.GetPosition(0.5f, 0f);
		float num = UIUtility.WidgetsReposition(_materialItems, Vector3.down, position);
		num += (float)_titleWidget.height;
		Widget.height = (int)num;
	}

	public void SetPinButton(bool? isPin)
	{
		if (!isPin.HasValue || GameManager.Region.IsWarpRush())
		{
			_pinButton.gameObject.SetActive(value: false);
			return;
		}
		_pinButton.gameObject.SetActive(value: true);
		_pinButton.Selected = isPin.Value;
	}

	private void OnClickMaterialItem(RecipeMaterialInfoItem obj)
	{
		if (_slots != null)
		{
			int index = _materialItems.IndexOf(obj);
			RecipeInfoWidget.SlotStruct slotStruct = _slots.Get(index);
			SlotInfoPopup slotInfoPopup = UIManager.Popup.Tooltip<SlotInfoPopup>();
			slotInfoPopup.Set(slotStruct.Name, slotStruct.RequiredLevel, slotStruct.Tags, slotStruct.Materials, slotStruct.SourceInfos);
			slotInfoPopup.Show();
		}
	}
}

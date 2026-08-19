using System;
using System.Collections.Generic;
using UnityEngine;

public class ComboBox : MonoBehaviour
{
	public enum Direction
	{
		Up,
		Down
	}

	public Action<int> ItemSelected;

	[SerializeField]
	private ListObjectPool _items;

	[SerializeField]
	private UIScrollView _itemScrollView;

	[SerializeField]
	private GameObject _touchObj;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _arrowButton;

	[SerializeField]
	private Direction _direction;

	[SerializeField]
	private float _margin;

	private bool _isPopupShow;

	private UIWidget _widget;

	private IList<string> _popupList;

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

	private void Awake()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		_items.Init(InitComboboxItem);
		if ((Object)(object)_touchObj == (Object)null)
		{
			_touchObj = ((Component)this).gameObject;
		}
		UIEventListener.Get(_touchObj).onClick = OnClick_TouchBox;
		if ((Object)(object)_arrowButton != (Object)null)
		{
			TweenRotation tweenRotation = ((Component)_arrowButton).GetComponent<TweenRotation>();
			if ((Object)(object)tweenRotation == (Object)null)
			{
				tweenRotation = ((Component)_arrowButton).gameObject.AddComponent<TweenRotation>();
				tweenRotation.duration = 0.3f;
			}
			tweenRotation.from = ((Component)_arrowButton).transform.localEulerAngles;
			tweenRotation.to = tweenRotation.from + Vector3.forward * 180f;
			((Behaviour)tweenRotation).enabled = false;
		}
	}

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnPressObject));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnPressObject));
		((Component)_itemScrollView).gameObject.SetActive(false);
		_items.Set(0);
	}

	private void OnClick_TouchBox(GameObject obj)
	{
		if (_isPopupShow)
		{
			HidePopupList();
		}
		else
		{
			ShowPopupList();
		}
	}

	private void InitComboboxItem(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClickComboboxItem;
		ComboBoxItem component = obj.GetComponent<ComboBoxItem>();
		if ((Object)(object)component != (Object)null)
		{
			component.Parent = this;
			component.Widget.alpha = 0f;
		}
	}

	private void OnClickComboboxItem(GameObject obj)
	{
		int num = -1;
		int i = 0;
		for (int count = _items.Count; i < count; i++)
		{
			if ((Object)(object)_items[i] == (Object)(object)obj)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			if ((Object)(object)_textLabel != (Object)null)
			{
				ComboBoxItem component = obj.GetComponent<ComboBoxItem>();
				_textLabel.text = component.Text;
			}
			if (ItemSelected != null)
			{
				ItemSelected(num);
			}
			HidePopupList();
		}
	}

	public void SetLabel(string text, Color color)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_textLabel != (Object)null)
		{
			_textLabel.text = text;
			_textLabel.color = color;
		}
		if ((Object)(object)_arrowButton != (Object)null)
		{
			_arrowButton.color = color;
		}
	}

	public void Set(IList<string> items)
	{
		_popupList = items;
	}

	public void ShowPopupList()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		if (_isPopupShow)
		{
			return;
		}
		_isPopupShow = true;
		int num = ((_popupList != null) ? _popupList.Count : 0);
		_items.Set(num);
		Vector3 val = ((_direction != 0) ? Vector3.down : Vector3.up);
		Vector3 val3;
		if ((Object)(object)_itemScrollView == (Object)null)
		{
			UIWidget component = _items.BaseObject.GetComponent<UIWidget>();
			Vector2 val2 = Widget.pivotOffset - component.pivotOffset;
			val3 = val * (float)Widget.height + Vector3.right * val2.x * (float)Widget.width + Vector3.up * val2.y * (float)Widget.height;
		}
		else
		{
			((Component)_itemScrollView).gameObject.SetActive(true);
			val3 = Vector3.zero;
			Vector2 pivotOffset = Widget.pivotOffset;
			Vector3 val4 = Vector3.right * (pivotOffset.x - 0.5f) * (float)Widget.width + Vector3.up * (pivotOffset.y - 0.5f) * (float)Widget.height;
			UIPanel panel = _itemScrollView.panel;
			Vector4 finalClipRegion = panel.finalClipRegion;
			finalClipRegion.x = 0f;
			finalClipRegion.y = 0f;
			panel.clipOffset = Vector2.zero;
			panel.baseClipRegion = finalClipRegion;
			((Component)_itemScrollView).transform.localPosition = val4 + val * ((float)Widget.height / 2f + panel.height / 2f - panel.clipSoftness.y);
		}
		for (int i = 0; i < num; i++)
		{
			_items[i].transform.localPosition = val3;
			ComboBoxItem component2 = _items[i].GetComponent<ComboBoxItem>();
			if ((Object)(object)component2 != (Object)null)
			{
				component2.Text = _popupList[i];
				component2.Index = i;
				val3 += val * (component2.GetHeight() + _margin);
				component2.Show();
			}
			else
			{
				UIWidget component3 = _items[i].GetComponent<UIWidget>();
				val3 += val * ((float)component3.height + _margin);
			}
		}
		if ((Object)(object)_arrowButton != (Object)null)
		{
			TweenRotation component4 = ((Component)_arrowButton).GetComponent<TweenRotation>();
			component4.PlayForward();
		}
		if ((Object)(object)_itemScrollView != (Object)null)
		{
			_itemScrollView.ResetPosition();
		}
	}

	public void HidePopupList()
	{
		if (!_isPopupShow)
		{
			return;
		}
		_isPopupShow = false;
		int i = 0;
		for (int count = _items.Count; i < count; i++)
		{
			ComboBoxItem component = _items[i].GetComponent<ComboBoxItem>();
			if ((Object)(object)component != (Object)null)
			{
				component.Hide();
			}
			else
			{
				_items[i].gameObject.SetActive(false);
			}
		}
		if ((Object)(object)_arrowButton != (Object)null)
		{
			TweenRotation component2 = ((Component)_arrowButton).GetComponent<TweenRotation>();
			component2.PlayReverse();
		}
	}

	private void OnPressObject(GameObject obj, bool press)
	{
		if (_isPopupShow && press && !NGUITools.IsChild(((Component)this).transform, obj.transform))
		{
			HidePopupList();
		}
	}
}

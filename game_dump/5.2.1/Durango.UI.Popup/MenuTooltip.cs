using System;
using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class MenuTooltip : TooltipBase
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private ListObjectPool _menuItems;

	[SerializeField]
	private int _minWidth;

	private string _title;

	private IList<string> _menus;

	private Action<int> _callback;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_menuItems.Init(OnInitMenuItem);
		}
	}

	private void OnInitMenuItem(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClickMenuItem;
		UIEventListener.Get(obj).onDrag = OnDragMenuItem;
	}

	private void OnClickMenuItem(GameObject obj)
	{
		int num = -1;
		int i = 0;
		for (int count = _menuItems.Count; i < count; i++)
		{
			if (_menuItems[i] == obj)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			if (_callback != null)
			{
				_callback(num);
			}
			Hide();
		}
	}

	private void OnDragMenuItem(GameObject obj, Vector2 delta)
	{
		OnDrag(delta);
	}

	protected override void OnAwake()
	{
		SoundType = UISound.GroupType.NoSound;
	}

	protected override void OnHide()
	{
		base.OnHide();
		_callback = null;
	}

	public void Set(string title, IList<string> menus, Action<int> callback)
	{
		Init();
		_title = title;
		_menus = menus;
		_callback = callback;
	}

	protected override void FillData()
	{
		_titleLabel.text = _title;
		_menuItems.Set((_menus != null) ? _menus.Count : 0);
		int i = 0;
		for (int count = _menuItems.Count; i < count; i++)
		{
			Transform obj = _menuItems[i].transform;
			UISpriteLabel component = obj.Find("Label").GetComponent<UISpriteLabel>();
			GameObject obj2 = obj.Find("Line").gameObject;
			component.text = _menus[i];
			obj2.SetActive(i < count - 1);
		}
	}

	protected override void UpdateLayout()
	{
		float num = Mathf.Abs(_titleWidget.transform.localPosition.y);
		float num2 = Mathf.Abs(_titleLabel.transform.localPosition.y);
		float num3 = _titleLabel.printedSize.x;
		int i = 0;
		for (int count = _menuItems.Count; i < count; i++)
		{
			UILabel component = _menuItems[i].transform.Find("Label").GetComponent<UILabel>();
			num3 = Mathf.Max(num3, component.printedSize.x);
		}
		num3 += num * 2f + num2 * 2f;
		base.Widget.width = Mathf.Max((int)num3, _minWidth);
		int num4 = (int)((float)base.Widget.width - num * 2f);
		_titleWidget.width = num4;
		Vector3 vector = _titleWidget.transform.localPosition + Vector3.down * _titleWidget.height;
		float num5 = num * 2f + (float)_titleWidget.height;
		int num6 = 0;
		int j = 0;
		for (int count2 = _menuItems.Count; j < count2; j++)
		{
			UIWidget component2 = _menuItems[j].GetComponent<UIWidget>();
			component2.transform.localPosition = vector + Vector3.down * num6;
			component2.width = num4;
			Transform obj = component2.transform.Find("Label");
			Vector3 localPosition = obj.localPosition;
			localPosition.x = (float)num4 * 0.5f - 10f;
			obj.localPosition = localPosition;
			num6 += component2.height;
		}
		num5 += (float)num6;
		base.Widget.height = (int)num5;
		UIUtility.UpdateAnchors(base.transform);
	}
}

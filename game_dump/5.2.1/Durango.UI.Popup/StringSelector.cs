using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Popup;

public class StringSelector : TooltipBase
{
	[SerializeField]
	private StringSelectItemWidget _item;

	[SerializeField]
	private int _verticalPadding;

	private bool _dragLock = true;

	private ListObjectPool<StringSelectItemWidget> _list;

	private IEnumerable<string> _items;

	private Action<int> _onSelected;

	private bool _isDown;

	public override bool DragLock
	{
		get
		{
			return _dragLock;
		}
		set
		{
			_dragLock = value;
		}
	}

	public int MinWidth { get; set; }

	public int MaxWidth { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		SoundType = UISound.GroupType.NoSound;
		_list = new ListObjectPool<StringSelectItemWidget>();
		_list.BaseObject = _item;
		_list.Init(delegate(StringSelectItemWidget widget)
		{
			widget.Clicked += OnClickSelectItemWidget;
			widget.Draged += base.OnDrag;
		});
	}

	protected override void OnHide()
	{
		base.OnHide();
		MinWidth = 0;
		_dragLock = true;
	}

	public void SetItemColor(int index, Color color)
	{
		if (index >= 0 && index < _list.Count)
		{
			_list[index].SetColor(color);
		}
	}

	public void SetItemBold(int index, bool bold)
	{
		if (index >= 0 && index < _list.Count)
		{
			_list[index].SetBold(bold);
		}
	}

	public void Set(IEnumerable<string> items, Action<int> onSelected, bool isDown = false)
	{
		_items = items;
		_onSelected = onSelected;
		_isDown = isDown;
	}

	protected override void FillData()
	{
		_list.BeginLoad();
		StringSelectItemWidget stringSelectItemWidget = null;
		foreach (string item in _items)
		{
			StringSelectItemWidget next = _list.GetNext();
			next.SetText(item);
			if (_isDown)
			{
				next.EnableSeparator(stringSelectItemWidget != null);
			}
			else
			{
				next.EnableSeparator(enable: true);
			}
			stringSelectItemWidget = next;
		}
		_list.EndLoad();
		if (!_isDown && stringSelectItemWidget != null)
		{
			stringSelectItemWidget.EnableSeparator(enable: false);
		}
	}

	protected override void UpdateLayout()
	{
		float num = 0f;
		for (int i = 0; i < _list.Count; i++)
		{
			num = Mathf.Max(num, _list[i].TextWidth);
		}
		int num2 = Mathf.Max(MinWidth, (int)num + 30);
		if (MaxWidth > 0)
		{
			num2 = Mathf.Min(num2, MaxWidth);
		}
		for (int j = 0; j < _list.Count; j++)
		{
			_list[j].SetWidth(num2);
		}
		base.Widget.pivot = (_isDown ? UIWidget.Pivot.Top : UIWidget.Pivot.Bottom);
		_list.BaseObject.Widget.SetPosition(Vector2.zero, 0.5f, (!_isDown) ? 0f : 1f);
		float num3 = _list.Reposition((!_isDown) ? Vector3.up : Vector3.down, _verticalPadding);
		base.Widget.SetDimensions(num2, (int)num3);
		UIUtility.UpdateAnchors(base.transform);
	}

	private void OnClickSelectItemWidget(StringSelectItemWidget widget)
	{
		if (_onSelected != null)
		{
			_onSelected(_list.IndexOf(widget));
		}
		Hide();
	}
}

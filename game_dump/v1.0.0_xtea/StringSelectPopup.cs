using System;
using System.Collections.Generic;
using UnityEngine;

public class StringSelectPopup : TooltipBase
{
	[SerializeField]
	private DrumSelector _selector;

	[SerializeField]
	private int _textPadding;

	[SerializeField]
	private int _verticalPadding;

	private IList<string> _items;

	private Action<int> _onSelected;

	private int _initIndex;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	public void Set(IList<string> items, Action<int> onSelected, int index = 0)
	{
		_items = items;
		_onSelected = onSelected;
		_initIndex = index;
	}

	protected override void FillData()
	{
		_selector.Set(_items);
		_selector.SetIndex(_initIndex);
		_selector.Refresh();
	}

	protected override void UpdateLayout()
	{
		int width = _selector.ResizeWidth(_textPadding);
		base.Widget.width = width;
		base.Widget.height = (int)(_selector.R * 2f) + _verticalPadding * 2;
		UIUtility.UpdateAnchors(((Component)this).transform);
	}

	protected override void OnClickWidget()
	{
		base.OnClickWidget();
		if (_onSelected != null)
		{
			_onSelected(_selector.Index);
		}
	}
}

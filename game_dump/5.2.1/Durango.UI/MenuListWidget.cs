using System.Collections.Generic;
using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public class MenuListWidget : MenuListWidgetBase
{
	[SerializeField]
	private UIWidget _verticalLine;

	[SerializeField]
	private int _menuMinimumWidth = 270;

	[SerializeField]
	private float _tweenerDelay;

	public void Clear()
	{
		Init();
		_menuList.Clear();
		base.gameObject.SetActive(value: false);
	}

	public void Set(IEnumerable<MenuType> types)
	{
		Init();
		_menuList.BeginLoad();
		int a = _menuMinimumWidth;
		float num = 0f;
		foreach (MenuType type in types)
		{
			MenuWidget next = _menuList.GetNext();
			next.Set(type);
			next.Widget.width = base.width;
			next.PlayTweener(num);
			a = Mathf.Max(a, next.GetPreferredSize());
			num += _tweenerDelay;
		}
		_menuList.EndLoad();
		if (_menuList.Count > 0)
		{
			base.gameObject.SetActive(value: true);
			int num2 = (int)UIUtility.WidgetsReposition(_menuList, Vector3.down, Vector3.zero, 0f, GetRepositionPivotValue());
			if (_verticalLine != null)
			{
				_verticalLine.height = num2;
			}
			UIUtility.UpdateAnchors(base.transform);
			for (int i = 0; i < _menuList.Count; i++)
			{
				_menuList[i].Widget.width = a;
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void SetSelection(MenuType? type = null)
	{
		Init();
		for (int i = 0; i < _menuList.Count; i++)
		{
			_menuList[i].Selected = type.HasValue && type.Value == _menuList[i].Type;
		}
	}

	private float GetRepositionPivotValue()
	{
		switch (base.pivot)
		{
		case Pivot.TopLeft:
		case Pivot.Top:
		case Pivot.TopRight:
			return 0f;
		case Pivot.BottomLeft:
		case Pivot.Bottom:
		case Pivot.BottomRight:
			return 1f;
		default:
			return 0.5f;
		}
	}
}

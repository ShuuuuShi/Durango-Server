using System;
using Durango.Logic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public abstract class MenuListWidgetBase : UIWidget
{
	[SerializeField]
	protected MenuWidget _baseNode;

	protected ListObjectPool<MenuWidget> _menuList;

	private bool _isInit;

	public event Action<MenuType> MenuClicked;

	protected void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_menuList = new ListObjectPool<MenuWidget>();
			_menuList.BaseObject = _baseNode;
			_menuList.UseBase = true;
			_menuList.Init(delegate(MenuWidget comp)
			{
				comp.Clicked = (Action)Delegate.Combine(comp.Clicked, new Action(OnClickMenuItem));
			});
			OnInitialized();
		}
	}

	protected virtual void OnInitialized()
	{
	}

	public virtual bool TryGetMenuItem(MenuType type, out MenuWidget comp)
	{
		for (int i = 0; i < _menuList.Count; i++)
		{
			if (_menuList[i].Type == type)
			{
				comp = _menuList[i];
				return true;
			}
		}
		comp = null;
		return false;
	}

	protected void OnClickMenuItem()
	{
		MenuWidget menuWidget = Selectable.Current as MenuWidget;
		if (!(menuWidget == null))
		{
			OnMenuClick(menuWidget.Type);
		}
	}

	protected virtual void OnMenuClick(MenuType type)
	{
		if (this.MenuClicked != null)
		{
			this.MenuClicked(type);
		}
	}
}

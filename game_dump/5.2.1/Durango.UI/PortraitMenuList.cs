using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class PortraitMenuList : MenuListWidgetBase, IMenuList
{
	[SerializeField]
	private UIWidget _scrollContainer;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	protected MenuListWidget _childMenuList;

	[SerializeField]
	private SubMenuListWidget _subMenuList;

	[SerializeField]
	private int _menuMinimumWidth = 270;

	private MenuType? _currentCategory;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_childMenuList.MenuClicked += MenuList_MenuClicked;
		_subMenuList.MenuClicked += MenuList_MenuClicked;
		AddOnChange(OnChange);
	}

	protected override void OnMenuClick(MenuType type)
	{
		if (MenuContainer.HasChildren(type))
		{
			SetChildMenuList(type);
			return;
		}
		ClearChildMenuList();
		base.OnMenuClick(type);
	}

	private void OnChange()
	{
		if (Application.isPlaying)
		{
			Vector3[] array = localCorners;
			_subMenuList.SetPosition(Vector3.Lerp(array[1], array[2], 0.5f), 0.5f, 1f);
			_scrollView.ResetPosition();
			_childMenuList.UpdateAnchors();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			alpha = 0f;
		}
	}

	public void Refresh()
	{
		Init();
		_menuList.BeginLoad();
		foreach (MenuType item in MenuContainer.FirstDepthMenus.Where((MenuType menu) => MenuContainer.HasChildren(menu) ? MenuContainer.GetChildren(menu).Any((MenuType c) => GameSystem<MenuSystem>.Instance().IsEnabled(c)) : GameSystem<MenuSystem>.Instance().IsEnabled(menu)))
		{
			_menuList.GetNext().Set(item);
		}
		_menuList.EndLoad();
		_subMenuList.Set(MenuContainer.FixedMenus);
		Vector3[] array = localCorners;
		_subMenuList.SetPosition(Vector3.Lerp(array[1], array[2], 0.5f), 0.5f, 1f);
		List<UIWidget> widgets = _scrollView.Widgets;
		widgets.Clear();
		for (int i = 0; i < _menuList.Count; i++)
		{
			widgets.Add(_menuList[i].Widget);
		}
		int num = _menuMinimumWidth;
		for (int j = 0; j < _menuList.Count; j++)
		{
			num = Mathf.Max(num, _menuList[j].GetPreferredSize());
		}
		_scrollContainer.rightAnchor.absolute = num;
		for (int k = 0; k < _menuList.Count; k++)
		{
			_menuList[k].Widget.width = num;
		}
		UIUtility.UpdateAnchors(_scrollContainer.transform);
		_scrollView.ResetPosition();
		UpdateChildMenuList();
	}

	public void Show(bool instant)
	{
		Init();
		base.gameObject.SetActive(value: true);
		if (instant)
		{
			this.SetEnable<TweenAlpha>(enable: false);
			alpha = 1f;
		}
		else
		{
			TweenAlpha.Begin(base.gameObject, 0.2f, 1f);
		}
	}

	public void Hide()
	{
		Init();
		ClearChildMenuList();
		base.gameObject.SetActive(value: false);
	}

	private void ClearChildMenuList()
	{
		_currentCategory = null;
		UpdateChildMenuList();
	}

	private void SetChildMenuList(MenuType type)
	{
		if (_currentCategory.HasValue && _currentCategory.Value == type)
		{
			ClearChildMenuList();
			return;
		}
		_currentCategory = type;
		UpdateChildMenuList();
	}

	private void UpdateChildMenuList()
	{
		for (int i = 0; i < _menuList.Count; i++)
		{
			_menuList[i].Selected = _currentCategory.HasValue && _currentCategory.Value == _menuList[i].Type;
		}
		if (_currentCategory.HasValue)
		{
			_childMenuList.Set(from c in MenuContainer.GetChildren(_currentCategory.Value)
				where GameSystem<MenuSystem>.Instance().IsEnabled(c)
				select c);
		}
		else
		{
			_childMenuList.Clear();
		}
	}

	private void MenuList_MenuClicked(MenuType type)
	{
		base.OnMenuClick(type);
	}
}

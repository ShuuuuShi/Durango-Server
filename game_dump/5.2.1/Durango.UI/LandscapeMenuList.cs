using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class LandscapeMenuList : LandscapeMenuListBase, IUIInitializable
{
	[SerializeField]
	protected MenuListWidget _leftMenuList;

	[SerializeField]
	protected MenuListWidget _leftChildMenuList;

	[SerializeField]
	protected MenuListWidget _rightMenuList;

	[SerializeField]
	protected MenuListWidget _rightChildMenuList;

	[SerializeField]
	protected SubMenuListWidget _subMenuList;

	[SerializeField]
	private SelectableWidget _lockButton;

	private int _maxLeftMenuCount;

	private MenuType? _currentCategory;

	private MenuListWidget _currentChildMenuList;

	void IUIInitializable.Init()
	{
		_maxLeftMenuCount = ((GameManager.ClusterMode != 0) ? 7 : 6);
		_leftMenuList.MenuClicked += LeftMenuList_MenuClicked;
		_rightMenuList.MenuClicked += RightMenuList_MenuClicked;
		_leftChildMenuList.MenuClicked += base.OnMenuClick;
		_rightChildMenuList.MenuClicked += base.OnMenuClick;
		_subMenuList.MenuClicked += base.OnMenuClick;
		SelectableWidget lockButton = _lockButton;
		lockButton.Clicked = (Action)Delegate.Combine(lockButton.Clicked, new Action(base.OnLockClick));
		AddOnChange(OnChange);
	}

	protected override void OnChange()
	{
		if (Application.isPlaying)
		{
			_subMenuList.SetPosition(localCorners[1] + new Vector3(_lockButton.Widget.width, 0f), 0f, 1f);
			_leftMenuList.UpdateAnchors();
			_leftChildMenuList.UpdateAnchors();
			_rightMenuList.UpdateAnchors();
			_rightChildMenuList.UpdateAnchors();
			UpdateListMenus();
		}
	}

	private void UpdateListMenus()
	{
		IEnumerable<MenuType> source = MenuContainer.FirstDepthMenus.Where((MenuType menu) => MenuContainer.HasChildren(menu) ? MenuContainer.GetChildren(menu).Any((MenuType c) => GameSystem<MenuSystem>.Instance().IsEnabled(c)) : GameSystem<MenuSystem>.Instance().IsEnabled(menu));
		_leftMenuList.Set(source.Take(_maxLeftMenuCount));
		_rightMenuList.Set(source.Skip(_maxLeftMenuCount));
		UpdateChildMenuList();
	}

	public override void Refresh()
	{
		UpdateListMenus();
		_subMenuList.Set(MenuContainer.FixedMenus);
	}

	public override bool TryGetMenuItem(MenuType type, out MenuWidget comp)
	{
		if (_leftMenuList.TryGetMenuItem(type, out comp))
		{
			return true;
		}
		if (_rightMenuList.TryGetMenuItem(type, out comp))
		{
			return true;
		}
		if (_subMenuList.TryGetMenuItem(type, out comp))
		{
			return true;
		}
		if (_currentChildMenuList != null && _currentChildMenuList.TryGetMenuItem(type, out comp))
		{
			return true;
		}
		return false;
	}

	public override void Hide()
	{
		ClearChildMenuList();
		base.Hide();
	}

	private void ClearChildMenuList()
	{
		_currentCategory = null;
		_currentChildMenuList = null;
		UpdateChildMenuList();
	}

	private void SetChildMenuList(MenuType type, [NotNull] MenuListWidget parent, [NotNull] MenuListWidget child)
	{
		if (_currentCategory.HasValue && _currentCategory.Value == type)
		{
			ClearChildMenuList();
			return;
		}
		_currentCategory = type;
		_currentChildMenuList = child;
		parent.SetSelection(type);
		UpdateChildMenuList();
	}

	private void UpdateChildMenuList()
	{
		if (_leftChildMenuList != _currentChildMenuList)
		{
			_leftMenuList.SetSelection();
			_leftChildMenuList.Clear();
		}
		if (_rightChildMenuList != _currentChildMenuList)
		{
			_rightMenuList.SetSelection();
			_rightChildMenuList.Clear();
		}
		if (_currentChildMenuList != null)
		{
			_currentChildMenuList.Set((!_currentCategory.HasValue) ? Enumerable.Empty<MenuType>() : (from c in MenuContainer.GetChildren(_currentCategory.Value)
				where GameSystem<MenuSystem>.Instance().IsEnabled(c)
				select c));
		}
	}

	private void LeftMenuList_MenuClicked(MenuType type)
	{
		if (MenuContainer.HasChildren(type))
		{
			SetChildMenuList(type, _leftMenuList, _leftChildMenuList);
			return;
		}
		ClearChildMenuList();
		OnMenuClick(type);
	}

	private void RightMenuList_MenuClicked(MenuType type)
	{
		if (MenuContainer.HasChildren(type))
		{
			SetChildMenuList(type, _rightMenuList, _rightChildMenuList);
			return;
		}
		ClearChildMenuList();
		OnMenuClick(type);
	}
}

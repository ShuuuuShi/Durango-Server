using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class LandscapeMenuList_PC : LandscapeMenuListBase, IUIInitializable
{
	[SerializeField]
	protected MenuListWidget_PC _menuList;

	[SerializeField]
	protected SubMenuListWidget _subMenuList;

	[SerializeField]
	private SelectableWidget _exitGameButton;

	[SerializeField]
	private UILabel _exitGameLabel;

	private readonly List<MenuType> _mainMenus = new List<MenuType>();

	void IUIInitializable.Init()
	{
		_menuList.MenuClicked += base.OnMenuClick;
		_subMenuList.MenuClicked += base.OnMenuClick;
		SelectableWidget exitGameButton = _exitGameButton;
		exitGameButton.Clicked = (Action)Delegate.Combine(exitGameButton.Clicked, (Action)delegate
		{
			MessageBox messageBox = UIManager.MessageBox;
			if (messageBox != null)
			{
				messageBox.Show(T._("종료하시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						Platform.Instance.Quit();
					}
				});
			}
			else
			{
				Platform.Instance.Quit();
			}
		});
		_exitGameLabel.text = T._("게임 나가기");
		_exitGameLabel.ProcessText();
		_exitGameButton.GetComponent<RectLayoutComponent>().UpdateLayout();
		AddOnChange(OnChange);
	}

	protected override void OnChange()
	{
		if (Application.isPlaying)
		{
			_menuList.UpdateAnchors();
			UpdateListMenus();
		}
	}

	private void UpdateListMenus()
	{
		int index = 0;
		_menuList.BeginSetting();
		while (_menuList.Set(_mainMenus, ref index))
		{
		}
		_menuList.FinishSetting();
		_menuList.gameObject.SetActive(_menuList.HasMenu());
	}

	public override void Refresh()
	{
		_mainMenus.Clear();
		_mainMenus.AddRange(MenuContainer.Menus);
		_mainMenus.Add(MenuType.WorldMap);
		UpdateListMenus();
		List<MenuType> list = MenuContainer.FixedMenus.Where((MenuType x) => x != MenuType.Config).ToList();
		list.Add(MenuType.Config);
		_subMenuList.Set(list);
	}

	public override bool TryGetMenuItem(MenuType type, out MenuWidget comp)
	{
		if (_menuList.TryGetMenuItem(type, out comp))
		{
			return true;
		}
		if (_subMenuList.TryGetMenuItem(type, out comp))
		{
			return true;
		}
		return false;
	}
}

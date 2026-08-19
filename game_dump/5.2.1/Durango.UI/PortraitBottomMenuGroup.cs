using Durango.Logic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class PortraitBottomMenuGroup : UIBase
{
	[SerializeField]
	private UIWidget _menuContainer;

	[SerializeField]
	private KScrollView _menuList;

	public int BottomMenuHeight => _menuContainer.height;

	private void Start()
	{
		InitMenuList();
		UIBase.UIOpened += RefreshBottomMenuList;
		UIBase.UIClosed += RefreshBottomMenuList;
		SetChildrenActive(activated: false);
	}

	private void OnEnable()
	{
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated += RefreshMenuList;
		RefreshMenuList();
	}

	private void OnDisable()
	{
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated -= RefreshMenuList;
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		RefreshBottomMenuList();
	}

	private void RefreshBottomMenuList()
	{
		if (!UIManager.IsPortraitScreen)
		{
			Close();
			return;
		}
		UIBase currentUI = UIBase.CurrentUI;
		if (currentUI != null && (currentUI.Anchor == AnchorType.Fullscreen || currentUI.Anchor == AnchorType.CloneFullscreen))
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	private void InitMenuList()
	{
		_menuList.Nodes.Clear();
		foreach (MenuType menu in MenuContainer.Menus)
		{
			MenuWidget menuWidget = _menuList.Nodes.Add<MenuWidget>();
			menuWidget.Set(menu);
			menuWidget.Clicked = OnClickMenuButton;
			if (Application.isEditor)
			{
				menuWidget.gameObject.name = menu.ToString();
			}
		}
		RefreshMenuList(init: true);
	}

	private void OnClickMenuButton()
	{
		MenuWidget menuWidget = Selectable.Current as MenuWidget;
		if (!(menuWidget == null))
		{
			UIBase script = MenuHelper.GetScript(menuWidget.Type);
			if (!(script == null) && !script.IsOpened)
			{
				float currentOffset = _menuList.CurrentOffset;
				UIBase.CloseAllUI();
				MenuHelper.Open(menuWidget.Type, immediately: true);
				_menuList.MoveTo(currentOffset, instant: true);
			}
		}
	}

	private void RefreshMenuList()
	{
		RefreshMenuList(init: false);
	}

	private void RefreshMenuList(bool init)
	{
		ListObjectPool nodes = _menuList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			MenuWidget component = nodes[i].GetComponent<MenuWidget>();
			if (!GameSystem<MenuSystem>.Instance().IsEnabled(component.Type))
			{
				component.gameObject.SetActive(value: false);
			}
			else
			{
				component.gameObject.SetActive(value: true);
			}
		}
		_menuList.Reposition(init, !init);
	}
}

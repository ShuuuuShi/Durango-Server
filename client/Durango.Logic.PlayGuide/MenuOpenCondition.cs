using Durango.UI;
using Durango.Utils.Extensions;

namespace Durango.Logic.PlayGuide;

internal class MenuOpenCondition : FlowCondition
{
	private readonly MenuType _menuType;

	public MenuOpenCondition(string menuType)
	{
		_menuType = menuType.ToEnum(MenuType.Character);
	}

	protected override void OnRegister()
	{
		MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
		if (menuListGroupBase != null)
		{
			menuListGroupBase.MenuOpened += MenuListGroup_MenuOpened;
		}
	}

	protected override void OnUnregister()
	{
		MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
		if (menuListGroupBase != null)
		{
			menuListGroupBase.MenuOpened -= MenuListGroup_MenuOpened;
		}
	}

	private void MenuListGroup_MenuOpened(MenuType type)
	{
		if (type == _menuType)
		{
			Interrupt();
		}
	}
}

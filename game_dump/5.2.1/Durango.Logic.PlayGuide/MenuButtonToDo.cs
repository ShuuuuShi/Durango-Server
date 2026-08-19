using Durango.UI;
using Durango.Utils.Extensions;

namespace Durango.Logic.PlayGuide;

public class MenuButtonToDo : ToDoBase
{
	private readonly MenuType _menu;

	public MenuButtonToDo(string id)
	{
		_menu = id.ToEnum(MenuType.Character);
	}

	public override void OnAddItem()
	{
		MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
		if (menuListGroupBase != null)
		{
			menuListGroupBase.MenuClicked += LeftMenuListGroup_MenuClicked;
		}
		else
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
		if (menuListGroupBase != null)
		{
			menuListGroupBase.MenuClicked -= LeftMenuListGroup_MenuClicked;
		}
	}

	private void LeftMenuListGroup_MenuClicked(MenuType menu)
	{
		if (_menu == menu)
		{
			CallComplete();
		}
	}
}

using MenuData;
using UnityEngine;

namespace PlayGuide;

public class MenuButtonToDo : ToDoBase
{
	private readonly MenuType _menu;

	public MenuButtonToDo(string id)
	{
		_menu = id.ToEnum(MenuType.Character);
	}

	public override void OnAddItem()
	{
		LeftMenuListGroup leftMenuListGroup = UIManager.FindScript<LeftMenuListGroup>();
		if ((Object)(object)leftMenuListGroup != (Object)null)
		{
			leftMenuListGroup.MenuClicked += LeftMenuListGroup_MenuClicked;
		}
		else
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		LeftMenuListGroup leftMenuListGroup = UIManager.FindScript<LeftMenuListGroup>();
		if ((Object)(object)leftMenuListGroup != (Object)null)
		{
			leftMenuListGroup.MenuClicked -= LeftMenuListGroup_MenuClicked;
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

using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorMenu : Locator
{
	private MenuListGroupBase _menuGroup;

	private MenuType? _parent;

	private MenuType _menuType;

	protected override void OnInitialized()
	{
		Parameter parameter = Parameters.Get("select_menu");
		if (parameter != null && !string.IsNullOrEmpty(parameter.id))
		{
			SetMenuType(parameter.id.ToEnum(MenuType.Character));
		}
		_menuGroup = UIManager.FindScript<MenuListGroupBase>();
	}

	protected override string SelectPhase()
	{
		if (_menuGroup != null && _menuGroup.IsMenuVisible())
		{
			return "select_menu";
		}
		return "bottom_left_menu";
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "bottom_left_menu":
			base.TargetTransform = ((!(_menuGroup != null)) ? null : _menuGroup.GetBottomLeftMenuTransform());
			base.CurrentParameter.rotate = 90f;
			break;
		case "select_menu":
		{
			Transform menuTransform = _menuGroup.GetMenuTransform(_menuType);
			if (menuTransform == null)
			{
				MenuType? parent = _parent;
				if (parent.HasValue)
				{
					menuTransform = _menuGroup.GetMenuTransform(_parent.Value);
				}
			}
			base.TargetTransform = menuTransform;
			break;
		}
		}
	}

	protected void SetMenuType(MenuType type)
	{
		_menuType = type;
		_parent = MenuContainer.GetParent(_menuType);
	}
}

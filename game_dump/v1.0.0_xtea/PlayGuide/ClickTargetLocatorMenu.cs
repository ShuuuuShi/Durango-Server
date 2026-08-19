using MenuData;
using UnityEngine;

namespace PlayGuide;

public class ClickTargetLocatorMenu : ClickTargetLocator
{
	private MenuType _menuType;

	private LeftMenuListGroup _leftMenuGroup;

	protected override void OnInitialized()
	{
		ClickTargetData clickTargetData = ClickTargetDict.Get("select_menu");
		if (clickTargetData != null)
		{
			_menuType = clickTargetData.id.ToEnum(MenuType.Character);
		}
		_leftMenuGroup = UIManager.FindScript<LeftMenuListGroup>();
	}

	protected override string SelectPhase()
	{
		if ((Object)(object)_leftMenuGroup != (Object)null && _leftMenuGroup.IsMenuVisible())
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
			base.TargetTransform = ((!((Object)(object)_leftMenuGroup != (Object)null)) ? null : _leftMenuGroup.GetBottomLeftMenuTransform());
			break;
		case "select_menu":
			base.TargetTransform = _leftMenuGroup.GetMenuTransform(_menuType);
			CurrentClickTarget.x = 0.02f;
			CurrentClickTarget.y = -0.01f;
			break;
		}
	}
}

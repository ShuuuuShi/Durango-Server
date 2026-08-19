using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class SubMenuListWidget_PC : SubMenuListWidget
{
	protected override void OnInitialized()
	{
		_menuList.UseBase = false;
	}

	public override void Set(IEnumerable<MenuType> types)
	{
		Init();
		LoadMenulist(types);
		for (int i = 0; i < _menuList.Count; i++)
		{
			_menuList[i].GetComponent<HoverShortcutViewer>().Set(_menuList[i].Type);
		}
		SetDimensions(_baseNode.Widget.width * _menuList.Count, _baseNode.Widget.height);
		UIUtility.WidgetsReposition(_menuList, this, Vector3.right);
	}
}

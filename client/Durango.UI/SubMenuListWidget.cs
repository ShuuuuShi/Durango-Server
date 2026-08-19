using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using UnityEngine;

namespace Durango.UI;

public class SubMenuListWidget : MenuListWidgetBase
{
	public virtual void Set(IEnumerable<MenuType> types)
	{
		Init();
		base.pivot = Pivot.Left;
		LoadMenulist(types);
		float f = UIUtility.WidgetsReposition(_menuList, Vector3.right, Vector3.zero);
		SetDimensions(Mathf.CeilToInt(f), _baseNode.Widget.height);
	}

	protected void LoadMenulist(IEnumerable<MenuType> types)
	{
		_menuList.BeginLoad();
		foreach (MenuType item in types.Where((MenuType t) => GameSystem<MenuSystem>.Instance().IsEnabled(t)))
		{
			MenuWidget next = _menuList.GetNext();
			next.Set(item);
		}
		Transform transform = _menuList[_menuList.Count - 1].transform.Find("separator");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: false);
		}
		_menuList.EndLoad();
	}
}

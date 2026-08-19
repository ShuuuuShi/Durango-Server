using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class MenuListWidget_PC : MenuListWidgetBase
{
	[SerializeField]
	private float _tooltipDuration;

	[SerializeField]
	private Vector2 _tooltipPos;

	[SerializeField]
	private KGridScrollView _gridScroll;

	private ListObjectPool _nodes;

	protected override void OnInitialized()
	{
		_menuList = null;
		_gridScroll.ScrollView.movement = UIScrollView.Movement.Vertical;
		_nodes = _gridScroll.Nodes;
		if (_nodes != null)
		{
			_nodes.Init(delegate(GameObject obj)
			{
				MenuWidget component = obj.GetComponent<MenuWidget>();
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(base.OnClickMenuItem));
			});
		}
	}

	public override bool TryGetMenuItem(MenuType type, out MenuWidget comp)
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			MenuWidget component = _nodes[i].GetComponent<MenuWidget>();
			if (component.Type == type)
			{
				comp = component;
				return true;
			}
		}
		comp = null;
		return false;
	}

	public bool HasMenu()
	{
		return _nodes != null && _nodes.Count > 0;
	}

	public void BeginSetting()
	{
		Init();
		UIUtility.UpdateAnchors(base.transform);
		_nodes.BeginLoad();
	}

	public bool Set(IList<MenuType> types, ref int index)
	{
		bool result = false;
		_gridScroll.Panel.bottomAnchor.absolute = 0;
		_gridScroll.Panel.UpdateAnchors();
		int size = KUtility.GetSize(types);
		while (index < size)
		{
			MenuType menuType = types[index];
			if (GameSystem<MenuSystem>.Instance().IsEnabled(menuType))
			{
				MenuWidget_PC component = _nodes.GetNext().GetComponent<MenuWidget_PC>();
				component.Set(menuType);
				component.SetShortcutLabel(menuType);
			}
			index++;
			result = true;
		}
		return result;
	}

	public void FinishSetting()
	{
		_nodes.EndLoad();
		_gridScroll.Reposition(resetPosition: true);
	}
}

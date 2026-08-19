using System;
using ClanData;
using L10N;
using UnityEngine;

public class ClanMenuTabs : MonoBehaviour
{
	[SerializeField]
	private KScrollView _tabsScroll;

	private ClanMenus[] _menus;

	private bool _isInit;

	public event Action<ClanMenus> Selected;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabsScroll.Nodes.Init(delegate(GameObject obj)
			{
				Selectable component = obj.GetComponent<Selectable>();
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTab));
			});
		}
	}

	private void OnClickTab()
	{
		int num = _tabsScroll.Nodes.IndexOf(((Component)Selectable.Current).gameObject);
		if (num != -1)
		{
			SelectMenu(_menus[num]);
		}
	}

	public void Set(ClanMenus[] menus)
	{
		Init();
		_menus = menus;
		ListObjectPool nodes = _tabsScroll.Nodes;
		nodes.Set(KUtility.GetSize(menus));
		for (int i = 0; i < nodes.Count; i++)
		{
			Selectable component = nodes[i].GetComponent<Selectable>();
			component.Select = false;
			UISprite component2 = ((Component)nodes[i].transform.FindChild("Icon")).GetComponent<UISprite>();
			UILabel component3 = ((Component)nodes[i].transform.FindChild("Text")).GetComponent<UILabel>();
			component2.spriteName = IconMap.Get(menus[i]);
			component3.text = menus[i].GetName();
		}
		_tabsScroll.ResetPosition();
	}

	public void SelectMenu(ClanMenus menu)
	{
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_menus); i < size; i++)
		{
			if (_menus[i] == menu)
			{
				num = i;
				break;
			}
		}
		ListObjectPool nodes = _tabsScroll.Nodes;
		for (int j = 0; j < nodes.Count; j++)
		{
			Selectable component = nodes[j].GetComponent<Selectable>();
			component.Select = j == num;
		}
		if (this.Selected != null)
		{
			this.Selected(menu);
		}
	}
}

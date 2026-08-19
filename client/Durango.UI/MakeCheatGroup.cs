using Durango.UI.Control;
using Durango.Utils;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class MakeCheatGroup : UIBase
{
	public enum Tab
	{
		Build,
		Item,
		Gathering,
		Animal,
		Market
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[EnumList(typeof(Tab), false, 0, -1)]
	[SerializeField]
	private GameObject[] _pages;

	private HorizontalTabList _tabList;

	private void Awake()
	{
		_tabList = _tabLinker.Object.GetComponent<HorizontalTabList>();
		_tabList.Clicked += SelectTab;
		_tabList.BeginLoad();
		Tab[] array = Enums<Tab>.All();
		foreach (Tab tab in array)
		{
			_tabList.AddText(GetTabText(tab));
		}
		_tabList.EndLoadByFixedSize(200);
		SelectTab(0);
		SetChildrenActive(activated: false);
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle("Cheat");
	}

	private string GetTabText(Tab tab)
	{
		return tab switch
		{
			Tab.Build => "건설", 
			Tab.Item => "아이템", 
			Tab.Gathering => "채집물", 
			Tab.Animal => "동물", 
			Tab.Market => "장터", 
			_ => tab.ToString(), 
		};
	}

	public void OpenTab(Tab tab)
	{
		SelectTab((int)tab);
		Open();
	}

	private void SelectTab(int index)
	{
		_tabList.Select(index);
		Tab[] array = Enums<Tab>.All();
		for (int i = 0; i < array.Length; i++)
		{
			_pages[i].SetActive(i == index);
		}
	}
}

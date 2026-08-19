using System;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class EstateTabWidget : UIWidget
{
	public enum Menu
	{
		[EnumIcon("icon_island_private")]
		[T.EnumName("개인섬")]
		Personal,
		[EnumIcon("icon_island_civilized")]
		[T.EnumName("도시섬")]
		Urban,
		[EnumIcon("icon_clan_createclan")]
		[T.EnumName("부족")]
		Clan,
		[EnumIcon("estate_base_icon_s")]
		[T.EnumName("거점")]
		Base
	}

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	private IconTabList _tabList;

	public event Action<Menu> TabClicked;

	protected override void Awake()
	{
		base.Awake();
		if (!Application.isPlaying)
		{
			return;
		}
		_tabList = _tabLinker.Object.GetComponent<IconTabList>();
		Menu[] array = Enums<Menu>.All();
		_tabList.BeginLoad();
		Menu[] array2 = array;
		foreach (Menu menu in array2)
		{
			if (menu != Menu.Base)
			{
				_tabList.Add(menu.GetIcon(), menu.GetName());
			}
		}
		_tabList.EndLoad();
		_tabList.Clicked += OnTabSelected;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			Refresh();
		}
	}

	private void OnTabSelected(int index)
	{
		if (this.TabClicked != null)
		{
			this.TabClicked((Menu)index);
		}
	}

	private void Refresh()
	{
		Menu[] array = Enums<Menu>.All();
		foreach (Menu menu in array)
		{
			if (menu != Menu.Base)
			{
				IconTabWidget iconTabWidget = _tabList.Get((int)menu);
				if (menu != 0)
				{
					iconTabWidget.gameObject.SetActive(GameManager.Region.IsAfterSafeHouse());
				}
				if (menu == Menu.Base)
				{
					iconTabWidget.Disabled = !PlayerBehavior.LocalPlayer.HasClan;
				}
			}
		}
	}

	public void SelectTab(Menu menu)
	{
		_tabList.Select((int)menu);
	}
}

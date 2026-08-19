using MenuData;
using UnityEngine;

public class BottomMenuList : MonoBehaviour
{
	[SerializeField]
	private MenuType[] _listMenus;

	[SerializeField]
	private KScrollView _menuList;

	private void Start()
	{
		InitMenuList();
	}

	private void OnEnable()
	{
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated += RefreshMenuList;
		RefreshMenuList();
	}

	private void OnDisable()
	{
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated -= RefreshMenuList;
	}

	private void InitMenuList()
	{
		_menuList.Nodes.Clear();
		int i = 0;
		for (int num = _listMenus.Length; i < num; i++)
		{
			MenuType type = _listMenus[i];
			if (MenuSystem.IsMenuAvailable(type))
			{
				MenuListControl item = ((ListObjectPoolBase<GameObject>)_menuList.Nodes).Add<MenuListControl>();
				SetMenuItem(item, type);
			}
		}
		RefreshMenuList(init: true);
	}

	private void SetMenuItem(MenuListControl item, MenuType type)
	{
		UIBase script = MenuSystem.GetScript(type);
		item.Disable = (Object)(object)script == (Object)null;
		item.Type = type;
		item.Clicked = OnClickMenuButton;
		item.MenuIcon = MenuUtil.GetIcon(type);
		item.MenuLabel = MenuUtil.GetName(type);
		((Object)((Component)item).gameObject).name = type.ToString();
	}

	private void OnClickMenuButton()
	{
		MenuListControl menuListControl = Selectable.Current as MenuListControl;
		if (!((Object)(object)menuListControl == (Object)null))
		{
			UIBase script = MenuSystem.GetScript(menuListControl.Type);
			if (!((Object)(object)script == (Object)null) && !script.IsOpen)
			{
				float currentOffset = _menuList.CurrentOffset;
				UIBase.CloseAllUI();
				bool softOpen = script.SoftOpen;
				script.SoftOpen = false;
				script.Open();
				script.SoftOpen = softOpen;
				_menuList.MoveTo(currentOffset, instant: true);
			}
		}
	}

	private void RefreshMenuList()
	{
		RefreshMenuList(init: false);
	}

	private void RefreshMenuList(bool init)
	{
		ListObjectPool nodes = _menuList.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			MenuListControl component = nodes[i].GetComponent<MenuListControl>();
			if (!GameSystem<MenuSystem>.Instance().IsEnabled(component.Type))
			{
				((Component)component).gameObject.SetActive(false);
			}
			else
			{
				((Component)component).gameObject.SetActive(true);
			}
		}
		_menuList.Reposition(init, !init);
	}
}

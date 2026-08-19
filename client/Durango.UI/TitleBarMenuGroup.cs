using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.InputSystem;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TitleBarMenuGroup : UIBase
{
	[SerializeField]
	private UIWidget _menuContainer;

	[SerializeField]
	private KScrollView _menuList;

	[SerializeField]
	private UIWidget _prevButton;

	private MenuType _prevMenu;

	private MenuType _nextMenu;

	public Transform TitleBarRightAnchor => (!(_prevButton != null)) ? null : _prevButton.transform;

	private void Start()
	{
		InitMenuList();
		UIBase.UIOpened += RefreshTitleBarMenuList;
		UIBase.UIClosed += RefreshTitleBarMenuList;
		base.OnOpenSucceed += RefreshMenuList;
		SetChildrenActive(activated: false);
	}

	private void OnEnable()
	{
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated += RefreshMenuList;
		RefreshMenuList();
		GameSystem<InputSystem>.Instance().On(InputCommand.PrevUIGroup, OnPrevUIGroup);
		GameSystem<InputSystem>.Instance().On(InputCommand.NextUIGroup, OnNextUIGroup);
	}

	private void OnDisable()
	{
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated -= RefreshMenuList;
		GameSystem<InputSystem>.Instance().Off(InputCommand.PrevUIGroup, OnPrevUIGroup);
		GameSystem<InputSystem>.Instance().Off(InputCommand.NextUIGroup, OnNextUIGroup);
	}

	private void OnPrevUIGroup(InputCommandMessage message)
	{
		if (message.CurrentTrigger == Trigger.Up && IsOpenableUI(UIBase.CurrentUI))
		{
			OpenMenu(_prevMenu);
		}
	}

	private void OnNextUIGroup(InputCommandMessage message)
	{
		if (message.CurrentTrigger == Trigger.Up && IsOpenableUI(UIBase.CurrentUI))
		{
			OpenMenu(_nextMenu);
		}
	}

	private void OpenMenu(MenuType menu)
	{
		UIBase script = MenuHelper.GetScript(menu);
		if (!(script == null) && !script.IsOpened)
		{
			UIBase.CloseAllUI();
			MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
			menuListGroupBase.SetLastOpenUI(IconMap.Get(menu), script);
			MenuHelper.Open(menu, immediately: true);
		}
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		RefreshTitleBarMenuList();
	}

	private void RefreshTitleBarMenuList()
	{
		UIBase currentUI = UIBase.CurrentUI;
		if (currentUI != null && currentUI.Anchor == AnchorType.Fullscreen && IsOpenableUI(currentUI))
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	private bool IsOpenableUI(UIBase ui)
	{
		if (ui == null)
		{
			return false;
		}
		for (int i = 0; i < _menuList.Nodes.Count; i++)
		{
			MenuWidget menuWidget = _menuList.Nodes.Get<MenuWidget>(i);
			// [แก้เอง] เดิมเรียก .gameObject ต่อท้ายเลยโดยไม่เช็ค null
			//
			// เมนูที่ไม่มีสคริปต์ในบิลด์นี้ (ระบบที่ยังไม่เปิด / asset โหลดไม่ขึ้น) ทำให้ GetScript คืน null
			// ⇒ NullReferenceException ตรงนี้ ⇒ **UIBase.Close() ที่เรียกมาทั้งสายพังกลางทาง**
			// ⇒ อาการที่เห็นในเกมคือ **กดปุ่มกากบาทปิดหน้าต่างแล้วไม่มีอะไรเกิดขึ้น**
			//
			// InitMenuList ในไฟล์เดียวกันกรอง script == null อยู่แล้ว ตรงนี้แค่ลืมกรองตาม
			UIBase script = MenuHelper.GetScript(menuWidget.Type);
			if (script == null)
			{
				continue;
			}
			if (script.gameObject == ui.gameObject)
			{
				return true;
			}
		}
		return false;
	}

	private void InitMenuList()
	{
		_menuList.Nodes.Clear();
		List<MenuType> list = MenuContainer.Menus.Where(delegate(MenuType menuType)
		{
			UIBase script = MenuHelper.GetScript(menuType);
			return !(script == null) && script.Anchor != AnchorType.FullscreenMobileOnly;
		}).ToList();
		foreach (MenuType item in list)
		{
			MenuWidget menuWidget = _menuList.Nodes.Add<MenuWidget>();
			menuWidget.Set(item);
			menuWidget.GetComponent<HoverShortcutViewer>().Set(item);
			menuWidget.Clicked = OnClickMenuButton;
			if (Application.isEditor)
			{
				menuWidget.gameObject.name = item.ToString();
			}
		}
		RefreshMenuList(init: true);
	}

	private void OnClickMenuButton()
	{
		MenuWidget menuWidget = Selectable.Current as MenuWidget;
		if (!(menuWidget == null))
		{
			OpenMenu(menuWidget.Type);
		}
	}

	private void RefreshMenuList()
	{
		RefreshMenuList(init: false);
	}

	private void RefreshMenuList(bool init)
	{
		ListObjectPool nodes = _menuList.Nodes;
		if (nodes.Count == 0)
		{
			return;
		}
		int num = -1;
		List<int> list = new List<int>();
		for (int i = 0; i < nodes.Count; i++)
		{
			MenuWidget component = nodes[i].GetComponent<MenuWidget>();
			if (!GameSystem<MenuSystem>.Instance().IsEnabled(component.Type))
			{
				component.gameObject.SetActive(value: false);
				continue;
			}
			UIBase script = MenuHelper.GetScript(component.Type);
			if (script == null)
			{
				component.gameObject.SetActive(value: false);
				continue;
			}
			list.Add(i);
			if (script == UIBase.CurrentUI)
			{
				num = list.Count - 1;
			}
			component.Selected = script == UIBase.CurrentUI;
			component.gameObject.SetActive(value: true);
		}
		if (num >= 0)
		{
			int index = list[(num + list.Count - 1) % list.Count];
			_prevMenu = nodes[index].GetComponent<MenuWidget>().Type;
			int index2 = list[(num + 1) % list.Count];
			_nextMenu = nodes[index2].GetComponent<MenuWidget>().Type;
		}
		if (UIBase.CurrentUI != null)
		{
			UIPanel uIPanel = base.Rect as UIPanel;
			UIPanel uIPanel2 = UIBase.CurrentUI.Rect as UIPanel;
			if (uIPanel != null && uIPanel2 != null)
			{
				UIPanel panel = _menuList.Panel;
				uIPanel.depth = uIPanel2.depth + 1;
				panel.depth = uIPanel.depth + 1;
			}
		}
		_menuList.UpdateLayout();
		float width = _menuContainer.GetWidth();
		float a = Mathf.Max(40f, _menuList.ContentsLength + _menuList.Panel.clipSoftness.x * 2f);
		float num2 = Mathf.Min(a, width);
		float num3 = (width - num2) * 0.5f;
		_menuList.Panel.leftAnchor.Set(0f, num3);
		_menuList.Panel.rightAnchor.Set(1f, 0f - num3);
		UIUtility.UpdateAnchors(_menuList.transform);
		if (init)
		{
			_menuList.Reposition(resetPosition: true, tween: false);
		}
		else
		{
			_menuList.MoveToVisibleArea(num, instant: false);
		}
	}
}

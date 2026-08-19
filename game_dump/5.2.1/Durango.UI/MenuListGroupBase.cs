using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Notification;
using Durango.Offline;
using Durango.System;
using Durango.System.Config;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

[Uri("Menu")]
public class MenuListGroupBase : UIBase
{
	private class TryKnockLoaclNetwork
	{
		private UdpClient _udp;

		private GenericSelector _selector;

		private readonly List<IPAddress> _addresses = new List<IPAddress>();

		~TryKnockLoaclNetwork()
		{
			End();
		}

		public void Start()
		{
			if (_udp == null)
			{
				_selector = UIManager.Popup.Tooltip<GenericSelector>();
				_selector.ResetArguments();
				_selector.SetTitle(T._("접속할 섬의 ip를 입력하세요."));
				_selector.AddItem(T._("직접 입력"));
				_addresses.Add(null);
				_selector.AddOnFinished(End);
				_selector.SetSelected(OnSelectItem);
				_selector.Show();
				_udp = new UdpClient();
				_udp.EnableBroadcast = true;
				byte[] bytes = Encoding.UTF8.GetBytes("Knock:" + GameManager.PlayerId);
				_udp.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, 8191));
				_udp.BeginReceive(KnockUdpCallback, null);
			}
		}

		private void OnSelectItem(int index)
		{
			if (index >= 0 && index < _addresses.Count)
			{
				IPAddress iPAddress = _addresses[index];
				End();
				if (iPAddress == null)
				{
					ShowConnectIpInput();
				}
				else
				{
					ConfirmConnectTo(iPAddress.ToString());
				}
			}
		}

		private void End()
		{
			if (_udp != null)
			{
				_udp.Close();
				_udp = null;
			}
		}

		private void KnockUdpCallback(IAsyncResult ar)
		{
			if (_udp != null)
			{
				IPEndPoint remoteEP = null;
				byte[] bytes = _udp.EndReceive(ar, ref remoteEP);
				IPAddress address = remoteEP.Address;
				if (_addresses.FindIndex((IPAddress a) => a?.Equals(address) ?? false) == -1)
				{
					_addresses.Add(remoteEP.Address);
					_selector.AddItem($"<em>{Encoding.UTF8.GetString(bytes)}</em> [size=*0.8]<weak>{remoteEP.Address}</weak>[/size]");
					_selector.MarkAsChanged();
				}
				_udp.BeginReceive(KnockUdpCallback, null);
			}
		}

		private void ShowConnectIpInput()
		{
			UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string ip)
			{
				Preferences.SetString("last_connect_ip", ip);
				ConfirmConnectTo(ip);
			}, T._("접속할 섬의 ip를 입력하세요."), Preferences.GetString("last_connect_ip", string.Empty));
		}

		private static void ConfirmConnectTo(string ip)
		{
			if (string.IsNullOrEmpty(ip))
			{
				return;
			}
			UIManager.MessageBox.Show(T._("<em>{0}</em> 섬으로 이동합니다.", ip), T._("<alert_icon/> 친구 섬에서 획득한 아이템은 내 섬으로 돌아오면 사라집니다.\n<alert_icon/> 친구 섬에서 만든 건축물은 친구 섬에 저장됩니다."), delegate(bool ok)
			{
				if (ok)
				{
					Server.ConnectTo(ip);
				}
			});
		}
	}

	private struct AddedItem
	{
		public ItemData Item;

		public float DisplayAt;
	}

	private enum LockState
	{
		None,
		Lock,
		Unlock
	}

	protected enum MenuLayout
	{
		Landscape,
		Portrait,
		Locked
	}

	public const string MenuLockKey = "menu_lock";

	public const string MenuAnchorKey = "Menu";

	[SerializeField]
	private LandscapeMenuListBase _landscapeMenuList;

	[SerializeField]
	private PortraitMenuList _portraitMenuList;

	[SerializeField]
	private MenuBannerList _bannerList;

	[SerializeField]
	private LockedMenuList _lockedMenuList;

	[SerializeField]
	protected GameObject _menuBtn;

	[SerializeField]
	private UISprite _newIcon;

	[SerializeField]
	private Transform _lastActionButtons;

	[SerializeField]
	private UIWidget _lastOpenUI;

	[SerializeField]
	private UISprite _lastOpenUISprite;

	[SerializeField]
	private UIWidget _lastAddedItemWidget;

	[SerializeField]
	private ItemIconTex _lastAddedItemIcon;

	[SerializeField]
	protected UIWidget TouchBlockBox;

	private RecipeSystem.RecipeType _lastOpenCraftType;

	private string _lastOpenCraftId;

	private UIBase _lastOpenUILink;

	private string _lastOpenUriLink;

	private string _lastAddedItemId;

	private Vector3 _lastActionButtonsPos;

	private readonly Container _notification = new Container();

	private LockState _lockState;

	private MenuLayout _menuLayout;

	private readonly Queue<AddedItem> _lastAddedItemQueue = new Queue<AddedItem>();

	private bool _prevOpened;

	protected override bool IsSoundOcclusion => false;

	public event Action<MenuType> MenuClicked;

	public event Action<MenuType> MenuOpened;

	public bool IsMenuVisible()
	{
		if (!base.IsOpened)
		{
			return IsMenuLocked();
		}
		return true;
	}

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.LeftMenu;
		ClearLastOpenUI();
		ClearLastCollectItem();
		_newIcon.gameObject.SetActive(value: false);
		_notification.Changed += OnUpdateNotification;
	}

	protected virtual void Start()
	{
		UIBase.UIOpened += ClosableUIOpened;
		UIBase.UIClosed += ClosableUIClosed;
		base.OnOpenSucceed += UpdateMenuBtnVisibleState;
		base.OnCloseSucceed += UpdateMenuBtnVisibleState;
		UIManager.FindScript<CombatGroup>().BattleViewChanged += delegate(CombatGroup.BattleViewMode view)
		{
			bool flag = view != CombatGroup.BattleViewMode.Normal;
			if (flag)
			{
				Close();
			}
			if (IsMenuLocked())
			{
				if (flag)
				{
					UIRootAnchor.Set("Menu", AnchorType.Base, null, null, null, null);
				}
				else
				{
					UIRootAnchor.Set("Menu", AnchorType.Base, _lockedMenuList.width, null, null, null);
				}
			}
		};
		UIEventListener.Get(_lastOpenUI.gameObject).onClick = OnClickLastOpenUI;
		UIEventListener.Get(_lastOpenUI.gameObject).onDrag = UIManager.IgnoreUIDrag;
		UIEventListener.Get(_lastAddedItemWidget.gameObject).onClick = OnClickLastGatheringItem;
		UIEventListener.Get(_lastAddedItemWidget.gameObject).onDrag = UIManager.IgnoreUIDrag;
		UIEventListener uIEventListener = UIEventListener.Get(_menuBtn);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Open();
		});
		UIManager.Inventory.OnOpenSucceed += ClearLastCollectItem;
		_landscapeMenuList.MenuClicked += OnMenuClick;
		_portraitMenuList.MenuClicked += OnMenuClick;
		if (_bannerList != null)
		{
			_bannerList.MenuClicked += OnMenuClick;
		}
		_lockedMenuList.MenuClicked += OnMenuClick;
		_landscapeMenuList.LockClicked += ToggleLockMode;
		_lockedMenuList.LockClicked += ToggleLockMode;
		_lastActionButtonsPos = _lastActionButtons.transform.localPosition;
		RefreshMenuLayout();
		UpdateMenuBtnVisibleState();
		base.TryOpen();
		TouchBlockBox.gameObject.SetActive(value: false);
		_landscapeMenuList.gameObject.SetActive(value: false);
		_portraitMenuList.gameObject.SetActive(value: false);
		if (_bannerList != null)
		{
			_bannerList.gameObject.SetActive(value: false);
		}
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated += MenuSystem_EnableMenuUpdated;
		GameSystem<InventorySystem>.Instance().ItemAdded += OnItemAdded;
		GameSystem<InputSystem>.Instance().On(InputCommand.ShowLastAddedItem, OnReceivedInputCommandMessage);
		GameSystem<InputSystem>.Instance().On(InputCommand.RepeatLastMenu, OnReceivedInputCommandMessage);
		RefreshMenuList();
	}

	private void Update()
	{
		CheckLastAddedItemQueue();
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		RefreshMenuLayout();
		UpdateMenuBtnVisibleState();
	}

	private void SaveMenuLockState()
	{
		LockState lockState = _lockState;
		if (lockState == LockState.Lock || lockState == LockState.Unlock)
		{
			Preferences.SetBool("menu_lock", _lockState == LockState.Lock);
		}
	}

	protected virtual bool GetMenuLockState()
	{
		if (_lockState == LockState.None)
		{
			_lockState = (Preferences.GetBool("menu_lock") ? LockState.Lock : LockState.Unlock);
		}
		return _lockState == LockState.Lock;
	}

	public bool IsMenuLocked()
	{
		return _menuLayout == MenuLayout.Locked;
	}

	private bool IsMenuVisible(MenuType type)
	{
		return GetMenuItem(type) != null;
	}

	private MenuWidget GetMenuItem(MenuType type)
	{
		MenuWidget comp = null;
		switch (_menuLayout)
		{
		case MenuLayout.Landscape:
			_landscapeMenuList.TryGetMenuItem(type, out comp);
			return comp;
		case MenuLayout.Portrait:
			_portraitMenuList.TryGetMenuItem(type, out comp);
			return comp;
		case MenuLayout.Locked:
			_lockedMenuList.TryGetMenuItem(type, out comp);
			return comp;
		default:
			return comp;
		}
	}

	public Transform GetBottomLeftMenuTransform()
	{
		return _menuBtn.transform;
	}

	public Transform GetMenuTransform(MenuType type)
	{
		MenuWidget menuItem = GetMenuItem(type);
		if (menuItem == null)
		{
			return null;
		}
		return menuItem.transform;
	}

	private void RefreshMenuList()
	{
		_landscapeMenuList.Refresh();
		_portraitMenuList.Refresh();
		if (_bannerList != null)
		{
			_bannerList.Refresh();
		}
		_lockedMenuList.Refresh();
		_notification.BeginSetting();
		_notification.ClearChild();
		MenuType[] array = Enums<MenuType>.All();
		foreach (MenuType type in array)
		{
			if (IsMenuVisible(type))
			{
				INotificationable notificationable = MenuHelper.GetNotificationable(type);
				if (notificationable != null)
				{
					_notification.AddChild(notificationable);
				}
			}
		}
		_notification.EndSetting();
		OnUpdateNotification();
	}

	private void MenuSystem_EnableMenuUpdated()
	{
		MenuHelper.RefreshCategoryMenuNotification();
		RefreshMenuList();
	}

	protected virtual void OnMenuClick(MenuType type)
	{
		PlayCloseSound = false;
		if (this.MenuClicked != null)
		{
			this.MenuClicked(type);
		}
		GameSystem<MenuSystem>.Instance().SetRecentlyUnlocked(type, on: false);
		switch (type)
		{
		case MenuType.Notice:
			GameSystem<NoticeSystem>.Instance().Show();
			return;
		case MenuType.OfficialCommunity:
			ConfigInstance.OpenOfficialCommunityUrl();
			return;
		case MenuType.Offerwall:
			Platform.Instance.ShowOfferwall();
			return;
		case MenuType.MoveToTitle:
			Singleton<GameManager>.Instance().MoveToTitle();
			return;
		case MenuType.Connect:
			new TryKnockLoaclNetwork().Start();
			return;
		}
		UIBase script = MenuHelper.GetScript(type);
		if (!(script == null))
		{
			SetLastOpenUI(IconMap.Get(type), script);
			MenuHelper.Open(type);
		}
	}

	public void NotifyMenuOpened(MenuType type)
	{
		if (this.MenuOpened != null)
		{
			this.MenuOpened(type);
		}
	}

	private void OnClickLastOpenUI(GameObject go)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		switch (_lastOpenCraftType)
		{
		case RecipeSystem.RecipeType.None:
			if (string.IsNullOrEmpty(_lastOpenUriLink))
			{
				if (_lastOpenUILink != null)
				{
					_lastOpenUILink.Open();
				}
			}
			else
			{
				Singleton<UIManager>.Instance().OpenUri(_lastOpenUriLink);
			}
			break;
		case RecipeSystem.RecipeType.Crafting:
		case RecipeSystem.RecipeType.Building:
			UIManager.FindScript<RecipeSelectorGroup>().QuickOpenCraftingUI(_lastOpenCraftType, _lastOpenCraftId);
			break;
		}
	}

	private void OnClickLastGatheringItem(GameObject obj)
	{
		if (!string.IsNullOrEmpty(_lastAddedItemId))
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			UIManager.Inventory.OpenAndSelectItem(_lastAddedItemId);
		}
	}

	private void OnReceivedInputCommandMessage(InputCommandMessage message)
	{
		if (message.Command == InputCommand.ShowLastAddedItem)
		{
			OnClickLastGatheringItem(null);
		}
		else if (message.Command == InputCommand.RepeatLastMenu)
		{
			OnClickLastOpenUI(null);
		}
	}

	private void ClearLastCollectItem()
	{
		_lastAddedItemWidget.GetComponent<TweenerPlayer>().Stop();
		_lastAddedItemWidget.alpha = 0f;
		_lastAddedItemQueue.Clear();
	}

	protected virtual void CheckLastAddedItemQueue()
	{
		if (_lastAddedItemQueue.Count != 0)
		{
			float time = Time.time;
			AddedItem addedItem = default(AddedItem);
			while (_lastAddedItemQueue.Count > 0 && !(time < _lastAddedItemQueue.Peek().DisplayAt))
			{
				addedItem = _lastAddedItemQueue.Dequeue();
			}
			if (addedItem.Item != null)
			{
				ItemData item = addedItem.Item;
				_lastAddedItemId = item.Id;
				_lastAddedItemIcon.SetIcon(item);
				_lastAddedItemWidget.GetComponent<TweenerPlayer>().Play();
				RefreshLastButtonsLayout();
			}
		}
	}

	public virtual void RefreshLastButtonsLayout()
	{
	}

	private void OnItemAdded(ItemData item)
	{
		_lastAddedItemQueue.Enqueue(new AddedItem
		{
			Item = item,
			DisplayAt = Time.time + 1.5f
		});
	}

	private void ClearLastOpenUI()
	{
		_lastOpenCraftType = RecipeSystem.RecipeType.None;
		_lastOpenCraftId = string.Empty;
		_lastOpenUILink = null;
		_lastOpenUI.alpha = 0f;
	}

	public void SetLastOpenUI(string icon, UIBase link)
	{
		_lastOpenCraftType = RecipeSystem.RecipeType.None;
		_lastOpenCraftId = string.Empty;
		_lastOpenUILink = link;
		_lastOpenUriLink = null;
		_lastOpenUISprite.spriteName = icon;
		_lastOpenUI.alpha = 1f;
		RefreshLastButtonsLayout();
	}

	public void SetLastOpenCraft(string icon, RecipeSystem.RecipeType type, string id)
	{
		_lastOpenCraftType = type;
		_lastOpenCraftId = id;
		_lastOpenUILink = null;
		_lastOpenUriLink = null;
		_lastOpenUISprite.spriteName = icon;
		_lastOpenUI.alpha = 1f;
		RefreshLastButtonsLayout();
	}

	public void SetLastOpenUri(string icon, string uri)
	{
		_lastOpenCraftType = RecipeSystem.RecipeType.None;
		_lastOpenCraftId = string.Empty;
		_lastOpenUILink = null;
		_lastOpenUriLink = uri;
		_lastOpenUISprite.spriteName = icon;
		_lastOpenUI.alpha = 1f;
		RefreshLastButtonsLayout();
	}

	private void OnUpdateNotification()
	{
		if (GameSystem<MenuSystem>.Instance().GetRecentlyUnlockedMenus().Any(IsMenuVisible))
		{
			_newIcon.gameObject.SetActive(value: true);
			_newIcon.color = Notification.GetTypeColor(Durango.Logic.Notification.Type.Important);
		}
		else if (_notification.On)
		{
			_newIcon.gameObject.SetActive(value: true);
			_newIcon.color = Notification.GetTypeColor(_notification.Type);
		}
		else
		{
			_newIcon.gameObject.SetActive(value: false);
		}
	}

	protected override bool TryOpen()
	{
		bool prevOpened = _prevOpened;
		switch (_menuLayout)
		{
		case MenuLayout.Landscape:
			_landscapeMenuList.Show(prevOpened);
			if (_bannerList != null)
			{
				_bannerList.Show(prevOpened, isPortrait: false);
			}
			break;
		case MenuLayout.Portrait:
			_portraitMenuList.Show(prevOpened);
			if (_bannerList != null)
			{
				_bannerList.Show(prevOpened, isPortrait: true);
			}
			break;
		case MenuLayout.Locked:
			return false;
		}
		TouchBlockBox.gameObject.SetActive(value: true);
		VisibleController.Hide(HideUIFunc, hide: true, "LeftMenu");
		return true;
	}

	protected override bool TryClose()
	{
		TouchBlockBox.gameObject.SetActive(value: false);
		_landscapeMenuList.Hide();
		_portraitMenuList.Hide();
		if (_bannerList != null)
		{
			_bannerList.Hide();
		}
		VisibleController.Hide(HideUIFunc, hide: false, "LeftMenu", 0.1f);
		return true;
	}

	protected virtual bool HideUIFunc(VisibleController script)
	{
		if (script != base.VisibleController && (script.Flag & VisibleType.Base) != 0)
		{
			if (Platform.Instance.UsePCUI)
			{
				return (script.Flag & VisibleType.HideOnLeftMenu) != 0;
			}
			return true;
		}
		return false;
	}

	private void ClosableUIOpened()
	{
		_prevOpened = base.IsOpened;
		Close();
		UpdateMenuBtnVisibleState();
	}

	private void ClosableUIClosed()
	{
		UpdateMenuBtnVisibleState();
		if (!UIBase.HasOpenedUI)
		{
			if (_prevOpened)
			{
				Open();
			}
			_prevOpened = false;
		}
	}

	protected virtual bool IsButtonVisible()
	{
		if (!base.IsOpened && !UIBase.HasOpenedFullscreenUI)
		{
			return !IsMenuLocked();
		}
		return false;
	}

	private void UpdateMenuBtnVisibleState()
	{
		bool active = !base.IsOpened && !UIBase.HasOpenedUI;
		bool active2 = IsButtonVisible();
		_menuBtn.gameObject.SetActive(active2);
		_lastOpenUI.gameObject.SetActive(active);
		_lastAddedItemWidget.gameObject.SetActive(active);
		_lastActionButtons.transform.localPosition = _lastActionButtonsPos + Vector3.right * ((!IsMenuLocked()) ? 0f : ((float)_lockedMenuList.width));
		RefreshLastButtonsLayout();
	}

	private void ToggleLockMode()
	{
		bool menuLockState = GetMenuLockState();
		_lockState = ((!menuLockState) ? LockState.Lock : LockState.Unlock);
		SaveMenuLockState();
		RefreshMenuLayout();
		if (!IsMenuLocked())
		{
			Open();
		}
	}

	private void RefreshMenuLayout()
	{
		SetMenuLayout(base.IsPortrait ? MenuLayout.Portrait : (GetMenuLockState() ? MenuLayout.Locked : MenuLayout.Landscape));
	}

	protected virtual void SetMenuLayout(MenuLayout layout)
	{
		bool flag = IsMenuLocked();
		Close();
		_menuLayout = layout;
		switch (_menuLayout)
		{
		case MenuLayout.Landscape:
		case MenuLayout.Portrait:
			_landscapeMenuList.Hide();
			_portraitMenuList.Hide();
			if (_bannerList != null)
			{
				_bannerList.Hide();
			}
			_lockedMenuList.Hide();
			break;
		case MenuLayout.Locked:
			_landscapeMenuList.Hide();
			_portraitMenuList.Hide();
			if (_bannerList != null)
			{
				_bannerList.Hide();
			}
			_lockedMenuList.Show(instant: false);
			break;
		}
		bool flag2 = IsMenuLocked();
		if (flag != flag2)
		{
			UpdateMenuBtnVisibleState();
			if (flag2)
			{
				UIRootAnchor.Set("Menu", AnchorType.Base, _lockedMenuList.width, null, null, null);
			}
			else
			{
				UIRootAnchor.Set("Menu", AnchorType.Base, null, null, null, null);
			}
		}
	}

	public void SetAirballoonMode(bool mode)
	{
		if (_menuLayout == MenuLayout.Locked)
		{
			int? left = ((!mode) ? new int?(_lockedMenuList.width) : null);
			UIRootAnchor.Set("Menu", AnchorType.Base, left, null, null, null);
		}
	}
}

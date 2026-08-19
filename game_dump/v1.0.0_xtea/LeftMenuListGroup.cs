using System;
using System.Collections.Generic;
using ItemSystem;
using JetBrains.Annotations;
using MenuData;
using UnityEngine;

public class LeftMenuListGroup : UIBase
{
	private enum LockState
	{
		None,
		Lock,
		Unlock
	}

	[Serializable]
	private struct ShowAninationParam
	{
		public float Duration;

		public float Delay;

		public Vector3 Offset;

		public AnimationCurve AlphaCurve;

		public AnimationCurve PositionCurve;
	}

	public const string MenuLockKey = "menu_lock";

	[SerializeField]
	private MenuType[] _listMenus;

	[SerializeField]
	private MenuType[] _bottomBoxMenus;

	[SerializeField]
	private MenuType _bottomMenu;

	[SerializeField]
	private int _menuMinimumWidth;

	[SerializeField]
	private int _lockMenuWidth;

	[SerializeField]
	private ShowAninationParam _showAnimationParam;

	[SerializeField]
	private AnimationWidget _menuWidget;

	[SerializeField]
	private GameObject _menuBg;

	[SerializeField]
	private GameObject _lockMenuBg;

	[SerializeField]
	private ListObjectPool _menuList;

	[SerializeField]
	private ListObjectPool _boxMenuList;

	[SerializeField]
	private MenuListControl _bottomMenuItem;

	[SerializeField]
	private MenuListControl _lockBottomMenuItem;

	[SerializeField]
	private ListObjectPool _menuSplitLine;

	[SerializeField]
	private ListObjectPool _boxSplitLine;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private Selectable[] _lockButtons;

	[SerializeField]
	private UIWidget _bottomWidget;

	[SerializeField]
	private UIWidget _lockBottomWidget;

	[SerializeField]
	private GameObject _menuBtn;

	[SerializeField]
	private GameObject _newIcon;

	[SerializeField]
	private UILabel _newCount;

	[SerializeField]
	private float _menuHorizontalMargin;

	[SerializeField]
	private Transform _lastActionButtons;

	[SerializeField]
	private UIWidget _lastOpenUI;

	[SerializeField]
	private UISprite _lastOpenUISprite;

	[SerializeField]
	private UIWidget _lastGatheringItem;

	[SerializeField]
	private ItemIconTex _lastGatheringIcon;

	[SerializeField]
	private GameObject _touchBlockBox;

	private int _listMenuCount;

	private RecipeSystem.RecipeType _lastOpenCraftType;

	private string _lastOpenCraftId;

	private UIBase _lastOpenUILink;

	private ItemData _lastGatheringItemData;

	private Vector3 _lastActionButtonsPos;

	private readonly NewChecker _newChecker = new NewCheckerContainer();

	private bool _locked;

	private LockState _lockState;

	private UIWidget _scrollViewBox;

	public event Action<MenuType> MenuClicked;

	public bool IsMenuVisible()
	{
		return base.IsOpen || _locked;
	}

	private void Awake()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Main_Open_01.wav", "Sound/Effect/UI/UI_Menu_Main_Close_01.wav");
		ClearLastOpenUI();
		ClearLastCollectItem();
		_newIcon.SetActive(false);
		_newChecker.RegisterCallback(OnUpdateNewChecker);
	}

	private void Start()
	{
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		UIEventListener uIEventListener = UIEventListener.Get(_touchBlockBox);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressTouchBlockBox));
		UIBase.OnCloseCloseableUI += UpdateMenuBtnVisibleState;
		UIBase.OnOpenCloseableUI += delegate
		{
			Close();
			UpdateMenuBtnVisibleState();
		};
		base.OnOpenSucceed += UpdateMenuBtnVisibleState;
		base.OnCloseSucceed += UpdateMenuBtnVisibleState;
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += delegate(bool combat)
		{
			if (combat)
			{
				Close();
			}
			if (_locked)
			{
				KSingleton<UIManager>.Instance().SetRootAnchor(AnchorType.Base, (!combat) ? _lockMenuWidth : 0, 0, 0, 0);
			}
		};
		UIEventListener.Get(((Component)_lastOpenUI).gameObject).onClick = OnClickLastOpenUI;
		UIEventListener.Get(((Component)_lastOpenUI).gameObject).onDrag = UIManager.IgnoreUIDrag;
		UIEventListener.Get(((Component)_lastGatheringItem).gameObject).onClick = OnClickLastGatheringItem;
		UIEventListener.Get(((Component)_lastGatheringItem).gameObject).onDrag = UIManager.IgnoreUIDrag;
		InitMenuList();
		UIEventListener uIEventListener2 = UIEventListener.Get(_menuBtn);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Open();
		});
		UIManager.FindScript<InventoryGroup>().OnOpenSucceed += ClearLastCollectItem;
		UIExtendEventListener uIExtendEventListener = UIExtendEventListener.Get(((Component)_scrollView).gameObject);
		uIExtendEventListener.onEnable = (UIEventListener.VoidDelegate)Delegate.Combine(uIExtendEventListener.onEnable, (UIEventListener.VoidDelegate)delegate
		{
			_scrollViewBox = UIUtility.SetScrollViewInvisibleBox(_scrollView, _scrollViewBox);
		});
		int i = 0;
		for (int num = _lockButtons.Length; i < num; i++)
		{
			_lockButtons[i].Clicked = ToggleLockMode;
		}
		_lastActionButtonsPos = ((Component)_lastActionButtons).transform.localPosition;
		((Component)_menuWidget).gameObject.SetActive(false);
		_menuWidget.Alpha = 0f;
		SetLockMode(GetLockState());
		UpdateMenuBtnVisibleState();
		base.OnOpen();
		_touchBlockBox.gameObject.SetActive(false);
		OnUpdateNewChecker();
		GameSystem<InventorySystem>.Instance().OnCollectItem += OnCollectItem;
		GameSystem<ItemCraftingSystem>.Instance().CraftingFinished += OnCraftedItem;
		GameSystem<MenuSystem>.Instance().EnableMenuUpdated += RefreshMenuList;
	}

	[UsedImplicitly]
	private void OnPortraitMode(bool isPortrait)
	{
		SetLockMode(!isPortrait && GetLockState());
		for (int i = 0; i < _lockButtons.Length; i++)
		{
			((Component)_lockButtons[i]).gameObject.SetActive(!isPortrait);
		}
		Close();
		UpdateMenuBtnVisibleState();
	}

	private void SaveMenuLockState()
	{
		LockState lockState = _lockState;
		if (lockState == LockState.Lock || lockState == LockState.Unlock)
		{
			PlayerPrefs.SetInt("menu_lock", (_lockState != LockState.Unlock) ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	private bool GetLockState()
	{
		if (_lockState == LockState.None)
		{
			int @int = PlayerPrefs.GetInt("menu_lock");
			_lockState = ((@int != 0) ? LockState.Lock : LockState.Unlock);
		}
		return _lockState == LockState.Lock;
	}

	private MenuListControl GetMenuItem(MenuType type)
	{
		int i = 0;
		for (int count = _menuList.Count; i < count; i++)
		{
			MenuListControl component = _menuList[i].GetComponent<MenuListControl>();
			if (((Component)component).gameObject.activeSelf && component.Type == type)
			{
				return component;
			}
		}
		int j = 0;
		for (int count2 = _boxMenuList.Count; j < count2; j++)
		{
			MenuListControl component2 = _boxMenuList[j].GetComponent<MenuListControl>();
			if (((Component)component2).gameObject.activeSelf && component2.Type == type)
			{
				return component2;
			}
		}
		if (_bottomMenuItem.Type == type && ((Component)_bottomMenuItem).gameObject.activeSelf)
		{
			return _bottomMenuItem;
		}
		return null;
	}

	public Transform GetBottomLeftMenuTransform()
	{
		return _menuBtn.transform;
	}

	public Transform GetMenuTransform(MenuType type)
	{
		MenuListControl menuItem = GetMenuItem(type);
		return (!((Object)(object)menuItem == (Object)null)) ? ((Component)menuItem).transform : null;
	}

	private void InitMenuList()
	{
		_menuList.Clear();
		int i = 0;
		for (int num = _listMenus.Length; i < num; i++)
		{
			MenuType type = _listMenus[i];
			if (MenuSystem.IsMenuAvailable(type))
			{
				SetMenuItem(((ListObjectPoolBase<GameObject>)_menuList).Add<MenuListControl>(), type);
			}
		}
		_listMenuCount = _menuList.Count;
		_boxMenuList.Clear();
		int j = 0;
		for (int num2 = _bottomBoxMenus.Length; j < num2; j++)
		{
			MenuType type2 = _bottomBoxMenus[j];
			if (MenuSystem.IsMenuAvailable(type2))
			{
				SetMenuItem(((ListObjectPoolBase<GameObject>)_boxMenuList).Add<MenuListControl>(), type2);
				SetMenuItem(((ListObjectPoolBase<GameObject>)_menuList).Add<MenuListControl>(), type2);
			}
		}
		SetMenuItem(_bottomMenuItem, _bottomMenu);
		SetMenuItem(_lockBottomMenuItem, _bottomMenu);
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
		if (script is INewCheckerable obj)
		{
			_newChecker.AddChild(obj);
		}
	}

	private void RefreshMenuList()
	{
		RefreshMenuList(init: false);
	}

	private void RefreshMenuList(bool init)
	{
		RefreshList(init);
		_menuBg.gameObject.SetActive(!_locked);
		_lockMenuBg.gameObject.SetActive(_locked);
		((Component)_bottomWidget).gameObject.SetActive(!_locked);
		((Component)_lockBottomWidget).gameObject.SetActive(_locked);
		if (!_locked)
		{
			RefreshBoxMenuList();
		}
	}

	private void RefreshBoxMenuList()
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)_boxMenuList.BaseObject.transform.parent).GetComponent<UIWidget>();
		int num = 0;
		int i = 0;
		for (int count = _boxMenuList.Count; i < count; i++)
		{
			MenuListControl component2 = _boxMenuList[i].GetComponent<MenuListControl>();
			if (!GameSystem<MenuSystem>.Instance().IsEnabled(component2.Type))
			{
				((Component)component2).gameObject.SetActive(false);
				continue;
			}
			num++;
			((Component)component2).gameObject.SetActive(true);
		}
		float num2 = (float)component.width / (float)num;
		_boxSplitLine.Set(num - 1);
		int num3 = 0;
		int j = 0;
		for (int count2 = _boxMenuList.Count; j < count2; j++)
		{
			UIWidget component3 = _boxMenuList[j].GetComponent<UIWidget>();
			if (((Component)component3).gameObject.activeSelf)
			{
				float num4 = Mathf.Clamp01(num2 / (float)component3.width);
				Vector3 localPosition = ((Component)component3).transform.localPosition;
				localPosition.x = num2 * ((float)num3 + 0.5f);
				((Component)component3).transform.localPosition = localPosition;
				((Component)component3).transform.localScale = Vector3.one * num4;
				if (num3 > 0)
				{
					_boxSplitLine[num3 - 1].transform.localPosition = Vector3.right * num2 * (float)num3;
				}
				num3++;
			}
		}
	}

	private void RefreshList(bool init)
	{
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _menuList.Count; i++)
		{
			MenuListControl component = _menuList[i].GetComponent<MenuListControl>();
			if (!GameSystem<MenuSystem>.Instance().IsEnabled(component.Type))
			{
				((Component)component).gameObject.SetActive(false);
				continue;
			}
			if (!_locked && i >= _listMenuCount)
			{
				((Component)component).gameObject.SetActive(false);
				continue;
			}
			((Component)component).gameObject.SetActive(true);
			if (_locked)
			{
				if ((Object)(object)component.TextLabel != (Object)null)
				{
					((Component)component.TextLabel).gameObject.SetActive(false);
				}
			}
			else
			{
				if ((Object)(object)component.TextLabel != (Object)null)
				{
					((Component)component.TextLabel).gameObject.SetActive(true);
				}
				num = Mathf.Max(component.GetLabelWidth(), num);
			}
			num2++;
		}
		int num3;
		if (_locked)
		{
			num3 = _lockMenuWidth;
		}
		else
		{
			num3 = _menuWidget.Widget.leftAnchor.absolute + num + (int)_menuHorizontalMargin;
			num3 = Mathf.Max(num3, _menuMinimumWidth);
		}
		_menuWidget.Widget.rightAnchor.absolute = num3;
		Vector3 val = _menuList.BaseObject.transform.localPosition;
		_menuSplitLine.Set(num2 - 1);
		int num4 = 0;
		for (int j = 0; j < _menuList.Count; j++)
		{
			UIWidget component2 = _menuList[j].GetComponent<UIWidget>();
			if (((Component)component2).gameObject.activeSelf)
			{
				component2.width = num3;
				if (num4 > 0)
				{
					Transform transform = _menuSplitLine[num4 - 1].transform;
					Vector3 localPosition = transform.localPosition;
					localPosition.y = val.y;
					transform.localPosition = localPosition;
				}
				((Component)component2).transform.localPosition = val;
				val += Vector3.down * (float)component2.height;
				num4++;
			}
		}
		if (init)
		{
			UIUtility.ResetAnUpdateAnchors(((Component)_menuWidget).transform);
		}
		else
		{
			UIUtility.UpdateAnchors(((Component)_menuWidget).transform);
		}
	}

	private void OnClickMenuButton()
	{
		PlayCloseSound = false;
		if (base.IsOpen)
		{
			Close();
			((Component)_menuWidget).gameObject.SetActive(false);
		}
		MenuListControl menuListControl = Selectable.Current as MenuListControl;
		if (!((Object)(object)menuListControl == (Object)null))
		{
			if (this.MenuClicked != null)
			{
				this.MenuClicked(menuListControl.Type);
			}
			UIBase script = MenuSystem.GetScript(menuListControl.Type);
			if (!((Object)(object)script == (Object)null))
			{
				SetLastOpenUI(menuListControl.MenuIcon, script);
				script.Open();
			}
		}
	}

	private void OnClickLastOpenUI(GameObject go)
	{
		switch (_lastOpenCraftType)
		{
		case RecipeSystem.RecipeType.None:
			if ((Object)(object)_lastOpenUILink != (Object)null)
			{
				_lastOpenUILink.Open();
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
		if (_lastGatheringItemData != null)
		{
			GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded(delegate
			{
				InventoryGroup inventoryGroup = UIManager.FindScript<InventoryGroup>();
				inventoryGroup.Open();
				inventoryGroup.SelectItem(_lastGatheringItemData);
			});
		}
	}

	private void ClearLastCollectItem()
	{
		_lastGatheringItem.alpha = 0f;
		((Behaviour)((Component)_lastGatheringItem).GetComponent<TweenAlpha>()).enabled = false;
	}

	private void DelayedSetLastCollectItem()
	{
		ItemData lastGatheringItemData = _lastGatheringItemData;
		_lastGatheringIcon.UITexture.alpha = 1f;
		_lastGatheringIcon.SetIcon(lastGatheringItemData);
		TweenAlpha component = ((Component)_lastGatheringItem).GetComponent<TweenAlpha>();
		component.ResetToBeginning();
		component.PlayForward();
		((Component)_lastGatheringItem).GetComponent<TweenerPlayer>().Play();
	}

	private void SetLastCollectedItem(ItemData item, float delay)
	{
		_lastGatheringItemData = item;
		((MonoBehaviour)this).CancelInvoke("DelayedSetLastCollectItem");
		if (delay > 0f)
		{
			((MonoBehaviour)this).Invoke("DelayedSetLastCollectItem", delay);
		}
		else
		{
			DelayedSetLastCollectItem();
		}
	}

	private void OnCollectItem(ItemData item)
	{
		SetLastCollectedItem(item, 1.5f);
	}

	private void OnCraftedItem(IList<ItemData> item, string recipeId)
	{
		if (item.Count != 0)
		{
			SetLastCollectedItem(item[0], 0f);
		}
	}

	private void ClearLastOpenUI()
	{
		_lastOpenCraftType = RecipeSystem.RecipeType.None;
		_lastOpenCraftId = string.Empty;
		_lastOpenUILink = null;
		_lastOpenUI.alpha = 0f;
	}

	private void SetLastOpenUI(string icon, UIBase link)
	{
		_lastOpenCraftType = RecipeSystem.RecipeType.None;
		_lastOpenCraftId = string.Empty;
		_lastOpenUILink = link;
		_lastOpenUISprite.spriteName = icon;
		_lastOpenUI.alpha = 1f;
	}

	public void SetLastOpenCraft(string icon, RecipeSystem.RecipeType type, string id)
	{
		_lastOpenCraftType = type;
		_lastOpenCraftId = id;
		_lastOpenUILink = null;
		_lastOpenUISprite.spriteName = icon;
		_lastOpenUI.alpha = 1f;
	}

	private void OnUpdateNewChecker()
	{
		Array values = Enum.GetValues(typeof(MenuType));
		int i = 0;
		for (int length = values.Length; i < length; i++)
		{
			MenuType type = (MenuType)(int)values.GetValue(i);
			MenuListControl menuItem = GetMenuItem(type);
			if (!((Object)(object)menuItem == (Object)null))
			{
				UIBase script = MenuSystem.GetScript(type);
				if (script is INewCheckerable { NewChecker: not null } newCheckerable)
				{
					menuItem.NewCount = newCheckerable.NewChecker.Count;
				}
				else
				{
					menuItem.NewCount = 0;
				}
			}
		}
		_newIcon.SetActive(_newChecker.IsNew);
		_newCount.text = _newChecker.Count.ToString();
	}

	protected override bool OnOpen()
	{
		OnUpdateNewChecker();
		((Component)_menuWidget).gameObject.SetActive(true);
		_touchBlockBox.gameObject.SetActive(true);
		_menuWidget.Alpha = 1f;
		PlayShowAnimation();
		UIBase.HideUI(UIFlag.CoveredByClosable, hide: true, "LeftMenu");
		return true;
	}

	protected override bool OnClose()
	{
		_touchBlockBox.gameObject.SetActive(false);
		_menuWidget.Alpha = 0f;
		UIBase.HideUI(UIFlag.CoveredByClosable, hide: false, "LeftMenu");
		return true;
	}

	private void UpdateMenuBtnVisibleState()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !base.IsOpen && !UIBase.HasCloseable;
		bool active = flag && !_locked;
		_menuBtn.gameObject.SetActive(active);
		((Component)_lastOpenUI).gameObject.SetActive(flag);
		((Component)_lastGatheringItem).gameObject.SetActive(flag);
		((Component)_lastActionButtons).transform.localPosition = _lastActionButtonsPos + Vector3.right * ((!_locked) ? 0f : ((float)_lockMenuWidth));
	}

	private void OnPressTouchBlockBox(GameObject obj, bool press)
	{
		if (!press)
		{
			Close();
		}
	}

	private void ToggleLockMode()
	{
		bool lockState = GetLockState();
		_lockState = ((!lockState) ? LockState.Lock : LockState.Unlock);
		SetLockMode(GetLockState());
		SaveMenuLockState();
		OnUpdateNewChecker();
	}

	private void SetLockMode(bool locked)
	{
		if (locked != _locked)
		{
			_locked = locked;
			if (locked)
			{
				Close();
			}
			else
			{
				Open();
			}
			((Component)_menuWidget).gameObject.SetActive(true);
			_menuWidget.SetAlpha(1f, useTween: false);
			RefreshMenuList();
			KSingleton<UIManager>.Instance().SetRootAnchor(AnchorType.Base, _locked ? _lockMenuWidth : 0, 0, 0, 0);
		}
	}

	private void PlayShowAnimation()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = _menuList.BaseObject.GetComponent<UIWidget>();
		Vector3 localPosition = _menuList.BaseObject.transform.localPosition;
		float num = Mathf.Floor(((Component)_scrollView).GetComponent<UIPanel>().height / (float)component.height) * _showAnimationParam.Delay;
		float num2 = 0f;
		int i = 0;
		for (int count = _menuList.Count; i < count; i++)
		{
			if (_menuList[i].gameObject.activeSelf)
			{
				UIWidget component2 = _menuList[i].GetComponent<UIWidget>();
				Vector3 localPosition2 = ((Component)component2).transform.localPosition;
				localPosition2.x = localPosition.x;
				component2.alpha = 0f;
				((Component)component2).transform.localPosition = localPosition2 + _showAnimationParam.Offset;
				TweenAlpha tweenAlpha = TweenAlpha.Begin(_menuList[i], _showAnimationParam.Duration, 1f);
				TweenPosition tweenPosition = TweenPosition.Begin(_menuList[i], _showAnimationParam.Duration, localPosition2);
				tweenAlpha.animationCurve = _showAnimationParam.AlphaCurve;
				tweenPosition.animationCurve = _showAnimationParam.PositionCurve;
				tweenAlpha.delay = num2;
				tweenPosition.delay = num2;
				num2 += _showAnimationParam.Delay;
			}
		}
		num2 = Mathf.Min(num, num2);
		UIWidget bottomWidget = _bottomWidget;
		bottomWidget.alpha = 0f;
		TweenAlpha tweenAlpha2 = TweenAlpha.Begin(((Component)bottomWidget).gameObject, _showAnimationParam.Duration, 1f);
		tweenAlpha2.animationCurve = _showAnimationParam.AlphaCurve;
		tweenAlpha2.delay = num2;
	}
}

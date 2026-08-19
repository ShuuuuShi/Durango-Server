using System;
using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using InteractionData;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public abstract class InteractionMenuListWidgetBase : AnimationWidget, IUIInitializable
{
	public Action<InteractionMenuData> OnClickInteractionMenu;

	public Action<InteractionMenuData> OnLongPressInteractionMenu;

	[SerializeField]
	protected float Radius;

	[SerializeField]
	protected float VisibleStartDegree;

	[SerializeField]
	protected int[] VisibleOrder;

	protected int VisibleStartIndex;

	[SerializeField]
	protected GameObject NextArrow;

	[SerializeField]
	protected GameObject PrevArrow;

	protected readonly List<InteractionMenuWidgetBase> Menus = new List<InteractionMenuWidgetBase>();

	protected bool IsInit;

	protected InteractionObject MenuTarget;

	protected InteractionMenuWidgetBase SubMenuParent;

	protected int VisiblePage;

	[SerializeField]
	private UILabel _targetNameLabel;

	[SerializeField]
	private InteractionMenuWidgetBase _interactionMenuWidget;

	[SerializeField]
	private UIWidget _pageButtonContainer;

	[SerializeField]
	private float _majorScale;

	[SerializeField]
	private float _minorScale;

	[SerializeField]
	private InteractionSubMenuWidget _baseSubMenu;

	[SerializeField]
	private KScrollView _craftSlotScrollView;

	[SerializeField]
	private GameObject _mannequinSlots;

	[SerializeField]
	private InteractionBottomSlotWidget _mannequinHeadSlot;

	[SerializeField]
	private InteractionBottomSlotWidget _mannequinBodySlot;

	private readonly Queue<InteractionMenuWidgetBase> _interactionMenuPool = new Queue<InteractionMenuWidgetBase>();

	private readonly ListObjectPool<InteractionSubMenuWidget> _subMenus = new ListObjectPool<InteractionSubMenuWidget>();

	private bool _updateFlag;

	private bool _updateResetFlag;

	private bool _isShowSubmenus;

	private List<Interaction> _subMenuList = new List<Interaction>();

	private bool _submenusUpdateFlag;

	private bool _isCancelCraftingMode;

	private DelayedFunction _updateQueueFunc;

	public static float MajorScale { get; private set; }

	public static float MinorScale { get; private set; }

	public static bool IsShow { get; private set; }

	protected int VisibleCountPerPage => VisibleOrder.Length;

	private string TargetName
	{
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				_targetNameLabel.gameObject.SetActive(value: false);
				return;
			}
			_targetNameLabel.gameObject.SetActive(value: true);
			_targetNameLabel.text = value;
		}
	}

	public virtual void Init()
	{
		if (!IsInit)
		{
			IsInit = true;
			_interactionMenuWidget.gameObject.SetActive(value: false);
			UIEventListener.Get(_targetNameLabel.gameObject).onDrag = UIManager.IgnoreUIDrag;
			_subMenus.BaseObject = _baseSubMenu;
			_subMenus.Init(delegate(InteractionSubMenuWidget comp)
			{
				comp.Clicked = OnClickSubmenu;
			});
			_subMenus.Clear();
			_craftSlotScrollView.Nodes.Init(delegate(GameObject obj)
			{
				InteractionCraftSlotWidget component = obj.GetComponent<InteractionCraftSlotWidget>();
				component.CancelModeChanged += SetCancelCraftingMode;
				component.EmptyClicked += OnClickEmptyCraftSlot;
			});
			_mannequinHeadSlot.Clicked += delegate(Item? item)
			{
				GameSystem<InteractionSystem>.Instance().SelectTargetInteraction((!item.HasValue) ? Interaction.ChangeMannequinHead : Interaction.TakeOffMannequinHead);
			};
			_mannequinBodySlot.Clicked += delegate(Item? item)
			{
				GameSystem<InteractionSystem>.Instance().SelectTargetInteraction((!item.HasValue) ? Interaction.ChangeMannequinBody : Interaction.TakeOffMannequinBody);
			};
			MajorScale = _majorScale;
			MinorScale = _minorScale;
		}
	}

	private void Start()
	{
		_updateQueueFunc = new DelayedFunction(SetGatheringQueueList);
		if (IsShow)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	private void OnEnable()
	{
		GameSystem<InteractionSystem>.Instance().ReservationQueue.Updated += OnUpdateGatheringQueue;
		GameSystem<InteractionSystem>.Instance().MenuList.Updated += OnUpdateInteractionMenu;
		GameSystem<InteractionSystem>.Instance().MenuList.Cleared += OnClearInteractionMenu;
	}

	private void OnDisable()
	{
		GameSystem<InteractionSystem>.Instance().ReservationQueue.Updated -= OnUpdateGatheringQueue;
		GameSystem<InteractionSystem>.Instance().MenuList.Updated -= OnUpdateInteractionMenu;
		GameSystem<InteractionSystem>.Instance().MenuList.Cleared -= OnClearInteractionMenu;
		MenuTarget = null;
		_isCancelCraftingMode = false;
		VisiblePage = 0;
	}

	protected virtual void LateUpdate()
	{
		RepositionInteractionMenuContainer();
		LateUpdateMenuList();
		RefreshSubMenus();
	}

	public virtual void Show()
	{
		Init();
		IsShow = true;
		base.gameObject.SetActive(value: true);
		base.Alpha = 1f;
		UpdateMenuList();
	}

	public virtual void Hide()
	{
		Init();
		IsShow = false;
		ClearSubMenus();
		base.Alpha = 0f;
	}

	public InteractionMenuWidgetBase FindMenu(Interaction action, params string[] argument)
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			if (Menus[i].Data.Action == action)
			{
				if (string.IsNullOrEmpty(Menus[i].Data.Id) && KUtility.GetSize(argument) == 0)
				{
					return Menus[i];
				}
				if (argument != null && argument.Contains(Menus[i].Data.Id))
				{
					return Menus[i];
				}
			}
		}
		return null;
	}

	public void ShowSubmenus(Interaction parent, params Interaction[] menus)
	{
		int size = KUtility.GetSize(menus);
		if (size == 0)
		{
			ClearSubMenus();
			return;
		}
		InteractionMenuWidgetBase interactionMenuWidgetBase = FindMenu(parent);
		if (interactionMenuWidgetBase == null || interactionMenuWidgetBase == SubMenuParent)
		{
			ClearSubMenus();
			return;
		}
		if (_isShowSubmenus && SubMenuParent != null && parent == SubMenuParent.Data.Action && _subMenuList.Count == size)
		{
			bool flag = true;
			for (int i = 0; i < size; i++)
			{
				if (_subMenuList[i] != menus[i])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return;
			}
		}
		if (SubMenuParent != null)
		{
			SubMenuParent.Selected = false;
		}
		_isShowSubmenus = true;
		_submenusUpdateFlag = true;
		SubMenuParent = interactionMenuWidgetBase;
		_subMenuList.Clear();
		for (int j = 0; j < size; j++)
		{
			_subMenuList.Add(menus[j]);
		}
		UpdateMenuList();
	}

	public void ClearSubMenus()
	{
		if (SubMenuParent != null)
		{
			SubMenuParent.Selected = false;
		}
		_pageButtonContainer.alpha = 1f;
		_isShowSubmenus = false;
		_submenusUpdateFlag = false;
		SubMenuParent = null;
		_subMenuList.Clear();
		_subMenus.Clear();
	}

	private void OnUpdateInteractionMenu()
	{
		if (IsShow)
		{
			UpdateMenuList();
		}
	}

	private void OnUpdateGatheringQueue()
	{
		if (IsShow)
		{
			_updateQueueFunc.Call(this);
		}
	}

	private void OnClearInteractionMenu()
	{
		ClearSubMenus();
	}

	private void UpdateMenuList()
	{
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		_updateFlag = true;
		_updateResetFlag |= menuList.ResetFrame == Time.frameCount;
	}

	private void LateUpdateMenuList()
	{
		if (_updateFlag)
		{
			InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
			TargetName = menuList.Name;
			if (_updateResetFlag)
			{
				ClearSubMenus();
				RemoveAll();
			}
			UpdateCraftSlots();
			UpdateMannequinSlots();
			Set(menuList);
			Reposition(!_updateResetFlag);
			_updateFlag = false;
			_updateResetFlag = false;
		}
	}

	private void UpdateCraftSlots()
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		Touched lastTouched = GameSystem<InteractionSystem>.Instance().LastTouched;
		ListObjectPool nodes = _craftSlotScrollView.Nodes;
		if (target != null && !(target.EntityId != lastTouched.EntityId))
		{
			Workbench? workbench = lastTouched.Workbench;
			if (workbench.HasValue)
			{
				Workbench value = lastTouched.Workbench.Value;
				int size = KUtility.GetSize(value.Crafteds);
				int size2 = KUtility.GetSize(value.Craftings);
				int count = nodes.Count;
				nodes.Set(Mathf.Max(size + size2, (int)value.Capacity));
				for (int i = 0; i < nodes.Count; i++)
				{
					InteractionCraftSlotWidget component = nodes[i].GetComponent<InteractionCraftSlotWidget>();
					component.Valid = false;
					component.SetIndex(i);
				}
				for (int j = 0; j < size; j++)
				{
					CraftedResult crafted = value.Crafteds[j];
					int num = -1;
					for (int k = 0; k < count; k++)
					{
						InteractionCraftSlotWidget component2 = nodes[k].GetComponent<InteractionCraftSlotWidget>();
						if (!component2.Valid && component2.Id == crafted.Id)
						{
							num = k;
							break;
						}
					}
					if (num != -1)
					{
						nodes[num].GetComponent<InteractionCraftSlotWidget>().SetCrafted(crafted);
					}
				}
				for (int l = 0; l < size2; l++)
				{
					Messages.Crafting crafting = value.Craftings[l];
					int num2 = -1;
					for (int m = 0; m < count; m++)
					{
						InteractionCraftSlotWidget component3 = nodes[m].GetComponent<InteractionCraftSlotWidget>();
						if (!component3.Valid && component3.Id == crafting.Id)
						{
							num2 = m;
							break;
						}
					}
					if (num2 != -1)
					{
						nodes[num2].GetComponent<InteractionCraftSlotWidget>().SetCrafting(crafting);
					}
				}
				for (int n = 0; n < size; n++)
				{
					CraftedResult crafted2 = value.Crafteds[n];
					int num3 = -1;
					for (int num4 = 0; num4 < nodes.Count; num4++)
					{
						InteractionCraftSlotWidget component4 = nodes[num4].GetComponent<InteractionCraftSlotWidget>();
						if (component4.Valid && component4.Id == crafted2.Id)
						{
							num3 = num4;
							break;
						}
					}
					if (num3 != -1)
					{
						continue;
					}
					for (int num5 = 0; num5 < nodes.Count; num5++)
					{
						if (!nodes[num5].GetComponent<InteractionCraftSlotWidget>().Valid)
						{
							num3 = num5;
							break;
						}
					}
					if (num3 != -1)
					{
						nodes[num3].GetComponent<InteractionCraftSlotWidget>().SetCrafted(crafted2);
					}
				}
				for (int num6 = 0; num6 < size2; num6++)
				{
					Messages.Crafting crafting2 = value.Craftings[num6];
					int num7 = -1;
					for (int num8 = 0; num8 < nodes.Count; num8++)
					{
						InteractionCraftSlotWidget component5 = nodes[num8].GetComponent<InteractionCraftSlotWidget>();
						if (component5.Valid && component5.Id == crafting2.Id)
						{
							num7 = num8;
							break;
						}
					}
					if (num7 != -1)
					{
						continue;
					}
					for (int num9 = 0; num9 < nodes.Count; num9++)
					{
						if (!nodes[num9].GetComponent<InteractionCraftSlotWidget>().Valid)
						{
							num7 = num9;
							break;
						}
					}
					if (num7 != -1)
					{
						nodes[num7].GetComponent<InteractionCraftSlotWidget>().SetCrafting(crafting2);
					}
				}
				for (int num10 = 0; num10 < nodes.Count; num10++)
				{
					InteractionCraftSlotWidget component6 = nodes[num10].GetComponent<InteractionCraftSlotWidget>();
					if (!component6.Valid)
					{
						component6.SetEmpty();
					}
				}
				if (_updateResetFlag)
				{
					TweenAlpha component7 = _craftSlotScrollView.GetComponent<TweenAlpha>();
					component7.ResetToBeginning();
					component7.PlayForward();
				}
				int num11 = Mathf.Min(UIManager.ScreenWidth - 120, 600);
				int num12 = _craftSlotScrollView.GetNode(0).width + _craftSlotScrollView.Margin;
				num11 = (int)(Mathf.Floor((float)num11 / (float)num12) * ((float)num12 + 0.5f));
				_craftSlotScrollView.GetComponent<UIWidget>().width = (int)((float)num11 + _craftSlotScrollView.Panel.clipSoftness.x * 2f);
				UIUtility.UpdateAnchors(_craftSlotScrollView.transform);
				_craftSlotScrollView.UpdateLayout();
				if (_craftSlotScrollView.ViewLength > _craftSlotScrollView.ContentsLength)
				{
					_craftSlotScrollView.MoveTo((0f - (_craftSlotScrollView.ViewLength - _craftSlotScrollView.ContentsLength)) * 0.5f, instant: true, restrictWithinPanel: false);
					_craftSlotScrollView.ScrollView.enabled = false;
				}
				else
				{
					_craftSlotScrollView.ScrollView.enabled = true;
					if (_updateFlag)
					{
						_craftSlotScrollView.MoveTo(0f, instant: true);
					}
				}
				SetCancelCraftingMode(_isCancelCraftingMode);
				return;
			}
		}
		_isCancelCraftingMode = false;
		nodes.Clear();
	}

	private void UpdateMannequinSlots()
	{
		InteractionObject target = GameSystem<InteractionSystem>.Instance().Target;
		Touched lastTouched = GameSystem<InteractionSystem>.Instance().LastTouched;
		if (target != null && !(target.EntityId != lastTouched.EntityId))
		{
			Messages.Mannequin? mannequin = lastTouched.Mannequin;
			if (mannequin.HasValue)
			{
				_mannequinSlots.SetActive(value: true);
				Messages.Mannequin value = lastTouched.Mannequin.Value;
				_mannequinHeadSlot.SetItem(value.Head);
				_mannequinBodySlot.SetItem(value.Body);
				return;
			}
		}
		_mannequinSlots.SetActive(value: false);
	}

	private void SetCancelCraftingMode(bool on)
	{
		_isCancelCraftingMode = on;
		ListObjectPool nodes = _craftSlotScrollView.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			nodes[i].GetComponent<InteractionCraftSlotWidget>().SetCancelMode(on);
		}
	}

	private void OnClickEmptyCraftSlot()
	{
		ClearSubMenus();
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		for (int i = 0; i < InteractionCraftSlotWidget.EmptyInteractionPriority.Length; i++)
		{
			int num = menuList.IndexOf(InteractionCraftSlotWidget.EmptyInteractionPriority[i]);
			if (num != -1)
			{
				if (OnClickInteractionMenu != null)
				{
					OnClickInteractionMenu(menuList[num]);
				}
				break;
			}
		}
	}

	protected virtual void OnClickMenu()
	{
		InteractionMenuWidgetBase interactionMenuWidgetBase = (InteractionMenuWidgetBase)Selectable.Current;
		if (interactionMenuWidgetBase != SubMenuParent)
		{
			ClearSubMenus();
		}
		if (OnClickInteractionMenu != null)
		{
			OnClickInteractionMenu(interactionMenuWidgetBase.Data);
		}
	}

	protected void OnLongpressMenu()
	{
		InteractionMenuWidgetBase interactionMenuWidgetBase = (InteractionMenuWidgetBase)Selectable.Current;
		if (interactionMenuWidgetBase != SubMenuParent)
		{
			ClearSubMenus();
		}
		if (OnLongPressInteractionMenu != null)
		{
			OnLongPressInteractionMenu(interactionMenuWidgetBase.Data);
		}
	}

	private void OnClickSubmenu()
	{
		int num = _subMenus.IndexOf((InteractionSubMenuWidget)Selectable.Current);
		if (num != -1)
		{
			InteractionMenuData obj = new InteractionMenuData(_subMenuList[num]);
			obj.Id = SubMenuParent.Data.Id;
			if (OnClickInteractionMenu != null)
			{
				OnClickInteractionMenu(obj);
			}
		}
	}

	protected virtual void SetGatheringQueueList()
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[i];
			if (GameSystem<InteractionSystem>.Instance().ReservationQueue.TryGetQueueItems(interactionMenuWidgetBase.Data.Action, interactionMenuWidgetBase.Data.Id, out var items))
			{
				interactionMenuWidgetBase.SetReservedQueueList(items);
			}
			else
			{
				interactionMenuWidgetBase.ClearReservedQueueList();
			}
		}
	}

	protected abstract void Reposition(bool instant);

	private void RepositionInteractionMenuContainer()
	{
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		MenuTarget = ((lastInteractionTarget != null && lastInteractionTarget.IsValid()) ? lastInteractionTarget : null);
		base.transform.localPosition = ((MenuTarget != null) ? MainCamera.WorldToNGUIPos(MenuTarget.Position, base.transform.parent) : Vector3.zero);
	}

	private int IndexOf(InteractionMenuData data)
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			if (Menus[i].Data.IsEqualKey(data))
			{
				return i;
			}
		}
		return -1;
	}

	private void Set(InteractionMenuList list)
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			Menus[i].Valid = false;
		}
		bool flag = false;
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			if (Add(list[j]))
			{
				flag = true;
			}
		}
		if (flag)
		{
			SoundManager.PlayEvent("ui_hexagon_popup");
		}
		ClearInvalidMenu();
	}

	protected virtual void ClearInvalidMenu()
	{
		for (int num = Menus.Count - 1; num >= 0; num--)
		{
			if (!Menus[num].Valid)
			{
				RemoveAt(num);
			}
		}
	}

	private bool Add(InteractionMenuData data)
	{
		int num = IndexOf(data);
		InteractionMenuWidgetBase interactionMenuWidgetBase;
		if (num != -1)
		{
			interactionMenuWidgetBase = Menus[num];
			interactionMenuWidgetBase.gameObject.SetActive(value: true);
			interactionMenuWidgetBase.Set(data, MenuTarget);
			return false;
		}
		interactionMenuWidgetBase = InteractionMenu_Pop();
		interactionMenuWidgetBase.Set(data, MenuTarget);
		Menus.Add(interactionMenuWidgetBase);
		interactionMenuWidgetBase.Index = FindEmptyIndex();
		SoundManager.PlayEvent("ui_hexagon_sub_popup");
		return true;
	}

	protected void RemoveAll()
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			InteractionMenu_Push(Menus[i]);
		}
		Menus.Clear();
	}

	protected void RemoveAt(int index)
	{
		if (index >= 0 && index < Menus.Count)
		{
			InteractionMenuWidgetBase menuWidget = Menus[index];
			Menus.RemoveAt(index);
			InteractionMenu_Push(menuWidget);
		}
	}

	protected int FindEmptyIndex()
	{
		int count = Menus.Count;
		int num = 0;
		while (true)
		{
			bool flag = true;
			for (int i = 0; i < count; i++)
			{
				InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[i];
				if (interactionMenuWidgetBase != null && interactionMenuWidgetBase.Index == num)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			num++;
		}
		return num;
	}

	protected InteractionMenuWidgetBase InteractionMenu_Pop()
	{
		InteractionMenuWidgetBase interactionMenuWidgetBase;
		if (_interactionMenuPool.Count == 0)
		{
			interactionMenuWidgetBase = _interactionMenuWidget.transform.parent.gameObject.AddChild(_interactionMenuWidget.gameObject).GetComponent<InteractionMenuWidgetBase>();
			SetMenuWidgetEvent(interactionMenuWidgetBase);
		}
		else
		{
			interactionMenuWidgetBase = _interactionMenuPool.Dequeue();
		}
		interactionMenuWidgetBase.gameObject.SetActive(value: true);
		interactionMenuWidgetBase.NeedInitAnimation = true;
		interactionMenuWidgetBase.Valid = false;
		interactionMenuWidgetBase.Index = -1;
		return interactionMenuWidgetBase;
	}

	protected virtual void SetMenuWidgetEvent(InteractionMenuWidgetBase menuWidget)
	{
		menuWidget.Clicked = OnClickMenu;
		menuWidget.LongPressed = OnLongpressMenu;
		menuWidget.OnHovered = delegate(bool isHoverd)
		{
			if (menuWidget.IsWarning())
			{
				GameCursorUtil.SetGameCursorDisabled(isHoverd);
			}
		};
	}

	private void InteractionMenu_Push(InteractionMenuWidgetBase menuWidget)
	{
		menuWidget.gameObject.SetActive(value: false);
		_interactionMenuPool.Enqueue(menuWidget);
	}

	private void RefreshSubMenus()
	{
		if (!_submenusUpdateFlag)
		{
			return;
		}
		_submenusUpdateFlag = false;
		InteractionMenuWidgetBase subMenuParent = SubMenuParent;
		if (subMenuParent == null)
		{
			ClearSubMenus();
			return;
		}
		subMenuParent.Selected = true;
		_pageButtonContainer.alpha = 0f;
		Vector3 localPosition = subMenuParent.transform.localPosition;
		int sign = subMenuParent.GetSign();
		_subMenus.BeginLoad();
		for (int i = 0; i < _subMenuList.Count; i++)
		{
			Interaction num = _subMenuList[i];
			string text = IconMap.Get(num);
			string text2 = num.GetName();
			InteractionSubMenuWidget next = _subMenus.GetNext();
			next.Set(text, text2, sign);
			float menuRadian = subMenuParent.MenuRadian;
			menuRadian = menuRadian + (float)Math.PI / 6f - (float)Math.PI / 3f * (float)i;
			Vector3 localPosition2 = localPosition + new Vector3(Mathf.Cos(menuRadian), Mathf.Sin(menuRadian)) * 110f;
			next.transform.localPosition = localPosition2;
			next.Widget.alpha = 0f;
			TweenAlpha.Begin(next.gameObject, 0.2f, 1f);
		}
		_subMenus.EndLoad();
	}

	public virtual bool CloseMenus()
	{
		return false;
	}
}

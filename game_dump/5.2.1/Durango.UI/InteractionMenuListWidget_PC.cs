using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.InputSystem;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class InteractionMenuListWidget_PC : InteractionMenuListWidgetBase
{
	[SerializeField]
	private InteractionBattleMenuWidget _battleMenu;

	[SerializeField]
	private RingMenuSelector _menuSelector;

	[SerializeField]
	private UILabel _pageDesc;

	[SerializeField]
	private UISprite _targetNameBg;

	[SerializeField]
	[Tooltip("상호작용 대상과 (움직이는 중인)플레이어 간의 최대 거리")]
	private float _maxDistanceFromPlayer;

	protected override void LateUpdate()
	{
		base.LateUpdate();
		CloseOnDistance();
	}

	public override void Init()
	{
		if (IsInit)
		{
			return;
		}
		base.Init();
		UIEventListener.Get(NextArrow).onClick = delegate
		{
			OnClickNextArrow();
		};
		UIEventListener.Get(PrevArrow).onClick = delegate
		{
			OnClickPrevArrow();
		};
		for (int i = 0; i < base.VisibleCountPerPage; i++)
		{
			int count = i;
			GameSystem<InputSystem>.Instance().On((InputCommand)(49 + count), delegate(InputCommandMessage message)
			{
				ClickHotKey(count, message.CurrentTrigger);
			});
		}
		InteractionBattleMenuWidget battleMenu = _battleMenu;
		battleMenu.Clicked = (Action)Delegate.Combine(battleMenu.Clicked, new Action(OnClickBattleMenu));
		_battleMenu.SetRadius(Radius);
		_menuSelector.SetRadius(Radius);
	}

	public override void Show()
	{
		base.Show();
		base.Scale = Vector3.one;
	}

	public override void Hide()
	{
		base.Hide();
		base.Scale = Vector3.zero;
	}

	protected override void OnClickMenu()
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

	private void OnRightClickMenu()
	{
		InteractionMenuWidgetBase obj = (InteractionMenuWidgetBase)Selectable.Current;
		if (obj != SubMenuParent)
		{
			ClearSubMenus();
		}
		obj.RemoveFirstQueue();
	}

	private void OnClickBattleMenu()
	{
		InteractionBattleMenuWidget interactionBattleMenuWidget = (InteractionBattleMenuWidget)Selectable.Current;
		if (!(interactionBattleMenuWidget == null) && OnClickInteractionMenu != null)
		{
			OnClickInteractionMenu(interactionBattleMenuWidget.Data);
		}
	}

	private void ClickHotKey(int index, Trigger currentTrigger)
	{
		if (!_menuSelector.gameObject.activeInHierarchy)
		{
			return;
		}
		foreach (InteractionMenuWidget_PC menu in _menuSelector.Menus)
		{
			if (menu.gameObject.activeInHierarchy && menu.Index % base.VisibleCountPerPage == index)
			{
				switch (currentTrigger)
				{
				case Trigger.Down:
					menu.SetPress(isPress: true, isShortcut: true);
					break;
				case Trigger.Up:
					menu.SetPress(isPress: false, isShortcut: true);
					menu.SetClick();
					break;
				}
				break;
			}
		}
	}

	public override bool CloseMenus()
	{
		if (!InteractionMenuListWidgetBase.IsShow)
		{
			return false;
		}
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		return !InteractionMenuListWidgetBase.IsShow;
	}

	private void OnClickNextArrow(InputCommandMessage message = null)
	{
		if (NextArrow.activeInHierarchy && !NextArrow.GetComponent<SelectableWidget>().Disabled)
		{
			VisiblePage++;
			if (VisiblePage * base.VisibleCountPerPage > Menus.Count((InteractionMenuWidgetBase x) => x.Valid))
			{
				VisiblePage = 0;
			}
			RefreshMenus();
		}
	}

	private void OnClickPrevArrow(InputCommandMessage message = null)
	{
		if (!PrevArrow.activeInHierarchy || PrevArrow.GetComponent<SelectableWidget>().Disabled)
		{
			return;
		}
		VisiblePage--;
		if (VisiblePage < 0)
		{
			VisiblePage = (Menus.Count((InteractionMenuWidgetBase x) => x.Valid) - 1) / base.VisibleCountPerPage;
		}
		RefreshMenus();
	}

	private void RefreshMenus()
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			Menus[i].NeedInitAnimation = true;
		}
		ClearSubMenus();
		Reposition(instant: true);
	}

	protected override void ClearInvalidMenu()
	{
		for (int num = Menus.Count - 1; num >= 0; num--)
		{
			if (!Menus[num].Valid && !Menus[num].Empty)
			{
				RemoveAt(num);
			}
		}
		if (Menus.Count((InteractionMenuWidgetBase x) => x.Valid) == 0)
		{
			RemoveAll();
			return;
		}
		int num2 = Menus.Count((InteractionMenuWidgetBase x) => x.Empty);
		if (num2 < base.VisibleCountPerPage)
		{
			return;
		}
		int num3 = 0;
		int num4 = Menus.Count - 1;
		while (num4 >= 0)
		{
			if (Menus[num4].Empty)
			{
				RemoveAt(num4);
				num3++;
			}
			if (num3 < num2)
			{
				num4--;
				continue;
			}
			break;
		}
	}

	protected override void SetMenuWidgetEvent(InteractionMenuWidgetBase menuWidget)
	{
		menuWidget.Clicked = OnClickMenu;
		menuWidget.RightClicked = OnRightClickMenu;
		menuWidget.LongPressed = base.OnLongpressMenu;
	}

	protected override void SetGatheringQueueList()
	{
		for (int i = 0; i < Menus.Count; i++)
		{
			InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[i];
			if (!interactionMenuWidgetBase.IsWarning() && !interactionMenuWidgetBase.Empty && !interactionMenuWidgetBase.Disabled)
			{
				GameSystem<InteractionSystem>.Instance().ReservationQueue.TryGetQueueItems(interactionMenuWidgetBase.Data.Action, interactionMenuWidgetBase.Data.Id, out var items);
				interactionMenuWidgetBase.SetReservedQueueList(items);
			}
		}
	}

	protected override void Reposition(bool instant)
	{
		if (MenuTarget != null && ObjectIdentifier.IsTargetableEnemy(MenuTarget.Target, includePets: false))
		{
			RepositionBattleMenuItem();
		}
		else
		{
			RepositionMenuItems();
		}
		_targetNameBg.UpdateAnchors();
	}

	private void RepositionMenuItems()
	{
		_battleMenu.gameObject.SetActive(value: false);
		int count = Menus.Count;
		int min = VisiblePage * base.VisibleCountPerPage;
		int max = (VisiblePage + 1) * base.VisibleCountPerPage;
		if (count == 0)
		{
			_menuSelector.gameObject.SetActive(value: false);
			PrevArrow.SetActive(value: false);
			NextArrow.SetActive(value: false);
			_pageDesc.gameObject.SetActive(value: false);
			return;
		}
		while (true)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[i];
				if (interactionMenuWidgetBase.Index < min || interactionMenuWidgetBase.Index >= max)
				{
					interactionMenuWidgetBase.Index = -1;
				}
				else
				{
					num++;
				}
			}
			if (num != 0 || VisiblePage <= 0)
			{
				break;
			}
			VisiblePage--;
			min -= base.VisibleCountPerPage;
			max -= base.VisibleCountPerPage;
		}
		for (int j = 0; j < count; j++)
		{
			InteractionMenuWidgetBase interactionMenuWidgetBase2 = Menus[j];
			if (interactionMenuWidgetBase2.Index == -1)
			{
				interactionMenuWidgetBase2.Index = FindEmptyIndex();
				interactionMenuWidgetBase2.NeedInitAnimation = true;
			}
		}
		for (int k = 0; k < count; k++)
		{
			InteractionMenuWidget_PC interactionMenuWidget_PC = (InteractionMenuWidget_PC)Menus[k];
			if (interactionMenuWidget_PC.Index < min || interactionMenuWidget_PC.Index >= max)
			{
				interactionMenuWidget_PC.gameObject.SetActive(value: false);
				continue;
			}
			interactionMenuWidget_PC.gameObject.SetActive(value: true);
			RepositionMenuItem(interactionMenuWidget_PC);
		}
		bool flag = Menus.Exists((InteractionMenuWidgetBase x) => x.Empty);
		for (int l = count; l < max; l++)
		{
			InteractionMenuWidget_PC interactionMenuWidget_PC2 = (InteractionMenuWidget_PC)InteractionMenu_Pop();
			interactionMenuWidget_PC2.SetEmpty();
			Menus.Add(interactionMenuWidget_PC2);
			interactionMenuWidget_PC2.NeedInitAnimation = !flag;
			interactionMenuWidget_PC2.Index = FindEmptyIndex();
			interactionMenuWidget_PC2.gameObject.SetActive(value: true);
			RepositionMenuItem(interactionMenuWidget_PC2);
		}
		List<InteractionMenuWidgetBase> activeMenus = Menus.FindAll((InteractionMenuWidgetBase x) => x.Index >= min && x.Index < max);
		_menuSelector.SetActiveMenus(activeMenus);
		if (count > base.VisibleCountPerPage)
		{
			PrevArrow.SetActive(value: true);
			NextArrow.SetActive(value: true);
			_pageDesc.gameObject.SetActive(value: true);
			_pageDesc.text = $"[f4c83e]{VisiblePage + 1}[-] / [c3c3c3]{(Menus.Count((InteractionMenuWidgetBase x) => x.Valid) - 1) / base.VisibleCountPerPage + 1}[-]";
		}
		else
		{
			PrevArrow.SetActive(value: false);
			NextArrow.SetActive(value: false);
			_pageDesc.gameObject.SetActive(value: false);
		}
	}

	private void RepositionMenuItem(InteractionMenuWidget_PC menuWidget)
	{
		int num = menuWidget.Index % base.VisibleCountPerPage;
		float num2 = ((menuWidget.Type != 0) ? InteractionMenuListWidgetBase.MinorScale : InteractionMenuListWidgetBase.MajorScale);
		float num3 = 360f / (float)base.VisibleCountPerPage;
		float num4 = Mathf.Repeat(VisibleStartDegree - (float)num * num3, 360f) * ((float)Math.PI / 180f);
		float num5 = Radius + (float)menuWidget.Widget.width * 0.5f * (num2 - 1f);
		Vector2 vector = default(Vector2);
		vector.x = Mathf.Cos(num4) * num5;
		vector.y = Mathf.Sin(num4) * num5;
		menuWidget.MenuRadian = num4;
		if (menuWidget.NeedInitAnimation)
		{
			menuWidget.transform.localPosition = Vector3.Lerp(Vector3.zero, vector, 0.5f);
			menuWidget.Widget.alpha = 0f;
			TweenPosition positionTweener = menuWidget.PositionTweener;
			positionTweener.from = menuWidget.transform.localPosition;
			positionTweener.to = vector;
			positionTweener.tweenFactor = 0f;
			positionTweener.PlayForward();
			TweenAlpha alphaTweener = menuWidget.AlphaTweener;
			alphaTweener.from = menuWidget.Widget.alpha;
			alphaTweener.to = menuWidget.Alpha;
			alphaTweener.tweenFactor = 0f;
			alphaTweener.PlayForward();
			menuWidget.NeedInitAnimation = false;
		}
		else
		{
			TweenPosition positionTweener2 = menuWidget.PositionTweener;
			if (positionTweener2.enabled)
			{
				positionTweener2.to = vector;
			}
			else
			{
				menuWidget.transform.localPosition = vector;
			}
		}
		menuWidget.UpdateUIPosition();
		menuWidget.UpdateShortcut();
	}

	private void RepositionBattleMenuItem()
	{
		if (Menus.Count != 1)
		{
			return;
		}
		foreach (InteractionMenuWidgetBase menu in Menus)
		{
			menu.gameObject.SetActive(value: false);
		}
		PrevArrow.SetActive(value: false);
		NextArrow.SetActive(value: false);
		_menuSelector.gameObject.SetActive(value: false);
		_pageDesc.gameObject.SetActive(value: false);
		_battleMenu.gameObject.SetActive(value: true);
		InteractionMenuWidgetBase interactionMenuWidgetBase = Menus[0];
		_battleMenu.Set(interactionMenuWidgetBase.Data);
	}

	private void CloseOnDistance()
	{
		if (InteractionMenuListWidgetBase.IsShow && MenuTarget != null && (bool)PlayerBehavior.LocalPlayer.IsMoving && !((PlayerBehavior.LocalPlayer.CurrentPosition - MenuTarget.Position).sqrMagnitude < _maxDistanceFromPlayer * _maxDistanceFromPlayer))
		{
			GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		}
	}
}

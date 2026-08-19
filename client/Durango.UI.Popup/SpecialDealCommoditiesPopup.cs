using System;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class SpecialDealCommoditiesPopup : TooltipBase
{
	[SerializeField]
	private KScrollView _scroll;

	[SerializeField]
	private PageIndexSprite _pageIndex;

	[SerializeField]
	private SelectableWidget _buttonPrevious;

	[SerializeField]
	private SelectableWidget _buttonNext;

	[SerializeField]
	private SelectableButton _buttonAfterwards;

	[SerializeField]
	private SelectableButton _buttonBuyNow;

	[SerializeField]
	private TweenerPlayer _tweenerPlayer;

	[SerializeField]
	private UILabel _textWarning;

	[SerializeField]
	private RectLayout _layout;

	private int _currentIndex;

	private int _pageCount;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.PrevOnModalPopup, delegate
		{
			MoveLeft();
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.NextOnModalPopup, delegate
		{
			MoveRight();
		});
		_scroll.AttachPageIndexSprite(_pageIndex);
		_scroll.DragFinshed += Scroll_DragFinshed;
	}

	protected override void Start()
	{
		base.Start();
		SelectableWidget buttonPrevious = _buttonPrevious;
		buttonPrevious.Clicked = (Action)Delegate.Combine(buttonPrevious.Clicked, new Action(ButtonPrevious_Clicked));
		SelectableWidget buttonNext = _buttonNext;
		buttonNext.Clicked = (Action)Delegate.Combine(buttonNext.Clicked, new Action(ButtonNext_Clicked));
		SelectableButton buttonAfterwards = _buttonAfterwards;
		buttonAfterwards.Clicked = (Action)Delegate.Combine(buttonAfterwards.Clicked, new Action(ButtonAfterwards_Clicked));
		SelectableButton buttonBuyNow = _buttonBuyNow;
		buttonBuyNow.Clicked = (Action)Delegate.Combine(buttonBuyNow.Clicked, new Action(ButtonBuyNow_Clicked));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<ShopSystem>.Instance().SpecialDealsUpdated += ShopSystem_SpecialDealsUpdated;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameSystem<ShopSystem>.Instance().SpecialDealsUpdated -= ShopSystem_SpecialDealsUpdated;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		int num = Mathf.FloorToInt(_scroll.GoalOffset / _scroll.ViewLength);
		bool flag = _pageCount > 1;
		_buttonPrevious.gameObject.SetActive(flag && num > 0);
		_buttonNext.gameObject.SetActive(flag && num < _scroll.GetNodeCount() - 1);
	}

	protected override void OnShow()
	{
		base.OnShow();
		string key = GetType().ToString();
		UIManager.FindScript<DialogueGroupBase>().SetVisible(visible: false, key);
		UIManager.FindScript<ChapterGroup>().SetVisible(visible: false, key);
	}

	protected override void OnHide()
	{
		base.OnHide();
		string key = GetType().ToString();
		UIManager.FindScript<DialogueGroupBase>().SetVisible(visible: true, key, 0.2f);
		UIManager.FindScript<ChapterGroup>().SetVisible(visible: true, key, 0.2f);
	}

	protected override void OnTryConfirmOnModal()
	{
		MoveRight();
	}

	protected override void OnTryCancelOnModal()
	{
		Hide();
	}

	public bool Set()
	{
		_currentIndex = 0;
		_pageCount = 0;
		SpecialDeal[] specialDeals = GameSystem<ShopSystem>.Instance().SpecialDeals;
		if (specialDeals != null && specialDeals.Length > 0)
		{
			string freshSpecialDealId = GameSystem<ShopSystem>.Instance().FreshSpecialDealId;
			if (freshSpecialDealId != null)
			{
				for (int i = 0; i < specialDeals.Length; i++)
				{
					if (specialDeals[i].CommodityId == freshSpecialDealId)
					{
						_currentIndex = i;
						break;
					}
				}
			}
			_pageCount = specialDeals.Length;
			return true;
		}
		return false;
	}

	protected override void FillData()
	{
		_scroll.Nodes.BeginLoad();
		SpecialDeal[] specialDeals = GameSystem<ShopSystem>.Instance().SpecialDeals;
		if (specialDeals != null)
		{
			SpecialDeal[] array = specialDeals;
			foreach (SpecialDeal deal in array)
			{
				GameObject next = _scroll.Nodes.GetNext();
				SpecialDealCommodityWidget component = next.GetComponent<SpecialDealCommodityWidget>();
				if (component != null)
				{
					component.Set(deal);
				}
			}
		}
		_scroll.Nodes.EndLoad();
		_pageIndex.Make(_pageCount);
		RefreshButtonAndTexts();
	}

	protected override void UpdateLayout()
	{
		_scroll.ResetPosition();
		_scroll.MoveToNode(_currentIndex, instant: true);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void RefreshButtonAndTexts()
	{
		SpecialDealCommodityWidget specialDealCommodityWidget = GetSpecialDealCommodityWidget(_currentIndex);
		if (specialDealCommodityWidget != null)
		{
			Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(specialDealCommodityWidget.SpecialDeal.CommodityId);
			if (commodity != null)
			{
				_buttonBuyNow.Text = string.Format("{0}  [preset=round_box?{1}]", T._("구매"), commodity.GetCurrencyText(hasDiscountRatio: false));
				_buttonBuyNow.Disabled = false;
			}
			else
			{
				_buttonBuyNow.Text = T._("구매");
				_buttonBuyNow.Disabled = true;
			}
			if (specialDealCommodityWidget.SpecialDealBanner != null)
			{
				_textWarning.text = specialDealCommodityWidget.SpecialDealBanner.WarningDescription;
				_tweenerPlayer.gameObject.SetActive(value: true);
				_tweenerPlayer.Play();
			}
			else
			{
				_tweenerPlayer.gameObject.SetActive(value: false);
			}
		}
	}

	private SpecialDealCommodityWidget GetSpecialDealCommodityWidget(int index)
	{
		if (0 <= index && index < _scroll.Nodes.Count)
		{
			GameObject gameObject = _scroll.Nodes[index];
			return gameObject.GetComponent<SpecialDealCommodityWidget>();
		}
		return null;
	}

	private void MoveLeft()
	{
		if (_buttonPrevious.gameObject.activeInHierarchy)
		{
			_currentIndex = _scroll.GetGoalNodeIndex() - 1;
			_scroll.MoveToNode(_currentIndex, instant: false);
			RefreshButtonAndTexts();
		}
	}

	private void MoveRight()
	{
		if (_buttonNext.gameObject.activeInHierarchy)
		{
			_currentIndex = _scroll.GetGoalNodeIndex() + 1;
			_scroll.MoveToNode(_currentIndex, instant: false);
			RefreshButtonAndTexts();
		}
	}

	private void Scroll_DragFinshed()
	{
		int goalNodeIndex = _scroll.GetGoalNodeIndex();
		if (_currentIndex != goalNodeIndex)
		{
			_currentIndex = goalNodeIndex;
			RefreshButtonAndTexts();
		}
	}

	private void ButtonPrevious_Clicked()
	{
		MoveLeft();
	}

	private void ButtonNext_Clicked()
	{
		MoveRight();
	}

	private void ButtonAfterwards_Clicked()
	{
		GameSystem<ShopSystem>.Instance().FreshSpecialDealId = null;
		Hide();
	}

	private void ButtonBuyNow_Clicked()
	{
		SpecialDealCommodityWidget widget = GetSpecialDealCommodityWidget(_currentIndex);
		if (widget != null)
		{
			GameSystem<ShopSystem>.Instance().GetPurchasableCommodities(delegate
			{
				UIManager.FindScript<ShopGroup>().Open(widget.SpecialDeal.CommodityId, select: true);
			}, immediately: true);
		}
		Hide();
	}

	private void ShopSystem_SpecialDealsUpdated()
	{
		if (base.IsVisible)
		{
			if (Set())
			{
				Refresh();
			}
			else
			{
				Hide();
			}
		}
	}
}

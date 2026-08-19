using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class SubCommoditiesPopup : TooltipBase
{
	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subTitleLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UIWidget _subCommodityContainer;

	[SerializeField]
	private KScrollView _subCommodityList;

	private Durango.Logic.Shop.Purchase _purchase;

	private Commodity _commodity;

	private bool _reset = true;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_subCommodityList.Nodes.Init(delegate(GameObject obj)
		{
			SubCommodityItem component = obj.GetComponent<SubCommodityItem>();
			component.Received += OnSubCommodityReceive;
		});
		UIEventListener uIEventListener = UIEventListener.Get(_titleWidget.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Hide();
		});
	}

	protected override void OnShow()
	{
		base.OnShow();
		GameSystem<ShopSystem>.Instance().AcceptableSubPurchasesUpdated += base.Refresh;
	}

	protected override void OnHide()
	{
		base.OnHide();
		GameSystem<ShopSystem>.Instance().AcceptableSubPurchasesUpdated -= base.Refresh;
		_reset = true;
	}

	public void Set([NotNull] Durango.Logic.Shop.Purchase purchase)
	{
		_purchase = purchase;
		_commodity = GameSystem<ShopSystem>.Instance().GetCommodity(_purchase.CommodityId);
	}

	protected override void FillData()
	{
		ItemIcon icon = _commodity.GetIcon(large: false);
		if (icon.Colors.Count > 1)
		{
			_iconTexture.SetIcon(icon);
			_iconSprite.gameObject.SetActive(value: false);
			_iconTexture.gameObject.SetActive(value: true);
		}
		else
		{
			_iconSprite.spriteName = icon.Main;
			_iconSprite.color = ((!icon.Colors.HasValue) ? Color.white : icon.Colors[0]);
			_iconSprite.gameObject.SetActive(value: true);
			_iconTexture.gameObject.SetActive(value: false);
		}
		_titleLabel.text = _commodity.Title;
		_subTitleLabel.text = _commodity.Description;
		_subCommodityList.Nodes.BeginLoad();
		if (_commodity.SubCommodities != null)
		{
			bool flag = true;
			AcceptableSubPurchase? acceptableSubPurchase = GameSystem<ShopSystem>.Instance().GetAcceptableSubPurchase(_purchase.Id);
			foreach (Commodity subCommodity in _commodity.SubCommodities)
			{
				bool flag2 = acceptableSubPurchase.HasValue && acceptableSubPurchase.Value.IsAcceptable(subCommodity.Id);
				bool hasValue = _purchase.GetSubAcceptedAt(subCommodity.Id).HasValue;
				SubCommodityItem component = _subCommodityList.Nodes.GetNext().GetComponent<SubCommodityItem>();
				component.Set(subCommodity);
				if (hasValue)
				{
					component.SetAccepted();
				}
				else if (flag2)
				{
					if (flag)
					{
						component.SetFirstAcceptable();
					}
					else
					{
						component.SetAcceptable();
					}
				}
				else
				{
					component.SetNonAcceptable();
				}
				component.UpdateLayout();
				if (flag2)
				{
					flag = false;
				}
			}
		}
		_subCommodityList.Nodes.EndLoad();
		float? num = null;
		for (int i = 0; i < _subCommodityList.Nodes.Count; i++)
		{
			SubCommodityItem component2 = _subCommodityList.Nodes[i].GetComponent<SubCommodityItem>();
			UIWidget component3 = component2.GetComponent<UIWidget>();
			if (num.HasValue)
			{
				component2.SetGaugeHeight(num.Value * 0.5f - 10f + (float)component3.height * 0.5f - 10f);
			}
			else
			{
				component2.SetGaugeHeight(0f);
			}
			num = component3.height;
		}
	}

	protected override void UpdateLayout()
	{
		int safeWidth = UIManager.SafeWidth;
		int safeHeight = UIManager.SafeHeight;
		safeWidth = Mathf.Min(safeWidth - 200, 650);
		safeHeight -= 120;
		_layout.UpdateLayout(safeWidth, safeHeight);
		_subCommodityList.UpdateLayout();
		float num = (float)_subCommodityContainer.height - _subCommodityList.ContentsLength;
		if (num > 0f)
		{
			_layout.UpdateLayout(safeWidth, (float)safeHeight - num);
		}
		if (_reset)
		{
			_subCommodityList.MoveTo(0f, instant: true);
		}
		else
		{
			_subCommodityList.MoveTo(_subCommodityList.CurrentOffset, instant: false);
		}
		_reset = false;
	}

	private void OnSubCommodityReceive(string subId)
	{
		string key = ShopGroup.ToSubPurchaseKey(_purchase.Id, subId);
		UIManager.Alarm.HideNotify(key, major: false);
		GameSystem<ShopSystem>.Instance().AcceptSubPurchase(_purchase.Id, subId, delegate(bool success)
		{
			if (success)
			{
				Commodity commodity = null;
				List<Commodity> subCommodities = _commodity.SubCommodities;
				int i = 0;
				for (int size = KUtility.GetSize(subCommodities); i < size; i++)
				{
					if (subCommodities[i].Id == subId)
					{
						commodity = subCommodities[i];
					}
				}
				if (commodity != null)
				{
					ReceiveRewardsPopup receiveRewardsPopup = UIManager.Popup.Tooltip<ReceiveRewardsPopup>();
					receiveRewardsPopup.ShowCommodityRewarded(commodity);
				}
			}
		});
	}
}

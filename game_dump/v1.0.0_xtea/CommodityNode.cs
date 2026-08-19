using System;
using ItemSystem;
using L10N;
using MarketData;
using Messages;
using Player;
using TimerData;
using UnityEngine;

public class CommodityNode : SelectableWidget
{
	[SerializeField]
	private ItemIconTex _itemIconTex;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _prototypeLabel;

	[SerializeField]
	private UISpriteLabel _durabilityLabel;

	[SerializeField]
	private UISpriteLabel _timeLabel;

	[SerializeField]
	private UISpriteLabel _priceLabel;

	[SerializeField]
	private UISpriteLabel _sellerLabel;

	[SerializeField]
	private UISpriteLabel _distanceLabel;

	[SerializeField]
	private UILabel _regionLabel;

	private UIWidget _itemWidget;

	private string _nameFormat;

	private string _prototypeFormat;

	private string _durabilityFormat;

	private string _timeFormat;

	private string _priceFormat;

	private string _sellerFormat;

	private string _distanceFormat;

	private string _regionFormat;

	public Commodity Data { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		if ((Object)(object)_nameLabel != (Object)null)
		{
			_nameFormat = _nameLabel.text;
			_itemWidget = ((Component)((Component)_nameLabel).transform.parent).GetComponent<UIWidget>();
			if ((Object)(object)_itemWidget != (Object)null)
			{
				UIEventListener uIEventListener = UIEventListener.Get(((Component)_itemWidget).gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickItemArea));
			}
		}
		if ((Object)(object)_prototypeLabel != (Object)null)
		{
			_prototypeFormat = _prototypeLabel.text;
		}
		if ((Object)(object)_durabilityLabel != (Object)null)
		{
			_durabilityFormat = _durabilityLabel.text;
		}
		if ((Object)(object)_timeLabel != (Object)null)
		{
			_timeFormat = _timeLabel.text;
		}
		if ((Object)(object)_priceLabel != (Object)null)
		{
			_priceFormat = _priceLabel.text;
		}
		if ((Object)(object)_sellerLabel != (Object)null)
		{
			_sellerFormat = _sellerLabel.text;
		}
		if ((Object)(object)_distanceLabel != (Object)null)
		{
			_distanceFormat = _distanceLabel.text;
		}
		if ((Object)(object)_regionLabel != (Object)null)
		{
			_regionFormat = _regionLabel.text;
		}
	}

	public void Set(Commodity commodity)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		Init();
		Data = commodity;
		ItemData item = commodity.GetItem();
		if ((Object)(object)_itemIconTex != (Object)null)
		{
			_itemIconTex.SetIcon(item);
		}
		if ((Object)(object)_nameLabel != (Object)null)
		{
			Vector3 position = _nameLabel.GetPosition(0f, 0f);
			UIWidget component = ((Component)((Component)_nameLabel).transform.parent).GetComponent<UIWidget>();
			Vector3 val = component.localCorners[0];
			_nameLabel.width = component.width - (int)(position.x - val.x) - 20;
			_nameLabel.text = string.Format(_nameFormat, item.Name);
		}
		if ((Object)(object)_prototypeLabel != (Object)null)
		{
			_prototypeLabel.text = T.Format(_prototypeFormat, item.PrototypeName, item.Level);
		}
		if ((Object)(object)_durabilityLabel != (Object)null)
		{
			_durabilityLabel.text = string.Format(_durabilityFormat, item.Durability.Get().ToString("0.0"), item.Durability.Max().ToString("0.0"));
		}
		if ((Object)(object)_timeLabel != (Object)null)
		{
			string arg = TimerSystem.TimeToString(commodity.ExpireAt - Connections.Frontend.GetPredictedServerTime(), TimePeriod.Min, 1);
			_timeLabel.text = string.Format(_timeFormat, arg);
		}
		if ((Object)(object)_priceLabel != (Object)null)
		{
			_priceLabel.text = string.Format(_priceFormat, ItemSystem.Inventory.CurrencyFormat(commodity.Price, commodity.CurrencyType));
		}
		if ((Object)(object)_sellerLabel != (Object)null)
		{
			_sellerLabel.text = string.Empty;
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(commodity.SellerId, OnResponseSellerInfo, useOldCache: true);
		}
		if ((Object)(object)_distanceLabel != (Object)null)
		{
			_distanceLabel.text = string.Empty;
			GameSystem<MarketSystem>.Instance().GetMarket(commodity.MarketId, OnResponseMarketInfo);
		}
		if ((Object)(object)_regionLabel != (Object)null)
		{
			_regionLabel.text = string.Empty;
			GameSystem<MapSystem>.Instance().GetRegion(commodity.RegionId, OnResponseRegionInfo);
		}
		if ((Object)(object)_nameLabel != (Object)null && (Object)(object)_prototypeLabel != (Object)null)
		{
			Vector3 position2 = _nameLabel.GetPosition(0f, 1f);
			Vector3 position3 = _prototypeLabel.GetPosition(0f, 0f);
			Vector3 val2 = Vector3.Lerp(position2, position3, 0.5f);
			position2.y -= val2.y;
			position3.y -= val2.y;
			_nameLabel.SetPosition(position2, 0f, 1f);
			_prototypeLabel.SetPosition(position3, 0f, 0f);
		}
	}

	private void OnResponseSellerInfo(Player.PlayerInfo playerInfo)
	{
		_sellerLabel.text = string.Format(_sellerFormat, playerInfo.Name);
	}

	private void OnResponseMarketInfo(Market market)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (market.Id != 0L)
		{
			Point2 currentTile = PlayerBehavior.LocalPlayer.CurrentTile;
			Point2 tile = market.Tile;
			Vector2 val = (tile - currentTile).ToVector2();
			float magnitude = ((Vector2)(ref val)).magnitude;
			_distanceLabel.text = string.Format(_distanceFormat, Mathf.RoundToInt(magnitude * 2f));
		}
	}

	private void OnResponseRegionInfo(Region region)
	{
		if (region.Id != 0L)
		{
			_regionLabel.text = string.Format(_regionFormat, region.Name);
		}
	}

	private void OnClickItemArea(GameObject obj)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SendMessage("OnClick");
		if (base.Select)
		{
			ItemData itemData = ((Data != null) ? Data.GetItem() : null);
			if (itemData != null)
			{
				ItemInfoPopup itemInfoPopup = UIManager.Popup.Tooltip<ItemInfoPopup>();
				itemInfoPopup.Set(itemData);
				itemInfoPopup.Direction = TooltipBase.TooltipDirection.Horizontal;
				itemInfoPopup.Show(_itemWidget, Vector2.zero, 3600f);
			}
		}
	}
}

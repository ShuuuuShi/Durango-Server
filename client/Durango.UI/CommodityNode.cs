using System;
using Durango.Logic.Clusters;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Market;
using UnityEngine;

namespace Durango.UI;

public class CommodityNode : SelectableWidget, IScreenResizeReceiver
{
	[SerializeField]
	private ItemIconTex _itemIconTex;

	[SerializeField]
	private ItemGradeViewer _itemGradeViewer;

	[SerializeField]
	private ItemModifiedCountViewer _itemModifiedCountViewer;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _prototypeLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _durabilityLabel;

	[SerializeField]
	private UISpriteLabel _priceLabel;

	[SerializeField]
	private SelectableButton _receiveButton;

	[SerializeField]
	private UILabel _durationLabel;

	[SerializeField]
	private UISprite _stateSprite;

	[SerializeField]
	private GameObject _receiveColumn;

	[SerializeField]
	private RectLayout _layout;

	[SerializeField]
	private GameObject[] _onlyLandscape;

	[SerializeField]
	private GameObject[] _onlyOnline;

	private UIWidget _itemWidget;

	public Commodity Data { get; private set; }

	protected override void OnInit()
	{
		base.OnInit();
		if (_nameLabel != null)
		{
			_itemWidget = _nameLabel.transform.parent.GetComponent<UIWidget>();
			if (_itemWidget != null)
			{
				UIEventListener uIEventListener = UIEventListener.Get(_itemWidget.gameObject);
				uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickItemArea));
			}
		}
		if (_receiveButton != null)
		{
			SelectableButton receiveButton = _receiveButton;
			receiveButton.Clicked = (Action)Delegate.Combine(receiveButton.Clicked, new Action(ReceiveButton_Clicked));
		}
		UpdateItemsOnScreenChanged();
		UpdateItemsOnOnline();
		_layout.UpdateOnSizeChange(delegate
		{
			UIUtility.UpdateAnchors(base.transform);
		});
	}

	public void Set(Commodity commodity, ProductType productType)
	{
		Init();
		Data = commodity;
		ItemData item = commodity.GetItem();
		if (item == null)
		{
			return;
		}
		if (_itemIconTex != null)
		{
			_itemIconTex.SetIcon(item);
		}
		if (_itemGradeViewer != null)
		{
			_itemGradeViewer.Set(item);
		}
		if (_itemModifiedCountViewer != null)
		{
			_itemModifiedCountViewer.Set(item.ModifiedCount);
		}
		if (_nameLabel != null)
		{
			Vector3 position = _nameLabel.GetPosition(0f, 0f);
			UIWidget component = _nameLabel.transform.parent.GetComponent<UIWidget>();
			Vector3 vector = component.localCorners[0];
			_nameLabel.overflowWidth = component.width - (int)(position.x - vector.x) - 20;
			_nameLabel.text = ((!item.Pet.HasValue) ? item.Name : item.Pet.Value.GetPetName(includeRank: true));
		}
		if (_prototypeLabel != null)
		{
			_prototypeLabel.text = ((!(_levelLabel == null)) ? item.PrototypeName : T._("{0} {1:lv:}", item.PrototypeName, item.Level));
		}
		if (_levelLabel != null)
		{
			_levelLabel.text = T._("{0:lv:}", item.Level);
		}
		if (_durabilityLabel != null)
		{
			_durabilityLabel.text = Util.LocalizedDurability(item.Durability.Get(), item.Durability.Max());
		}
		if (_priceLabel != null)
		{
			if (productType == ProductType.Sold)
			{
				string arg = Durango.Logic.Item.Inventory.CurrencyFormat(commodity.Price, commodity.CurrencyType);
				string salesFeeTooltip = GetSalesFeeTooltip(T._("가격"), T._("판매 수수료"), T._("판매 수익"), commodity.Price, commodity.Fee, commodity.Price - commodity.Fee);
				_priceLabel.text = $"{arg} <help>{salesFeeTooltip}</help>";
			}
			else
			{
				_priceLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(commodity.Price, commodity.CurrencyType);
			}
		}
		if (_durationLabel != null)
		{
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			string text;
			switch (productType)
			{
			case ProductType.Purchased:
			case ProductType.Sold:
				if (commodity.PurchasedAt.HasValue)
				{
					double num = predictedServerTime - commodity.PurchasedAt.Value;
					text = ((!(num < 60.0)) ? ((!(num < 3600.0)) ? T._("{0} 전", TimedeltaFormatter.Format(num, 2, "hour")) : T._("{0} 전", TimedeltaFormatter.Format(num, 2, "min"))) : T._("방금"));
					break;
				}
				_durationLabel.text = T._("방금");
				return;
			case ProductType.Expired:
			{
				double num = commodity.DeletesAt - predictedServerTime;
				text = ((!(num < 0.0)) ? TimedeltaFormatter.Format(num, 2, "hour") : T._("만료"));
				break;
			}
			default:
			{
				double num = commodity.ExpireAt - predictedServerTime;
				text = ((!(num < 0.0)) ? TimedeltaFormatter.Format(num, 2, "hour") : T._("만료"));
				break;
			}
			}
			_durationLabel.text = text;
		}
		if (_nameLabel != null && _prototypeLabel != null)
		{
			Vector3 position2 = _nameLabel.GetPosition(0f, 1f);
			Vector3 position3 = _prototypeLabel.GetPosition(0f, 0f);
			Vector3 vector2 = Vector3.Lerp(position2, position3, 0.5f);
			position2.y -= vector2.y;
			position3.y -= vector2.y;
			_nameLabel.SetPosition(position2, 0f, 1f);
			_prototypeLabel.SetPosition(position3, 0f, 0f);
		}
		if (_stateSprite != null)
		{
			if (GameSystem<MarketSystem>.Instance().IsFavorite(commodity.Id))
			{
				_stateSprite.spriteName = "faction_heart";
				_stateSprite.color = new Color(1f, 1f, 1f, 0.75f);
				_stateSprite.gameObject.SetActive(value: true);
			}
			else
			{
				_stateSprite.gameObject.SetActive(value: false);
			}
		}
		if (_receiveColumn != null)
		{
			bool flag = productType == ProductType.Sold;
			if (_receiveColumn.activeSelf != flag)
			{
				_receiveColumn.SetActive(flag);
				_layout.UpdateLayout();
				UIUtility.UpdateAnchors(base.transform);
			}
			if (flag)
			{
				RefreshReceiveButton();
			}
		}
	}

	private static string GetSalesFeeTooltip(string priceText, string feeText, string resultText, long price, long fee, long result)
	{
		return string.Format("<kv>key={0},value={3}</kv>\n<kv>key={1}, value={4}</kv>\n<hr>\n<kv>key=<em>{2}</em>, value=<em>{5}</em></kv>", priceText, feeText, resultText, price, fee, result);
	}

	public void RefreshReceiveButton()
	{
		if (_receiveButton != null)
		{
			_receiveButton.Disabled = Data.State != ProductState.PaymentPending;
		}
	}

	private void OnClickItemArea(GameObject obj)
	{
		base.gameObject.SendMessage("OnClick");
		if (base.Selected)
		{
			ItemData itemData = ((Data != null) ? Data.GetItem() : null);
			if (itemData != null)
			{
				ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
				itemInfoTooltip.Set(itemData);
				itemInfoTooltip.Direction = TooltipBase.TooltipDirection.Horizontal;
				itemInfoTooltip.Show(_itemWidget, Vector2.zero, 3600f);
			}
		}
	}

	private void ReceiveButton_Clicked()
	{
		MarketCollectPayment msg = default(MarketCollectPayment);
		msg.ProductId = Data.Id;
		MarketSystem.Send(msg);
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		UpdateItemsOnScreenChanged();
	}

	private void UpdateItemsOnScreenChanged()
	{
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		int i = 0;
		for (int size = KUtility.GetSize(_onlyLandscape); i < size; i++)
		{
			_onlyLandscape[i].SetActive(!flag);
		}
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void UpdateItemsOnOnline()
	{
		bool active = GameManager.ClusterMode == Mode.Online;
		int i = 0;
		for (int size = KUtility.GetSize(_onlyOnline); i < size; i++)
		{
			_onlyOnline[i].SetActive(active);
		}
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}

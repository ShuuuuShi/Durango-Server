using System;
using System.Collections.Generic;
using Durango.Network;
using JetBrains.Annotations;
using Messages;

namespace Durango.Logic.Market;

public class Commodities
{
	public readonly RequestOption Request = new RequestOption();

	public readonly List<Commodity> Goods = new List<Commodity>();

	[CanBeNull]
	public SearchOption SearchOption;

	public event Action GoodsListUpdated;

	public event Action<bool> OnRequestGoodsList;

	public void Reset()
	{
		Goods.Clear();
		Request.Index = 0;
		Request.NoMore = false;
		Request.IsLoading = false;
	}

	public void Get(bool reset)
	{
		if (reset)
		{
			Reset();
		}
		if (Request.NoMore || Request.IsLoading)
		{
			return;
		}
		int index = Request.Index;
		Request.Index += OptionSystem.GetMarketSearchLimit();
		Request.IsLoading = true;
		UIManager.ShowLoadingIcon(!reset);
		if (this.OnRequestGoodsList != null)
		{
			this.OnRequestGoodsList(reset);
		}
		ReplyMessageHandlerRegistrar wrappedMessage = CreateGetProductMessage(Request.Type, Request.Condition, index);
		GameSystem<MarketSystem>.Instance().GetProducts(wrappedMessage, delegate(Products? product)
		{
			if (product.HasValue)
			{
				OnResult(product.Value);
			}
		});
	}

	public ReplyMessageHandlerRegistrar CreateGetProductMessage(ProductType type, SortCondition condition, int pageIndex)
	{
		switch (type)
		{
		case ProductType.Searched:
		{
			SearchProducts msg5 = ((SearchOption != null) ? SearchOption.ToMessage() : default(SearchProducts));
			msg5.Skip = pageIndex;
			msg5.Sort = condition;
			return MarketSystem.Send(msg5);
		}
		case ProductType.Registered:
		{
			GetRegisteredProducts msg4 = default(GetRegisteredProducts);
			msg4.Skip = pageIndex;
			msg4.Sort = condition;
			return MarketSystem.Send(msg4);
		}
		case ProductType.Purchased:
		{
			GetPurchasedProducts msg3 = default(GetPurchasedProducts);
			msg3.Skip = pageIndex;
			msg3.Sort = condition;
			return MarketSystem.Send(msg3);
		}
		case ProductType.Sold:
		{
			GetSoldProducts msg2 = default(GetSoldProducts);
			msg2.Skip = pageIndex;
			msg2.Sort = condition;
			return MarketSystem.Send(msg2);
		}
		case ProductType.Expired:
		{
			GetExpiredProducts msg = default(GetExpiredProducts);
			msg.Skip = pageIndex;
			msg.Sort = condition;
			return MarketSystem.Send(msg);
		}
		case ProductType.Favorites:
			return MarketSystem.Send(default(GetFavoriteProducts));
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}

	private void OnResult(Products products)
	{
		Request.IsLoading = false;
		UIManager.ShowLoadingIcon(show: false);
		bool flag = products._Products == null || products._Products.Length == 0;
		if (!flag)
		{
			flag = true;
			for (int i = 0; i < products._Products.Length; i++)
			{
				string id = products._Products[i].Id;
				int num = -1;
				for (int j = 0; j < Goods.Count; j++)
				{
					if (Goods[j].Id == id)
					{
						num = j;
						break;
					}
				}
				if (num == -1)
				{
					Commodity commodity = new Commodity();
					commodity.Set(products._Products[i]);
					Goods.Add(commodity);
					flag = false;
				}
				else
				{
					Goods[num].Set(products._Products[i]);
				}
			}
		}
		if (flag)
		{
			if (Request.NoMore)
			{
				return;
			}
			Request.NoMore = true;
		}
		if (this.GoodsListUpdated != null)
		{
			this.GoodsListUpdated();
		}
	}

	public void Buy(Commodity item)
	{
		if (item == null || InventorySystem.Wallet.GetBalance(item.CurrencyType) < item.Price || item.IsWaiting())
		{
			return;
		}
		item.SetToWait();
		GameSystem<MarketSystem>.Instance().BuyCommodity(item.Id, delegate(bool success)
		{
			item.Responsed();
			if (success && Goods.Remove(item) && this.GoodsListUpdated != null)
			{
				this.GoodsListUpdated();
			}
		});
	}

	public void Unregister(Commodity item)
	{
		if (item == null || item.IsWaiting())
		{
			return;
		}
		item.SetToWait();
		GameSystem<MarketSystem>.Instance().UnregisterCommodity(item.Id, delegate(bool success)
		{
			item.Responsed();
			if (success && Goods.Remove(item) && this.GoodsListUpdated != null)
			{
				this.GoodsListUpdated();
			}
		});
	}

	public void Withdraw(Commodity item)
	{
		if (item == null || item.IsWaiting())
		{
			return;
		}
		item.SetToWait();
		GameSystem<MarketSystem>.Instance().WithdrawCommodity(item.Id, delegate(bool success)
		{
			item.Responsed();
			if (success && Goods.Remove(item))
			{
				if (Goods.Count == 0)
				{
					GameSystem<MarketSystem>.Instance().GetExpiredProduct();
				}
				if (this.GoodsListUpdated != null)
				{
					this.GoodsListUpdated();
				}
			}
		});
	}
}

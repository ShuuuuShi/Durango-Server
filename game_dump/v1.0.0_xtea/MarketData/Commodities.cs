using System;
using System.Collections.Generic;
using Messages;

namespace MarketData;

public class Commodities
{
	public int RequestCommodityCount = 10;

	public readonly FilterOption Filter = new FilterOption();

	public readonly RequestOption Request = new RequestOption();

	public readonly List<Commodity> Goods = new List<Commodity>();

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
		if (!Request.NoMore && !Request.IsLoading)
		{
			ulong regionId = 0uL;
			ulong marketId = 0uL;
			ulong sellerId = 0uL;
			switch (Request.Type)
			{
			case CommodityOwner.Region:
				regionId = Request.Id;
				break;
			case CommodityOwner.Market:
				marketId = Request.Id;
				break;
			case CommodityOwner.Seller:
				sellerId = Request.Id;
				break;
			}
			GameSystem<MarketSystem>.Instance().GetProducts(regionId, marketId, sellerId, Request.Index, RequestCommodityCount, Filter, OnResult);
			Request.Index += RequestCommodityCount;
			Request.IsLoading = true;
			UIManager.ShowLoadingIcon(!reset);
			if (this.OnRequestGoodsList != null)
			{
				this.OnRequestGoodsList(reset);
			}
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
				ulong id = products._Products[i].Id;
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
					Goods.Add(new Commodity(products._Products[i]));
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

	public void Buy(ulong entityId, Point2 tile, Commodity item)
	{
		GameSystem<MarketSystem>.Instance().BuyCommodity(entityId, tile, item, delegate
		{
			if (Goods.Remove(item) && this.GoodsListUpdated != null)
			{
				this.GoodsListUpdated();
			}
		});
	}

	public void Refund(ulong entityId, Point2 tile, Commodity item)
	{
		GameSystem<MarketSystem>.Instance().RefundCommodity(entityId, tile, item, delegate
		{
			if (Goods.Remove(item) && this.GoodsListUpdated != null)
			{
				this.GoodsListUpdated();
			}
		});
	}
}

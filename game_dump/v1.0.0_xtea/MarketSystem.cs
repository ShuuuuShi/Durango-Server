using System;
using ItemSystem;
using JetBrains.Annotations;
using K1Network;
using MarketData;
using Messages;
using Shared.Economy;

public class MarketSystem : GameSystem<MarketSystem>
{
	private AsyncCachedDictionary<ulong, Market> _marketDict;

	public event Action SuccessItemBuy;

	public event Action<ProductSold> OnProductSold;

	private void Awake()
	{
		_marketDict = new AsyncCachedDictionary<ulong, Market>(RequestMarket, 60f);
		Connections.Frontend.On<ProductSold>(OnCommoditySold);
	}

	private void RequestMarket(ulong id, Market cache, Action<ulong, Market> onResult)
	{
		Connections.Frontend.Send(new GetMarket
		{
			MarketId = id
		}).On(delegate(Market msg, PacketHeader header)
		{
			onResult(id, msg);
		}).On(delegate(Error msg, PacketHeader header)
		{
			onResult(id, default(Market));
			GameManager.DefaultErrorHandler(msg, header);
		});
	}

	public bool RegisterCommodity(ulong entityId, Point2 tile, ItemData item, int price, float duration)
	{
		if (item == null)
		{
			return false;
		}
		ulong[] itemIds = new ulong[1] { item.Id };
		RegisterProduct msg = default(RegisterProduct);
		msg.EntityId = entityId;
		msg.Tile = tile;
		msg.ItemIds = itemIds;
		msg.Price = price;
		msg.Duration = duration;
		Connections.Frontend.Send(msg);
		return true;
	}

	public void BuyCommodity(ulong entityId, Point2 tile, Commodity item, Action onSuccess)
	{
		if (item == null || GameSystem<InventorySystem>.Instance().PlayerInventory.GetBalance(item.CurrencyType) < item.Price)
		{
			return;
		}
		BuyProduct msg = default(BuyProduct);
		msg.EntityId = entityId;
		msg.Tile = tile;
		msg.ProductId = item.Id;
		Connections.Frontend.Send(msg).On<OK>(delegate
		{
			if (this.SuccessItemBuy != null)
			{
				this.SuccessItemBuy();
			}
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	public void RefundCommodity(ulong entityId, Point2 tile, Commodity item, Action onSuccess)
	{
		UnregisterProduct unregisterProduct = default(UnregisterProduct);
		unregisterProduct.EntityId = entityId;
		unregisterProduct.Tile = tile;
		unregisterProduct.ProductId = item.Id;
		UnregisterProduct msg = unregisterProduct;
		Connections.Frontend.Send(msg).On<OK>(delegate
		{
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	private void OnCommoditySold(ProductSold msg, PacketHeader header)
	{
		if (this.OnProductSold != null)
		{
			this.OnProductSold(msg);
		}
	}

	public void GetProducts(ulong regionId, ulong marketId, ulong sellerId, int skip, int limit, FilterOption filter, Action<Products> onResult)
	{
		GetProducts msg = default(GetProducts);
		if (regionId != 0)
		{
			msg.RegionId = regionId;
		}
		else if (marketId != 0)
		{
			msg.MarketId = marketId;
		}
		else
		{
			if (sellerId == 0)
			{
				throw new ArgumentException("No Have Entity Id");
			}
			msg.SellerId = sellerId;
		}
		msg.Skip = skip;
		msg.Limit = limit;
		int i = 0;
		for (int size = KUtility.GetSize(filter.Prototype); i < size; i++)
		{
			if (msg.PrototypeIds == null)
			{
				msg.PrototypeIds = new string[size];
			}
			msg.PrototypeIds[i] = filter.Prototype[i].Key;
		}
		if (filter.Level.Min > 1 || filter.Level.Max > 0)
		{
			RangePredicate value = default(RangePredicate);
			if (filter.Level.Min > 1)
			{
				value.Min = filter.Level.Min;
			}
			if (filter.Level.Max > 0)
			{
				value.Max = filter.Level.Max;
			}
			msg.Level = value;
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(filter.Tags); j < size2; j++)
		{
			if (msg.MajorTags == null)
			{
				msg.MajorTags = new string[size2];
			}
			msg.MajorTags[j] = filter.Tags[j].Key;
		}
		if (filter.Currency.Min > 1 || filter.Currency.Max > 0)
		{
			PriceRangePredicate value2 = default(PriceRangePredicate);
			value2.Currency = Currency.TStone;
			if (filter.Currency.Min > 1)
			{
				value2.Min = filter.Currency.Min;
			}
			if (filter.Currency.Max > 0)
			{
				value2.Max = filter.Currency.Max;
			}
			msg.Price = value2;
		}
		Connections.Frontend.Send(msg).On(delegate(Products products, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(products);
			}
		});
	}

	public void GetSimilarProducts(ItemData item, int count, [NotNull] Action<ItemData, Commodity[]> callback)
	{
		GetSimilarProducts getSimilarProducts = default(GetSimilarProducts);
		getSimilarProducts.PrototypeId = item.RawPrototypename;
		getSimilarProducts.Level = item.Level;
		getSimilarProducts.Limit = count;
		GetSimilarProducts msg = getSimilarProducts;
		int i = 0;
		for (int size = KUtility.GetSize(item.Tags); i < size; i++)
		{
			if (msg.MajorTags == null)
			{
				msg.MajorTags = new string[size];
			}
			msg.MajorTags[i] = item.Tags[i].Id;
		}
		Connections.Frontend.Send(msg).On(delegate(Products products, PacketHeader header)
		{
			Commodity[] array = null;
			int j = 0;
			for (int size2 = KUtility.GetSize(products._Products); j < size2; j++)
			{
				if (array == null)
				{
					array = new Commodity[products._Products.Length];
				}
				array[j] = new Commodity(products._Products[j]);
			}
			callback(item, array);
		});
	}

	public void GetMarket(ulong id, [NotNull] Action<Market> onMarket)
	{
		if (id != 0L)
		{
			_marketDict.Request(id, onMarket);
		}
	}

	public void GetPlayersMarkets(ulong ownerId, [NotNull] Action<Markets> onResult)
	{
		Connections.Frontend.Send(new GetMarkets
		{
			SellerId = ownerId
		}).On(delegate(Markets msg, PacketHeader header)
		{
			int i = 0;
			for (int size = KUtility.GetSize(msg._Markets); i < size; i++)
			{
				_marketDict.SetValue(msg._Markets[i].Id, msg._Markets[i]);
			}
			onResult(msg);
		}).On<Error>(delegate
		{
			onResult(new Markets
			{
				_Markets = null
			});
		});
	}
}

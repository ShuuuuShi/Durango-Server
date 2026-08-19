using ItemSystem;
using Messages;
using Shared.Economy;
using Shared.Market;

namespace MarketData;

public class Commodity
{
	public ulong Id;

	public ulong SellerId;

	public ulong RegionId;

	public ulong MarketId;

	public ItemData[] Items;

	public double RegisteredAt;

	public double ExpireAt;

	public int Price;

	public Currency CurrencyType;

	public ProductState State;

	public Commodity(Product msg)
	{
		Set(msg);
	}

	public void Set(Product msg)
	{
		Id = msg.Id;
		SellerId = msg.SellerId;
		RegionId = msg.RegionId;
		MarketId = msg.MarketId;
		RegisteredAt = msg.ListedAt;
		ExpireAt = msg.ExpiresAt;
		Price = msg.Price.Amount;
		CurrencyType = msg.Price.Currency;
		State = msg.State;
		if (Items == null || Items.Length != msg.Items.Length)
		{
			Items = new ItemData[msg.Items.Length];
		}
		for (int i = 0; i < Items.Length; i++)
		{
			if (Items[i] == null)
			{
				Items[i] = new ItemData(msg.Items[i]);
			}
			else
			{
				Items[i].Set(msg.Items[i]);
			}
		}
	}

	public ItemData GetItem()
	{
		return Items[0];
	}
}

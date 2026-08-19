using Durango.Logic.Item;
using JetBrains.Annotations;
using Messages;
using Shared.Economy;
using Shared.Market;
using UnityEngine;

namespace Durango.Logic.Market;

public class Commodity
{
	public string Id;

	public string RegionId;

	public ItemData[] Items;

	public double DeletesAt;

	public double RegisteredAt;

	public double ExpireAt;

	public double? PurchasedAt;

	public long Price;

	public long Fee;

	public Currency CurrencyType;

	public ProductState State;

	private float _requestAt;

	public void Set(Product msg)
	{
		Id = msg.Id;
		RegionId = msg.RegionId;
		RegisteredAt = msg.ListedAt;
		ExpireAt = msg.ExpiresAt;
		DeletesAt = msg.DeletesAt;
		PurchasedAt = msg.PurchasedAt;
		Price = msg.Price;
		Fee = msg.Fee;
		CurrencyType = msg.Currency;
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

	[CanBeNull]
	public ItemData GetItem()
	{
		if (KUtility.GetSize(Items) > 0)
		{
			return Items[0];
		}
		return null;
	}

	public bool IsWaiting()
	{
		if (_requestAt > 0f)
		{
			if (!(_requestAt + 5f < Time.time))
			{
				return true;
			}
			_requestAt = 0f;
		}
		return false;
	}

	public void SetToWait()
	{
		_requestAt = Time.time;
	}

	public void Responsed()
	{
		_requestAt = 0f;
	}
}

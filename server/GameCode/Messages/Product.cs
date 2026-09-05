using MsgPack;
using Shared.Economy;
using Shared.Market;

namespace Messages;

public struct Product
{
	public string Id;

	public string RegionId;

	public double ListedAt;

	public double ExpiresAt;

	public double DeletesAt;

	public double? PurchasedAt;

	public long Price;

	public long Fee;

	public Currency Currency;

	public Item[] Items;

	public ProductState State;

	public int Level;

	public float Durability;

	public static void Pack(Packer packer, Product val, bool hint = false)
	{
		packer.PackArrayHeader(13);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.Pack(val.ListedAt);
		packer.Pack(val.ExpiresAt);
		packer.Pack(val.DeletesAt);
		if (!val.PurchasedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PurchasedAt.Value);
		}
		packer.Pack(val.Price);
		packer.Pack(val.Fee);
		packer.Pack((int)val.Currency);
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int i = 0; i < val.Items.Length; i++)
			{
				Item.Pack(packer, val.Items[i]);
			}
		}
		packer.Pack((int)val.State);
		packer.Pack(val.Level);
		packer.Pack(val.Durability);
	}

	public static Product Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Product result = default(Product);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ListedAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.ExpiresAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.DeletesAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PurchasedAt = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.PurchasedAt = value;
		}
		unpacker.Read();
		result.Price = unpacker.LastReadData.AsInt64();
		unpacker.Read();
		result.Fee = unpacker.LastReadData.AsInt64();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 7 < num)
		{
			result.Currency = Currency.Invalid;
		}
		else
		{
			result.Currency = (Currency)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Items = new Item[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		if (num3 < 1 || 11 < num3)
		{
			result.State = ProductState.Invalid;
		}
		else
		{
			result.State = (ProductState)num3;
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Durability = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Product Id={Id} RegionId={RegionId} ListedAt={ListedAt} ExpiresAt={ExpiresAt} DeletesAt={DeletesAt} PurchasedAt={PurchasedAt} Price={Price} Fee={Fee} Currency={Currency} Items={Items} State={State} Level={Level} Durability={Durability}>";
	}
}

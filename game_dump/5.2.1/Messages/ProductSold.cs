using MsgPack;

namespace Messages;

public struct ProductSold
{
	public const uint TypeCode = 2427u;

	public Item Item;

	public long Price;

	public static void Pack(Packer packer, ProductSold val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2427u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		Item.Pack(packer, val.Item);
		packer.Pack(val.Price);
	}

	public static ProductSold Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ProductSold result = default(ProductSold);
		result.Item = Item.Unpack(unpacker);
		unpacker.Read();
		result.Price = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ProductSold Item={Item} Price={Price}>";
	}
}

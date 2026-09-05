using MsgPack;

namespace Messages;

public struct Products
{
	public const uint TypeCode = 5100u;

	public Product[] _Products;

	public static void Pack(Packer packer, Products val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5100u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Products == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Products.Length);
		for (int i = 0; i < val._Products.Length; i++)
		{
			Product.Pack(packer, val._Products[i]);
		}
	}

	public static Products Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Products result = default(Products);
		result._Products = new Product[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Product reference = ref result._Products[i];
			reference = Product.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Products _Products={_Products}>";
	}
}

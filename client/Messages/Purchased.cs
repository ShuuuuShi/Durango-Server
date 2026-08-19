using MsgPack;

namespace Messages;

public struct Purchased
{
	public const uint TypeCode = 856711u;

	public Purchase[] Purchases;

	public static void Pack(Packer packer, Purchased val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(856711u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Purchases == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Purchases.Length);
		for (int i = 0; i < val.Purchases.Length; i++)
		{
			Purchase.Pack(packer, val.Purchases[i]);
		}
	}

	public static Purchased Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Purchased result = default(Purchased);
		result.Purchases = new Purchase[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Purchase reference = ref result.Purchases[i];
			reference = Purchase.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Purchased Purchases={Purchases}>";
	}
}

using MsgPack;

namespace Messages;

public struct Purchases
{
	public const uint TypeCode = 510398u;

	public Purchase[] _Purchases;

	public static void Pack(Packer packer, Purchases val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(510398u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Purchases == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Purchases.Length);
		for (int i = 0; i < val._Purchases.Length; i++)
		{
			Purchase.Pack(packer, val._Purchases[i]);
		}
	}

	public static Purchases Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Purchases result = default(Purchases);
		result._Purchases = new Purchase[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Purchase reference = ref result._Purchases[i];
			reference = Purchase.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Purchases _Purchases={_Purchases}>";
	}
}

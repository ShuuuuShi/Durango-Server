using MsgPack;

namespace Messages;

public struct AcceptableSubPurchases
{
	public const uint TypeCode = 259675u;

	public AcceptableSubPurchase[] Ids;

	public static void Pack(Packer packer, AcceptableSubPurchases val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(259675u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Ids == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Ids.Length);
		for (int i = 0; i < val.Ids.Length; i++)
		{
			AcceptableSubPurchase.Pack(packer, val.Ids[i]);
		}
	}

	public static AcceptableSubPurchases Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AcceptableSubPurchases result = default(AcceptableSubPurchases);
		result.Ids = new AcceptableSubPurchase[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref AcceptableSubPurchase reference = ref result.Ids[i];
			reference = AcceptableSubPurchase.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptableSubPurchases Ids={Ids}>";
	}
}

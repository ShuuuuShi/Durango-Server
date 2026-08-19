using MsgPack;

namespace Messages;

public struct Markets
{
	public const uint TypeCode = 5101u;

	public Market[] _Markets;

	public static void Pack(Packer packer, Markets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5101u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val._Markets == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val._Markets.Length);
		for (int i = 0; i < val._Markets.Length; i++)
		{
			Market.Pack(packer, val._Markets[i]);
		}
	}

	public static Markets Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Markets result = default(Markets);
		result._Markets = new Market[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Market reference = ref result._Markets[i];
			reference = Market.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Markets _Markets={_Markets}>";
	}
}

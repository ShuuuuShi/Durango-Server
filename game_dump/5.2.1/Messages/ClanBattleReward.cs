using MsgPack;

namespace Messages;

public struct ClanBattleReward
{
	public const uint TypeCode = 2340979u;

	public float TaxRate;

	public int TotalTax;

	public int ReceivableTax;

	public static void Pack(Packer packer, ClanBattleReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2340979u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.TaxRate);
		packer.Pack(val.TotalTax);
		packer.Pack(val.ReceivableTax);
	}

	public static ClanBattleReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ClanBattleReward result = default(ClanBattleReward);
		result.TaxRate = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.TotalTax = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ReceivableTax = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ClanBattleReward TaxRate={TaxRate} TotalTax={TotalTax} ReceivableTax={ReceivableTax}>";
	}
}

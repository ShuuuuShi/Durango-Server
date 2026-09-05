using System.Collections.Generic;
using MsgPack;
using Shared.Economy;

namespace Messages;

public struct Wallet
{
	public Dictionary<Currency, long> PaidBalances;

	public Dictionary<Currency, long> UnpaidBalances;

	public VoucherInfo[] Vouchers;

	public static void Pack(Packer packer, Wallet val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.PaidBalances == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.PaidBalances.Count);
			foreach (KeyValuePair<Currency, long> paidBalance in val.PaidBalances)
			{
				packer.Pack((int)paidBalance.Key);
				packer.Pack(paidBalance.Value);
			}
		}
		if (val.UnpaidBalances == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.UnpaidBalances.Count);
			foreach (KeyValuePair<Currency, long> unpaidBalance in val.UnpaidBalances)
			{
				packer.Pack((int)unpaidBalance.Key);
				packer.Pack(unpaidBalance.Value);
			}
		}
		if (val.Vouchers == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Vouchers.Length);
		for (int i = 0; i < val.Vouchers.Length; i++)
		{
			VoucherInfo.Pack(packer, val.Vouchers[i]);
		}
	}

	public static Wallet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Wallet result = default(Wallet);
		result.PaidBalances = new Dictionary<Currency, long>(num, default(CurrencyComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Currency key = ((num2 >= 0 && 7 >= num2) ? ((Currency)num2) : Currency.Invalid);
			unpacker.Read();
			long value = unpacker.LastReadData.AsInt64();
			result.PaidBalances.Add(key, value);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.UnpaidBalances = new Dictionary<Currency, long>(num3, default(CurrencyComparer));
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			int num4 = unpacker.LastReadData.AsInt32();
			Currency key2 = ((num4 >= 0 && 7 >= num4) ? ((Currency)num4) : Currency.Invalid);
			unpacker.Read();
			long value2 = unpacker.LastReadData.AsInt64();
			result.UnpaidBalances.Add(key2, value2);
		}
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.Vouchers = new VoucherInfo[num5];
		for (int k = 0; k < num5; k++)
		{
			unpacker.Read();
			ref VoucherInfo reference = ref result.Vouchers[k];
			reference = VoucherInfo.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Wallet PaidBalances={PaidBalances} UnpaidBalances={UnpaidBalances} Vouchers={Vouchers}>";
	}
}

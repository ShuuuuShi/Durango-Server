using Durango.Logic.Shop;
using Durango.System;
using JetBrains.Annotations;
using Messages;
using Shared.Economy;
using Shared.Voucher;
using UnityEngine;
using Yaml;
using Yaml.Util;

public static class WalletExtension
{
	public static long GetBalance(this Wallet wallet, Currency currency)
	{
		return wallet.GetPaidBalance(currency) + wallet.GetUnpaidBalance(currency);
	}

	public static long GetPaidBalance(this Wallet wallet, Currency currency)
	{
		return (wallet.PaidBalances != null) ? wallet.PaidBalances.Get(currency.Normalize(), 0L) : 0;
	}

	public static long GetUnpaidBalance(this Wallet wallet, Currency currency)
	{
		return (wallet.UnpaidBalances != null) ? wallet.UnpaidBalances.Get(currency.Normalize(), 0L) : 0;
	}

	public static Currency Normalize(this Currency type)
	{
		if (type == Currency.Coin)
		{
			return (!Platform.Instance.UsePCCoin) ? Currency.MobileCoin : Currency.PcCoin;
		}
		return type;
	}

	public static int GetVoucherCount(this Wallet wallet, [CanBeNull] string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return 0;
		}
		int i = 0;
		for (int size = KUtility.GetSize(wallet.Vouchers); i < size; i++)
		{
			VoucherInfo voucherInfo = wallet.Vouchers[i];
			if (voucherInfo.VoucherId == id)
			{
				return voucherInfo.Count;
			}
		}
		return 0;
	}

	public static bool HasVouchers(this Wallet wallet, GuideType type)
	{
		int i = 0;
		for (int size = KUtility.GetSize(wallet.Vouchers); i < size; i++)
		{
			VoucherInfo voucherInfo = wallet.Vouchers[i];
			if (voucherInfo.Count > 0 && SingletonDict<string, Voucher>.TryGetValue(voucherInfo.VoucherId, out var value) && value.GuideType == type)
			{
				return true;
			}
		}
		return false;
	}

	public static int PurchasableVoucherCount(this Wallet wallet, Durango.Logic.Shop.Commodity commodity)
	{
		if (!commodity.VoucherPurchasable())
		{
			return 0;
		}
		int voucherCount = wallet.GetVoucherCount(commodity.Data.VoucherId);
		return Mathf.FloorToInt((float)voucherCount / (float)commodity.Data.VoucherAmount);
	}
}

using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils;
using Messages;
using NCalc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Economy;
using Yaml.Util;

namespace Yaml;

public class Cost
{
	[JsonProperty(PropertyName = "currency")]
	public Currency Currency;

	[JsonProperty(PropertyName = "voucher_amount")]
	public int VoucherAmount;

	[JsonProperty(PropertyName = "voucher_id")]
	public string VoucherId;

	private long? _amount;

	private Expression _amountExpression;

	private long? _cachedCurrencyExpressionValue;

	[JsonProperty(PropertyName = "amount")]
	public JToken Amount
	{
		set
		{
			switch (value.Type)
			{
			case JTokenType.Integer:
				_amount = value.Value<long>();
				_amountExpression = null;
				break;
			case JTokenType.None:
			case JTokenType.Null:
				_amount = null;
				_amountExpression = null;
				break;
			default:
				_amount = null;
				_amountExpression = ExpressionParser.Parse(value.Value<string>());
				break;
			}
		}
	}

	public bool PayableByCurrency(Currency? currency = null)
	{
		long? amount = _amount;
		if (!amount.HasValue && _amountExpression == null)
		{
			return false;
		}
		if (!currency.HasValue)
		{
			return true;
		}
		return currency.Value == Currency;
	}

	public bool PayableByVoucher()
	{
		return !string.IsNullOrEmpty(VoucherId);
	}

	public void SetAmountParams(params KeyValuePair<string, object>[] param)
	{
		_amountExpression.Parameters.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(param); i < size; i++)
		{
			_amountExpression.Parameters.Add(param[i].Key, param[i].Value);
		}
		_cachedCurrencyExpressionValue = null;
	}

	public long GetAmount()
	{
		if (_amountExpression == null)
		{
			long? amount = _amount;
			if (!amount.HasValue)
			{
				return 0L;
			}
			return _amount.Value;
		}
		long? cachedCurrencyExpressionValue = _cachedCurrencyExpressionValue;
		if (!cachedCurrencyExpressionValue.HasValue)
		{
			_cachedCurrencyExpressionValue = _amountExpression.ToInt64(0L);
		}
		return _cachedCurrencyExpressionValue.Value;
	}

	public bool Payable(Wallet wallet)
	{
		Payable(wallet, out var byVoucher, out var byCurrency);
		return byVoucher || byCurrency;
	}

	public void Payable(Wallet wallet, out bool byVoucher, out bool byCurrency)
	{
		byVoucher = PayableByVoucher() && VoucherAmount <= wallet.GetVoucherCount(VoucherId);
		byCurrency = PayableByCurrency() && GetAmount() <= wallet.GetBalance(Currency);
	}

	public string CostToString(Wallet wallet)
	{
		Payable(wallet, out var byVoucher, out var byCurrency);
		if (byVoucher)
		{
			return SingletonDict<string, Voucher>.Get(VoucherId).GetCostFormat(VoucherAmount);
		}
		if (byCurrency)
		{
			return Durango.Logic.Item.Inventory.CurrencyFormat(GetAmount(), Currency);
		}
		if (PayableByCurrency())
		{
			return Durango.Logic.Item.Inventory.CurrencyFormat(GetAmount(), Currency);
		}
		if (PayableByVoucher())
		{
			return SingletonDict<string, Voucher>.Get(VoucherId).GetCostFormat(VoucherAmount);
		}
		return string.Empty;
	}

	public string CostToEmphasisString(Wallet wallet)
	{
		Payable(wallet, out var byVoucher, out var byCurrency);
		if (byVoucher)
		{
			return SingletonDict<string, Voucher>.Get(VoucherId).GetEmphasisCostFormat(VoucherAmount);
		}
		if (byCurrency)
		{
			return Durango.Logic.Item.Inventory.CurrencyEmphasisFormat(GetAmount(), Currency);
		}
		if (PayableByCurrency())
		{
			return Durango.Logic.Item.Inventory.CurrencyEmphasisFormat(GetAmount(), Currency);
		}
		if (PayableByVoucher())
		{
			return SingletonDict<string, Voucher>.Get(VoucherId).GetEmphasisCostFormat(VoucherAmount);
		}
		return string.Empty;
	}

	public static Cost ConvertToYamlCost(Money money, string voucherId, int voucherAmount)
	{
		return new Cost
		{
			VoucherId = voucherId,
			VoucherAmount = voucherAmount,
			_amount = money.Amount,
			Currency = money.Currency
		};
	}
}

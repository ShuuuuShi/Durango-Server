using System.Collections.Generic;
using Durango.System;
using Durango.UI.Control;
using Durango.Utils;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Popup;

public class WalletInfoPopup : TooltipBase
{
	[SerializeField]
	private KWidgetScrollView _scroll;

	[SerializeField]
	private UIWidget _currencies;

	[SerializeField]
	private UIWidget _vouchers;

	[SerializeField]
	private UIWidget _baseObject;

	private readonly ListObjectPool<WalletInfo> _walletPool = new ListObjectPool<WalletInfo>();

	private readonly List<WalletInfo> _currencyInfos = new List<WalletInfo>();

	private readonly List<WalletInfo> _voucherInfos = new List<WalletInfo>();

	protected override void OnAwake()
	{
		_walletPool.BaseObject = _baseObject.GetComponent<WalletInfo>();
		_walletPool.UseBase = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<InventorySystem>.Instance().WalletUpdated += base.Refresh;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameSystem<InventorySystem>.Instance().WalletUpdated -= base.Refresh;
	}

	protected override void FillData()
	{
		_walletPool.BeginLoad();
		_currencyInfos.Clear();
		Currency[] array = Enums<Currency>.All();
		Currency[] array2 = array;
		foreach (Currency currency in array2)
		{
			if (currency == Currency.Invalid || currency == Currency.Coin || (!Platform.Instance.UsePCCoin && currency == Currency.PcCoin))
			{
				continue;
			}
			if (!Debug.isDebugBuild && currency == Currency.PcCoin)
			{
				NPCountry country = Platform.Instance.Country;
				if (country == NPCountry.China || country == NPCountry.Japan || country == NPCountry.Korea || country == NPCountry.Russia)
				{
					continue;
				}
			}
			WalletInfo next = _walletPool.GetNext();
			next.SetCurrency(currency);
			next.transform.parent = _currencies.transform;
			_currencyInfos.Add(next);
		}
		_voucherInfos.Clear();
		foreach (KeyValuePair<string, Voucher> item in SingletonDict<string, Voucher>.Instance)
		{
			if (item.Value.IsValid() && item.Value.Visible)
			{
				WalletInfo next2 = _walletPool.GetNext();
				next2.SetVoucher(item.Key, item.Value);
				next2.transform.parent = _vouchers.transform;
				_voucherInfos.Add(next2);
			}
		}
		_walletPool.EndLoad();
	}

	protected override void UpdateLayout()
	{
		Vector2 vector = UIUtility.WidgetsGridReposition(_currencyInfos, null, Vector2.down, _currencies.localCorners[1], _currencies.width, _baseObject.localSize, 0f, 0f);
		_currencies.width = (int)vector.x;
		_currencies.height = (int)vector.y;
		vector = UIUtility.WidgetsGridReposition(_voucherInfos, null, Vector2.down, _vouchers.localCorners[1], _vouchers.width, _baseObject.localSize, 0f, 0f);
		_vouchers.width = (int)vector.x;
		_vouchers.height = (int)vector.y;
		_scroll.ResetPosition();
	}
}

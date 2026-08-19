using System.Linq;
using Durango.Logic.Item;
using Durango.System;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class WalletInfo : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private GameObject _leftArea;

	[SerializeField]
	private GameObject _rightArea;

	private Currency? _currency;

	private Voucher? _voucher;

	private void Awake()
	{
		UIEventListener.Get(_leftArea).onClick = LeftAreaClicked;
		UIEventListener.Get(_rightArea).onClick = RightAreaClicked;
	}

	private void LeftAreaClicked(GameObject go)
	{
		GetTooltipText(out var title, out var comment);
		if (!string.IsNullOrEmpty(comment))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(title, comment, 400);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(_icon, Vector2.down, 10f);
		}
	}

	private void RightAreaClicked(GameObject go)
	{
		if (_currency.HasValue)
		{
			PresetCurrencyWidget.ChargeCurrency(_currency.Value);
		}
	}

	private static bool IsUsableCurrency(Currency currency)
	{
		bool result = true;
		switch (currency.Normalize())
		{
		case Currency.MobileCoin:
			result = !Platform.Instance.UsePCCoin;
			break;
		case Currency.PcCoin:
			result = Platform.Instance.UsePCCoin;
			break;
		}
		return result;
	}

	public void SetCurrency(Currency currency)
	{
		float alpha = ((!IsUsableCurrency(currency)) ? 0.5f : 1f);
		_currency = currency;
		_voucher = null;
		_icon.spriteName = Durango.Logic.Item.Inventory.GetIcon(currency);
		_icon.alpha = alpha;
		long balance = InventorySystem.Wallet.GetBalance(currency);
		string text = ((!PresetCurrencyWidget.IsChargable(currency)) ? string.Empty : "  [FFFFFF34][icon=box_plus][-]");
		_label.text = "[c]" + Durango.Logic.Item.Inventory.CurrencyFormat(balance) + "[/c]" + text;
		_label.alpha = alpha;
	}

	public void SetVoucher(string voucherId, Voucher voucher)
	{
		_currency = null;
		_voucher = voucher;
		VoucherInfo voucherInfo = ((InventorySystem.Wallet.Vouchers == null) ? default(VoucherInfo) : InventorySystem.Wallet.Vouchers.FirstOrDefault((VoucherInfo info) => info.VoucherId == voucherId));
		_icon.spriteName = voucher.Icon;
		_icon.color = voucher.GetHexColor().ToColor();
		string format = ((voucherInfo.Count != 0) ? "[c]{0}[/c] [ffffff80]/ {1}[-]" : "[ffffff80]{0}[-] [92929280]/ {1}[-]");
		_label.text = string.Format(format, voucherInfo.Count, voucher.CountMax);
	}

	private void GetTooltipText(out string title, out string comment)
	{
		title = null;
		comment = null;
		if (_currency.HasValue)
		{
			title = _currency.GetName();
			switch (_currency.Value.Normalize())
			{
			case Currency.TStone:
				comment = T._("항해/워프할 때, 또는 도시섬 사유지를 유지하거나, 장터의 물건을 구입하는 용도 등으로 씁니다. 게임 플레이로 얻을 수 있습니다.");
				break;
			case Currency.Gem:
				comment = string.Format("{0}<br>10</br>{1}", T._("상점의 상품을 구입하거나, 대기 시간이 필요한 행동을 즉시 완료하고 아이템/건물을 수리할 때 씁니다. 코인으로 사거나 게임 플레이로 얻을 수 있습니다."), T._("<ref>ui://shop/category/gem, 구매하러 가기</ref>"));
				break;
			case Currency.MobileCoin:
				comment = T._("상점에서 특정 패키지 상품이나 워프젬을 구입할 때 씁니다. 모바일 기기에서만 사용/충전할 수 있습니다.");
				break;
			case Currency.PcCoin:
				comment = T._("상점에서 특정 패키지 상품이나 워프젬을 구입할 때 씁니다. PC에서만 사용/충전할 수 있습니다.");
				break;
			case Currency.CashshopMileage:
				comment = T._("상점의 마일리지 샵에서 특정 아이템을 구입할 때 씁니다. 특송화물을 구입할 때마다 얻을 수 있습니다.");
				break;
			case Currency.RPiece:
				comment = string.Format("{0}<br>10</br>{1}", T._("기술지원을 통해 장비의 성능을 개선할 때 씁니다. 코인 또는 워프젬으로 구입하거나 게임 플레이로 얻을 수 있습니다."), T._("<ref>ui://shop/category/r_piece, 구매하러 가기</ref>"));
				break;
			case Currency.WarpMatter:
			{
				Pair<int, int> warpMatterAcquisition = GameSystem<WarpAcceleratorSystem>.Instance().GetWarpMatterAcquisition();
				comment = string.Format("{0}<br>10</br><weak>{1} <bar/> {2}</weak><br>10</br><ref>ui://Shop/Category/warp_matter, {3}</ref>", T._("불안정섬 워프 가속기에서 획득할 수 있습니다.\n주간 획득 가능 개수는 캐릭터 레벨에 따라 증가합니다."), T._("이번 주 획득 가능"), T._("<em>{0:N0}</em>/{1:N0} 개 남음", warpMatterAcquisition.Item1, warpMatterAcquisition.Item2), T._("사용하러 가기"));
				break;
			}
			case Currency.Coin:
				break;
			}
		}
		else if (_voucher.HasValue)
		{
			Voucher value = _voucher.Value;
			title = value.Name;
			if (string.IsNullOrEmpty(value.Link))
			{
				comment = value.Description;
			}
			else
			{
				comment = $"{value.Description}<br>10</br>{value.Link}";
			}
		}
	}
}

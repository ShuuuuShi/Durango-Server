using Durango.Logic;
using Durango.Logic.Shop;
using Durango.Network;
using Yaml.Util;

namespace Yaml;

public class Vouchers : SingletonDict<string, Voucher>
{
	public static string GetPackageEffectText(string voucherId, string commodityId, string textFormat)
	{
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(commodityId);
		StatusEffect statusEffectFromCommodity = GameSystem<StatusEffectSystem>.Instance().GetStatusEffectFromCommodity(commodityId);
		if (statusEffectFromCommodity == null || commodity == null)
		{
			return string.Empty;
		}
		Voucher voucher = SingletonDict<string, Voucher>.Instance.Get(voucherId);
		string arg = TimedeltaFormatter.Format(statusEffectFromCommodity.Until - Connections.Frontend.GetPredictedServerTime());
		return string.Format(textFormat, commodity.Title, voucher.CountMax, arg);
	}
}

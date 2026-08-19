using System.Collections.Generic;
using System.Text;
using Durango.Logic.Item;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Faction;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Quest;

public static class Util
{
	public static string RewardToString(RewardInfo reward)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder stringBuilder = reusable;
		if (reward.SkillPoints.HasValue && reward.SkillPoints.Value > 0)
		{
			stringBuilder.AppendFormat("[icon=icon_sp] {0}  ", reward.SkillPoints.Value);
		}
		int? exp = reward.Exp;
		if (exp.HasValue && exp.GetValueOrDefault() > 0)
		{
			stringBuilder.AppendFormat("[icon=icon_exp] {0}  ", reward.Exp);
		}
		if (reward.Currency != null)
		{
			foreach (KeyValuePair<Currency, long> item in reward.Currency)
			{
				if (item.Value > 0)
				{
					stringBuilder.AppendFormat("{0}  ", Durango.Logic.Item.Inventory.CurrencyFormat(item.Value, item.Key));
				}
			}
		}
		if (reward.Vouchers != null)
		{
			VoucherInfo[] vouchers = reward.Vouchers;
			for (int i = 0; i < vouchers.Length; i++)
			{
				VoucherInfo voucherInfo = vouchers[i];
				Voucher voucher = SingletonDict<string, Voucher>.Get(voucherInfo.VoucherId);
				stringBuilder.AppendFormat(T.Culture, "{0} {1:N0}  ", voucher.GetIconText(), voucherInfo.Count);
			}
		}
		if (KUtility.GetSize(reward.FriendshipPoint) > 0)
		{
			foreach (KeyValuePair<FactionType, int> item2 in reward.FriendshipPoint)
			{
				stringBuilder.AppendFormat(T.Culture, "[icon={0}] {1:N0}  ", IconMap.Get(item2.Key), item2.Value);
			}
		}
		return stringBuilder.ToString().Trim();
	}
}

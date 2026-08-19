using System;
using Durango.Logic.Event;
using Durango.Logic.Item;
using L10N;
using Shared.Economy;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public abstract class CalendarWidget : UIWidget
{
	public abstract void Set(Calendar calendar);

	public abstract CalendarNodeWidget GetNodeWidget(int index);

	public void TakeTodayAtendanceReward(Calendar calendar, bool restore, Action onSuccess)
	{
		calendar.TakeTodayAttendanceReward(restore, delegate(CalenderReward reward)
		{
			ShowRewardAlarm(reward);
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	protected void ShowRewardAlarm(CalenderReward reward)
	{
		AlarmRewardQueue.Args args = default(AlarmRewardQueue.Args);
		if (reward.Item != null)
		{
			args.Icon = reward.Item.Icon;
			args.Main = T._("{0} x{1}", reward.Item.Name, reward.ItemCount);
		}
		else if (reward.Money.Currency != Currency.Invalid && reward.Money.Amount > 0)
		{
			args.Icon = Inventory.GetIcon(reward.Money.Currency);
			args.Main = T._("{0} x{1}", reward.Money.Currency.GetName(), reward.Money.Amount.ToString());
		}
		else if (reward.Voucher.HasValue)
		{
			string voucherId = reward.Voucher.Value.VoucherId;
			Voucher voucher = SingletonDict<string, Voucher>.Get(voucherId);
			if (!voucher.IsValid() || reward.Voucher.Value.Count <= 0)
			{
			}
			args.Icon = new ItemIcon
			{
				Main = voucher.Icon,
				Colors = new ItemColor(voucher.GetHexColor())
			};
			args.Main = T._("{0} x{1}", voucher.Name, reward.Voucher.Value.Count);
		}
		AlarmGroup alarmGroup = UIManager.FindScript<AlarmGroup>();
		alarmGroup.RewardAlarm(args, AlarmGroup.RewardEffectType.TodayAttendanceReward);
	}
}

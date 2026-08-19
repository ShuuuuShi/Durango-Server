using Durango.Logic.Item;
using Messages;
using Shared.Attendance;

namespace Durango.Logic.Event;

public struct CalenderReward
{
	public RewardType Type;

	public RewardState State;

	public ItemData Item;

	public int ItemCount;

	public Money Money;

	public VoucherInfo? Voucher;

	public int Index;

	public CalenderReward(AttendanceReward reward, int index)
	{
		Type = reward.RewardType;
		State = ((!reward.Rewarded) ? RewardState.None : RewardState.Completed);
		Item = ((!reward.Item.HasValue) ? null : new ItemData(reward.Item.Value));
		ItemCount = reward.ItemCount;
		Money = ((!reward.Money.HasValue) ? default(Money) : reward.Money.Value);
		Voucher = reward.Voucher;
		Index = index;
	}
}

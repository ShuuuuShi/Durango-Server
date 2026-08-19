using MsgPack;
using Shared.Attendance;
using Shared.Economy;

namespace Messages;

public struct AttendanceReward
{
	public bool Rewarded;

	public RewardType RewardType;

	public Money? Money;

	public Item? Item;

	public int ItemCount;

	public VoucherInfo? Voucher;

	public static void Pack(Packer packer, AttendanceReward val, bool hint = false)
	{
		packer.PackArrayHeader(6);
		packer.Pack(val.Rewarded);
		packer.Pack((int)val.RewardType);
		if (!val.Money.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.Money.Value.Amount);
			packer.Pack((int)val.Money.Value.Currency);
		}
		if (!val.Item.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Item.Pack(packer, val.Item.Value);
		}
		packer.Pack(val.ItemCount);
		if (!val.Voucher.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			VoucherInfo.Pack(packer, val.Voucher.Value);
		}
	}

	public static AttendanceReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AttendanceReward result = default(AttendanceReward);
		result.Rewarded = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 2 < num)
		{
			result.RewardType = RewardType.Invalid;
		}
		else
		{
			result.RewardType = (RewardType)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Money = null;
		}
		else
		{
			unpacker.ReadInt32(out var result2);
			unpacker.ReadInt32(out var result3);
			Money value = new Money(result2, (Currency)result3);
			result.Money = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Item = null;
		}
		else
		{
			Item value2 = Messages.Item.Unpack(unpacker);
			result.Item = value2;
		}
		unpacker.Read();
		result.ItemCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Voucher = null;
		}
		else
		{
			VoucherInfo value3 = VoucherInfo.Unpack(unpacker);
			result.Voucher = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AttendanceReward Rewarded={Rewarded} RewardType={RewardType} Money={Money} Item={Item} ItemCount={ItemCount} Voucher={Voucher}>";
	}
}

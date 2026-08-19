using MsgPack;
using Shared.Economy;

namespace Messages;

public struct PriceRangePredicate
{
	public int? Min;

	public int? Max;

	public Currency Currency;

	public static void Pack(Packer packer, PriceRangePredicate val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (!val.Min.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Min.Value);
		}
		if (!val.Max.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Max.Value);
		}
		packer.Pack((int)val.Currency);
	}

	public static PriceRangePredicate Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PriceRangePredicate result = default(PriceRangePredicate);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.Min = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			result.Min = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.Max = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int value2 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			result.Max = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		if (num < 0 || 1 < num)
		{
			result.Currency = Currency.Invalid;
		}
		else
		{
			result.Currency = (Currency)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PriceRangePredicate Min={Min} Max={Max} Currency={Currency}>";
	}
}

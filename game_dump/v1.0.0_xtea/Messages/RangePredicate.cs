using MsgPack;

namespace Messages;

public struct RangePredicate
{
	public int? Min;

	public int? Max;

	public static void Pack(Packer packer, RangePredicate val, bool hint = false)
	{
		packer.PackArrayHeader(2);
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
	}

	public static RangePredicate Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RangePredicate result = default(RangePredicate);
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
		return result;
	}

	public override string ToString()
	{
		return $"<RangePredicate Min={Min} Max={Max}>";
	}
}

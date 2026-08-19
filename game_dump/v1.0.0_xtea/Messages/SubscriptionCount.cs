using MsgPack;
using Shared.Chat;

namespace Messages;

public struct SubscriptionCount
{
	public const uint TypeCode = 2080u;

	public ChannelType ChannelType;

	public uint Count;

	public static void Pack(Packer packer, SubscriptionCount val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2080u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.ChannelType);
		packer.Pack(val.Count);
	}

	public static SubscriptionCount Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		SubscriptionCount result = default(SubscriptionCount);
		if (num < 0 || 3 < num)
		{
			result.ChannelType = ChannelType.Invalid;
		}
		else
		{
			result.ChannelType = (ChannelType)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Count = ((MessagePackObject)(ref lastReadData2)).AsUInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SubscriptionCount ChannelType={ChannelType} Count={Count}>";
	}
}

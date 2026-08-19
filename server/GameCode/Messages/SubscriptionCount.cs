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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SubscriptionCount result = default(SubscriptionCount);
		if (num < 0 || 6 < num)
		{
			result.ChannelType = ChannelType.Invalid;
		}
		else
		{
			result.ChannelType = (ChannelType)num;
		}
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsUInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SubscriptionCount ChannelType={ChannelType} Count={Count}>";
	}
}

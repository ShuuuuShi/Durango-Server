using MsgPack;
using Shared.Chat;

namespace Messages;

public struct SayInExclusiveChannel
{
	public const uint TypeCode = 2408u;

	public Message_ Message;

	public ChannelType ChannelType;

	public static void Pack(Packer packer, SayInExclusiveChannel val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2408u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		Message_.Pack(packer, val.Message);
		packer.Pack((int)val.ChannelType);
	}

	public static SayInExclusiveChannel Unpack(Unpacker unpacker)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		SayInExclusiveChannel result = default(SayInExclusiveChannel);
		result.Message = Message_.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		if (num < 0 || 3 < num)
		{
			result.ChannelType = ChannelType.Invalid;
		}
		else
		{
			result.ChannelType = (ChannelType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SayInExclusiveChannel Message={Message} ChannelType={ChannelType}>";
	}
}

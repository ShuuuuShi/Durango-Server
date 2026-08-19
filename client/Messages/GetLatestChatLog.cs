using MsgPack;
using Shared.Chat;

namespace Messages;

public struct GetLatestChatLog
{
	public const uint TypeCode = 25u;

	public ChannelType ChannelType;

	public byte? Offset;

	public byte? Limit;

	public static void Pack(Packer packer, GetLatestChatLog val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(25u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.ChannelType);
		if (!val.Offset.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Offset.Value);
		}
		if (!val.Limit.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Limit.Value);
		}
	}

	public static GetLatestChatLog Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetLatestChatLog result = default(GetLatestChatLog);
		if (num < 0 || 6 < num)
		{
			result.ChannelType = ChannelType.Invalid;
		}
		else
		{
			result.ChannelType = (ChannelType)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Offset = null;
		}
		else
		{
			byte value = unpacker.LastReadData.AsByte();
			result.Offset = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Limit = null;
		}
		else
		{
			byte value2 = unpacker.LastReadData.AsByte();
			result.Limit = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetLatestChatLog ChannelType={ChannelType} Offset={Offset} Limit={Limit}>";
	}
}

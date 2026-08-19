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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		GetLatestChatLog result = default(GetLatestChatLog);
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
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Offset = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			byte value = ((MessagePackObject)(ref lastReadData3)).AsByte();
			result.Offset = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.Limit = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			byte value2 = ((MessagePackObject)(ref lastReadData5)).AsByte();
			result.Limit = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetLatestChatLog ChannelType={ChannelType} Offset={Offset} Limit={Limit}>";
	}
}

using MsgPack;

namespace Messages;

public struct TimelineOption
{
	public const uint TypeCode = 81234527u;

	public bool EstateNotification;

	public static void Pack(Packer packer, TimelineOption val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(81234527u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EstateNotification);
	}

	public static TimelineOption Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TimelineOption result = default(TimelineOption);
		result.EstateNotification = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<TimelineOption EstateNotification={EstateNotification}>";
	}
}

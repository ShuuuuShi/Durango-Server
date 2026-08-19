using MsgPack;

namespace Messages;

public struct TimerEnded
{
	public const uint TypeCode = 14u;

	public string EntityId;

	public string Subject;

	public static void Pack(Packer packer, TimerEnded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(14u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.Subject == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Subject);
		}
	}

	public static TimerEnded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TimerEnded result = default(TimerEnded);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Subject = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<TimerEnded EntityId={EntityId} Subject={Subject}>";
	}
}

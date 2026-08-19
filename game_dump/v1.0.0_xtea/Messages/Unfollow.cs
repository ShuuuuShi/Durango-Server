using MsgPack;

namespace Messages;

public struct Unfollow
{
	public const uint TypeCode = 2410u;

	public ulong EntityId;

	public static void Pack(Packer packer, Unfollow val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2410u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static Unfollow Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Unfollow result = default(Unfollow);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<Unfollow EntityId={EntityId}>";
	}
}

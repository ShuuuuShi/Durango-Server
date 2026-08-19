using MsgPack;

namespace Messages;

public struct FeedingSuccess
{
	public const uint TypeCode = 813u;

	public ulong PetId;

	public static void Pack(Packer packer, FeedingSuccess val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(813u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.PetId);
	}

	public static FeedingSuccess Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		FeedingSuccess result = default(FeedingSuccess);
		result.PetId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<FeedingSuccess PetId={PetId}>";
	}
}

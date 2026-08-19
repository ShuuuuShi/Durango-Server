using MsgPack;

namespace Messages;

public struct KickClanMember
{
	public const uint TypeCode = 3661u;

	public ulong EntityId;

	public static void Pack(Packer packer, KickClanMember val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3661u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static KickClanMember Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		KickClanMember result = default(KickClanMember);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<KickClanMember EntityId={EntityId}>";
	}
}

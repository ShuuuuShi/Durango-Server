using MsgPack;

namespace Messages;

public struct Secured
{
	public ulong EntityId;

	public ulong OwnerId;

	public static void Pack(Packer packer, Secured val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.EntityId);
		packer.Pack(val.OwnerId);
	}

	public static Secured Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Secured result = default(Secured);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.OwnerId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<Secured EntityId={EntityId} OwnerId={OwnerId}>";
	}
}

using MsgPack;

namespace Messages;

public struct EntityKey
{
	public ulong EntityId;

	public static void Pack(Packer packer, EntityKey val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		packer.Pack(val.EntityId);
	}

	public static EntityKey Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		EntityKey result = default(EntityKey);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<EntityKey EntityId={EntityId}>";
	}
}

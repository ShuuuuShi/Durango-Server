using MsgPack;

namespace Messages;

public struct TryInspectNatural
{
	public const uint TypeCode = 3606u;

	public ulong EntityId;

	public static void Pack(Packer packer, TryInspectNatural val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3606u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static TryInspectNatural Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		TryInspectNatural result = default(TryInspectNatural);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<TryInspectNatural EntityId={EntityId}>";
	}
}

using MsgPack;

namespace Messages;

public struct ArtifactCompleted
{
	public const uint TypeCode = 2095u;

	public ulong EntityId;

	public static void Pack(Packer packer, ArtifactCompleted val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2095u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.EntityId);
	}

	public static ArtifactCompleted Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ArtifactCompleted result = default(ArtifactCompleted);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactCompleted EntityId={EntityId}>";
	}
}

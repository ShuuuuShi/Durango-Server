using MsgPack;

namespace Messages;

public struct ArtifactBuilt
{
	public const uint TypeCode = 2093u;

	public ulong EntityId;

	public ulong BuilderId;

	public static void Pack(Packer packer, ArtifactBuilt val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2093u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.BuilderId);
	}

	public static ArtifactBuilt Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ArtifactBuilt result = default(ArtifactBuilt);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.BuilderId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactBuilt EntityId={EntityId} BuilderId={BuilderId}>";
	}
}

using MsgPack;

namespace Messages;

public struct ArtifactResponse
{
	public const uint TypeCode = 310u;

	public ulong ArtifactId;

	public string Action;

	public bool Success;

	public static void Pack(Packer packer, ArtifactResponse val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(310u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.ArtifactId);
		if (val.Action == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Action);
		}
		packer.Pack(val.Success);
	}

	public static ArtifactResponse Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ArtifactResponse result = default(ArtifactResponse);
		result.ArtifactId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Action = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Success = ((MessagePackObject)(ref lastReadData3)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactResponse ArtifactId={ArtifactId} Action={Action} Success={Success}>";
	}
}

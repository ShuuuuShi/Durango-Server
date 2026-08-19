using MsgPack;

namespace Messages;

public struct ArtifactDigest
{
	public string PrototypeId;

	public ulong EntityId;

	public ulong RegionId;

	public int[] Tile;

	public static void Pack(Packer packer, ArtifactDigest val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.RegionId);
		if (val.Tile == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Tile.Length);
		for (int i = 0; i < val.Tile.Length; i++)
		{
			packer.Pack(val.Tile[i]);
		}
	}

	public static ArtifactDigest Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ArtifactDigest result = default(ArtifactDigest);
		result.PrototypeId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.RegionId = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Tile = new int[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int[] tile = result.Tile;
			int num2 = i;
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			tile[num2] = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactDigest PrototypeId={PrototypeId} EntityId={EntityId} RegionId={RegionId} Tile={Tile}>";
	}
}

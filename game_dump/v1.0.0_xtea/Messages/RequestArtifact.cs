using MsgPack;

namespace Messages;

public struct RequestArtifact
{
	public const uint TypeCode = 309u;

	public ulong EntityId;

	public Point2 Tile;

	public short Password;

	public string Action;

	public static void Pack(Packer packer, RequestArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(309u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.Password);
		if (val.Action == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Action);
		}
	}

	public static RequestArtifact Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RequestArtifact result = default(RequestArtifact);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Password = ((MessagePackObject)(ref lastReadData2)).AsInt16();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Action = ((MessagePackObject)(ref lastReadData3)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RequestArtifact EntityId={EntityId} Tile={Tile} Password={Password} Action={Action}>";
	}
}

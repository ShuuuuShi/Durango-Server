using MsgPack;

namespace Messages;

public struct RepairArtifact
{
	public const uint TypeCode = 2055u;

	public string EntityId;

	public Point2 Tile;

	public string[] KitItemIds;

	public static void Pack(Packer packer, RepairArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2055u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.KitItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.KitItemIds.Length);
		for (int i = 0; i < val.KitItemIds.Length; i++)
		{
			if (val.KitItemIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.KitItemIds[i]);
			}
		}
	}

	public static RepairArtifact Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RepairArtifact result = default(RepairArtifact);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.KitItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.KitItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RepairArtifact EntityId={EntityId} Tile={Tile} KitItemIds={KitItemIds}>";
	}
}

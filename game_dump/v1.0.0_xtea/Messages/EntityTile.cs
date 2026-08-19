using MsgPack;

namespace Messages;

public struct EntityTile
{
	public Region Region;

	public Point2 Tile;

	public ulong? EntityId;

	public string EntityName;

	public static void Pack(Packer packer, EntityTile val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		Region.Pack(packer, val.Region);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (!val.EntityId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.EntityId.Value);
		}
		if (val.EntityName == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.EntityName);
		}
	}

	public static EntityTile Unpack(Unpacker unpacker)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		EntityTile result = default(EntityTile);
		result.Region = Region.Unpack(unpacker);
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.EntityId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
			result.EntityId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.EntityName = null;
		}
		else
		{
			string entityName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.EntityName = entityName;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EntityTile Region={Region} Tile={Tile} EntityId={EntityId} EntityName={EntityName}>";
	}
}

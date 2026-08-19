using MsgPack;

namespace Messages;

public struct EntityTile
{
	public Region Region;

	public Point2 Tile;

	public string EntityId;

	public string EntityName;

	public static void Pack(Packer packer, EntityTile val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		Region.Pack(packer, val.Region);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.EntityId == null)
		{
			packer.PackNull();
		}
		else if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
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
		unpacker.Read();
		EntityTile result = default(EntityTile);
		result.Region = Region.Unpack(unpacker);
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EntityId = null;
		}
		else
		{
			string entityId = unpacker.LastReadData.AsString();
			result.EntityId = entityId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
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

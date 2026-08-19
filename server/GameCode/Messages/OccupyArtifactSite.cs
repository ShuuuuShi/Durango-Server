using MsgPack;
using Shared.Etc;

namespace Messages;

public struct OccupyArtifactSite
{
	public const uint TypeCode = 2057u;

	public string BlueprintId;

	public string ItemId;

	public Point2 Tile;

	public int? Floor;

	public Point2 Size;

	public int? Stories;

	public Rotation Rotation;

	public string ModularEntityId;

	public static void Pack(Packer packer, OccupyArtifactSite val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(9);
			packer.Pack(2057u);
		}
		else
		{
			packer.PackArrayHeader(8);
		}
		if (val.BlueprintId == null)
		{
			packer.PackNull();
		}
		else if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
		if (val.ItemId == null)
		{
			packer.PackNull();
		}
		else if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (!val.Floor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Floor.Value);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Size.x);
		packer.Pack((ushort)val.Size.y);
		if (!val.Stories.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Stories.Value);
		}
		packer.Pack((int)val.Rotation);
		if (val.ModularEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ModularEntityId);
		}
	}

	public static OccupyArtifactSite Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		OccupyArtifactSite result = default(OccupyArtifactSite);
		if (unpacker.LastReadData.IsNil)
		{
			result.BlueprintId = null;
		}
		else
		{
			string blueprintId = unpacker.LastReadData.AsString();
			result.BlueprintId = blueprintId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ItemId = null;
		}
		else
		{
			string itemId = unpacker.LastReadData.AsString();
			result.ItemId = itemId;
		}
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Floor = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Floor = value;
		}
		unpacker.Read();
		unpacker.ReadUInt16(out var result3);
		result.Size.x = result3;
		unpacker.ReadUInt16(out result3);
		result.Size.y = result3;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Stories = null;
		}
		else
		{
			int value2 = unpacker.LastReadData.AsInt32();
			result.Stories = value2;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.Rotation = Rotation.Invalid;
		}
		else
		{
			result.Rotation = (Rotation)num;
		}
		unpacker.Read();
		result.ModularEntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<OccupyArtifactSite BlueprintId={BlueprintId} ItemId={ItemId} Tile={Tile} Floor={Floor} Size={Size} Stories={Stories} Rotation={Rotation} ModularEntityId={ModularEntityId}>";
	}
}

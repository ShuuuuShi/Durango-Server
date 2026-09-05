using MsgPack;

namespace Messages;

public struct StartClanResearch
{
	public const uint TypeCode = 3702u;

	public string EntityId;

	public Point2 Tile;

	public string Id;

	public static void Pack(Packer packer, StartClanResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3702u);
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
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
	}

	public static StartClanResearch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		StartClanResearch result = default(StartClanResearch);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Id = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<StartClanResearch EntityId={EntityId} Tile={Tile} Id={Id}>";
	}
}

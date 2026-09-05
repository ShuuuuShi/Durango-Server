using MsgPack;

namespace Messages;

public struct StartPersonalResearch
{
	public const uint TypeCode = 5987338u;

	public string EntityId;

	public Point2 Tile;

	public string ResearchId;

	public static void Pack(Packer packer, StartPersonalResearch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(5987338u);
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
		if (val.ResearchId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ResearchId);
		}
	}

	public static StartPersonalResearch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		StartPersonalResearch result = default(StartPersonalResearch);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.ResearchId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<StartPersonalResearch EntityId={EntityId} Tile={Tile} ResearchId={ResearchId}>";
	}
}

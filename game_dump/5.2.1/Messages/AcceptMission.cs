using MsgPack;

namespace Messages;

public struct AcceptMission
{
	public const uint TypeCode = 3623u;

	public string EntityId;

	public Point2 Tile;

	public string MissionId;

	public static void Pack(Packer packer, AcceptMission val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3623u);
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
		if (val.MissionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MissionId);
		}
	}

	public static AcceptMission Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptMission result = default(AcceptMission);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.MissionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptMission EntityId={EntityId} Tile={Tile} MissionId={MissionId}>";
	}
}

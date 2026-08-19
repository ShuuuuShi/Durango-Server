using MsgPack;
using UnityEngine;

namespace Messages;

public struct PartierStatus
{
	public const uint TypeCode = 20000u;

	public string EntityId;

	public string RegionId;

	public Point2 Tile;

	public Vector2 Health;

	public Vector2 Energy;

	public int Level;

	public bool IsOnline;

	public double ExpiresAt;

	public static void Pack(Packer packer, PartierStatus val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(9);
			packer.Pack(20000u);
		}
		else
		{
			packer.PackArrayHeader(8);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.PackArrayHeader(2);
		packer.Pack(val.Health.x);
		packer.Pack(val.Health.y);
		packer.PackArrayHeader(2);
		packer.Pack(val.Energy.x);
		packer.Pack(val.Energy.y);
		packer.Pack(val.Level);
		packer.Pack(val.IsOnline);
		packer.Pack(val.ExpiresAt);
	}

	public static PartierStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PartierStatus result = default(PartierStatus);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		unpacker.ReadSingle(out result.Health.x);
		unpacker.ReadSingle(out result.Health.y);
		unpacker.Read();
		unpacker.ReadSingle(out result.Energy.x);
		unpacker.ReadSingle(out result.Energy.y);
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.IsOnline = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.ExpiresAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<PartierStatus EntityId={EntityId} RegionId={RegionId} Tile={Tile} Health={Health} Energy={Energy} Level={Level} IsOnline={IsOnline} ExpiresAt={ExpiresAt}>";
	}
}

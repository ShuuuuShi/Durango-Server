using MsgPack;
using UnityEngine;

namespace Messages;

public struct FireProjectileFromVehicle
{
	public const uint TypeCode = 203493u;

	public string EntityId;

	public Point2 Tile;

	public WorldPosition TargetPosition;

	public static void Pack(Packer packer, FireProjectileFromVehicle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(203493u);
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
		packer.PackArrayHeader(2);
		packer.Pack((uint)Mathf.RoundToInt(val.TargetPosition.x));
		packer.Pack((uint)Mathf.RoundToInt(val.TargetPosition.y));
	}

	public static FireProjectileFromVehicle Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		FireProjectileFromVehicle result = default(FireProjectileFromVehicle);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		unpacker.ReadSingle(out result.TargetPosition.x);
		unpacker.ReadSingle(out result.TargetPosition.y);
		return result;
	}

	public override string ToString()
	{
		return $"<FireProjectileFromVehicle EntityId={EntityId} Tile={Tile} TargetPosition={TargetPosition}>";
	}
}

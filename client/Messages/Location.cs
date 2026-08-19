using MsgPack;
using UnityEngine;

namespace Messages;

public struct Location
{
	public WorldPosition Position;

	public float Yaw;

	public double Time;

	public byte Floor;

	public float Height;

	public static void Pack(Packer packer, Location val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		packer.PackArrayHeader(2);
		packer.Pack((uint)Mathf.RoundToInt(val.Position.x));
		packer.Pack((uint)Mathf.RoundToInt(val.Position.y));
		packer.Pack(val.Yaw);
		packer.Pack(val.Time);
		packer.Pack(val.Floor);
		packer.Pack(val.Height);
	}

	public static Location Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Location result = default(Location);
		unpacker.ReadSingle(out result.Position.x);
		unpacker.ReadSingle(out result.Position.y);
		unpacker.Read();
		result.Yaw = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Time = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Floor = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.Height = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Location Position={Position} Yaw={Yaw} Time={Time} Floor={Floor} Height={Height}>";
	}
}

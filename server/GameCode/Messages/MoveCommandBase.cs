using MsgPack;
using UnityEngine;

namespace Messages;

public struct MoveCommandBase
{
	public Vector2 Position;

	public Vector2 Direction;

	public ulong Time;

	public static void Pack(Packer packer, MoveCommandBase val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.PackArrayHeader(2);
		packer.Pack(val.Position.x);
		packer.Pack(val.Position.y);
		packer.PackArrayHeader(2);
		packer.Pack(val.Direction.x);
		packer.Pack(val.Direction.y);
		packer.Pack(val.Time);
	}

	public static MoveCommandBase Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MoveCommandBase result = default(MoveCommandBase);
		unpacker.ReadSingle(out result.Position.x);
		unpacker.ReadSingle(out result.Position.y);
		unpacker.Read();
		unpacker.ReadSingle(out result.Direction.x);
		unpacker.ReadSingle(out result.Direction.y);
		unpacker.Read();
		result.Time = unpacker.LastReadData.AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<MoveCommandBase Position={Position} Direction={Direction} Time={Time}>";
	}
}

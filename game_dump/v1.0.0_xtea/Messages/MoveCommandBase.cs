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
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MoveCommandBase result = default(MoveCommandBase);
		unpacker.ReadSingle(ref result.Position.x);
		unpacker.ReadSingle(ref result.Position.y);
		unpacker.Read();
		unpacker.ReadSingle(ref result.Direction.x);
		unpacker.ReadSingle(ref result.Direction.y);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Time = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return $"<MoveCommandBase Position={Position} Direction={Direction} Time={Time}>";
	}
}

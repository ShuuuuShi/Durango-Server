using MsgPack;
using UnityEngine;

namespace Messages;

public struct DrawLineBase
{
	public Vector3 Position;

	public ulong Time;

	public static void Pack(Packer packer, DrawLineBase val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.PackArrayHeader(3);
		packer.Pack(val.Position.x);
		packer.Pack(val.Position.y);
		packer.Pack(val.Position.z);
		packer.Pack(val.Time);
	}

	public static DrawLineBase Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DrawLineBase result = default(DrawLineBase);
		unpacker.ReadSingle(out result.Position.x);
		unpacker.ReadSingle(out result.Position.y);
		unpacker.ReadSingle(out result.Position.z);
		unpacker.Read();
		result.Time = unpacker.LastReadData.AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<DrawLineBase Position={Position} Time={Time}>";
	}
}

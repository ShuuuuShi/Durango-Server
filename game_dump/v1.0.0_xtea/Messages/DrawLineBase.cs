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
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		DrawLineBase result = default(DrawLineBase);
		unpacker.ReadSingle(ref result.Position.x);
		unpacker.ReadSingle(ref result.Position.y);
		unpacker.ReadSingle(ref result.Position.z);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Time = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return $"<DrawLineBase Position={Position} Time={Time}>";
	}
}

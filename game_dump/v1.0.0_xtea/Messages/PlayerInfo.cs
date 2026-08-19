using MsgPack;
using UnityEngine;

namespace Messages;

public struct PlayerInfo
{
	public ulong PlayerId;

	public Vector2 Position;

	public static void Pack(Packer packer, PlayerInfo val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.PlayerId);
		packer.PackArrayHeader(2);
		packer.Pack(val.Position.x);
		packer.Pack(val.Position.y);
	}

	public static PlayerInfo Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerInfo result = default(PlayerInfo);
		result.PlayerId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		unpacker.ReadSingle(ref result.Position.x);
		unpacker.ReadSingle(ref result.Position.y);
		return result;
	}

	public override string ToString()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return $"<PlayerInfo PlayerId={PlayerId} Position={Position}>";
	}
}

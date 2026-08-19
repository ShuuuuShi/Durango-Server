using MsgPack;
using UnityEngine;

namespace Messages;

public struct RespawnPoint
{
	public byte Id;

	public WorldPosition Position;

	public static void Pack(Packer packer, RespawnPoint val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.Pack(val.Id);
		packer.PackArrayHeader(2);
		packer.Pack((uint)Mathf.RoundToInt(val.Position.x));
		packer.Pack((uint)Mathf.RoundToInt(val.Position.y));
	}

	public static RespawnPoint Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RespawnPoint result = default(RespawnPoint);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsByte();
		unpacker.Read();
		unpacker.ReadSingle(ref result.Position.x);
		unpacker.ReadSingle(ref result.Position.y);
		return result;
	}

	public override string ToString()
	{
		return $"<RespawnPoint Id={Id} Position={Position}>";
	}
}

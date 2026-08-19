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
		unpacker.Read();
		RespawnPoint result = default(RespawnPoint);
		result.Id = unpacker.LastReadData.AsByte();
		unpacker.Read();
		unpacker.ReadSingle(out result.Position.x);
		unpacker.ReadSingle(out result.Position.y);
		return result;
	}

	public override string ToString()
	{
		return $"<RespawnPoint Id={Id} Position={Position}>";
	}
}

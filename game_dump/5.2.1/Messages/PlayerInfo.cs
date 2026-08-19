using MsgPack;
using UnityEngine;

namespace Messages;

public struct PlayerInfo
{
	public string PlayerId;

	public Vector2 Position;

	public static void Pack(Packer packer, PlayerInfo val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.PlayerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerId);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.Position.x);
		packer.Pack(val.Position.y);
	}

	public static PlayerInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayerInfo result = default(PlayerInfo);
		result.PlayerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadSingle(out result.Position.x);
		unpacker.ReadSingle(out result.Position.y);
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerInfo PlayerId={PlayerId} Position={Position}>";
	}
}

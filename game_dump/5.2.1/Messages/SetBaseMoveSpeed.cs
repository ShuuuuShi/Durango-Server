using MsgPack;

namespace Messages;

public struct SetBaseMoveSpeed
{
	public const uint TypeCode = 8749871u;

	public string EntityId;

	public int NormalSpeed;

	public int BattleSpeed;

	public static void Pack(Packer packer, SetBaseMoveSpeed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(8749871u);
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
		packer.Pack(val.NormalSpeed);
		packer.Pack(val.BattleSpeed);
	}

	public static SetBaseMoveSpeed Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetBaseMoveSpeed result = default(SetBaseMoveSpeed);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.NormalSpeed = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.BattleSpeed = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SetBaseMoveSpeed EntityId={EntityId} NormalSpeed={NormalSpeed} BattleSpeed={BattleSpeed}>";
	}
}

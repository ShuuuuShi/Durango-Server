using MsgPack;

namespace Messages;

public struct DefensiveState
{
	public int Atk;

	public float LoadTime;

	public float AtkRange;

	public static void Pack(Packer packer, DefensiveState val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.Atk);
		packer.Pack(val.LoadTime);
		packer.Pack(val.AtkRange);
	}

	public static DefensiveState Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DefensiveState result = default(DefensiveState);
		result.Atk = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.LoadTime = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.AtkRange = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<DefensiveState Atk={Atk} LoadTime={LoadTime} AtkRange={AtkRange}>";
	}
}

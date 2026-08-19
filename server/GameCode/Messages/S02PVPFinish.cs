using MsgPack;

namespace Messages;

public struct S02PVPFinish
{
	public const uint TypeCode = 222220u;

	public int WinnerKillCount;

	public float WinnerSurvivedTime;

	public static void Pack(Packer packer, S02PVPFinish val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(222220u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.WinnerKillCount);
		packer.Pack(val.WinnerSurvivedTime);
	}

	public static S02PVPFinish Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02PVPFinish result = default(S02PVPFinish);
		result.WinnerKillCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.WinnerSurvivedTime = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<S02PVPFinish WinnerKillCount={WinnerKillCount} WinnerSurvivedTime={WinnerSurvivedTime}>";
	}
}

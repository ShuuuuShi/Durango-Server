using MsgPack;

namespace Messages;

public struct BattleLog
{
	public const uint TypeCode = 2043u;

	public string Log;

	public static void Pack(Packer packer, BattleLog val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2043u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Log == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Log);
		}
	}

	public static BattleLog Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BattleLog result = default(BattleLog);
		result.Log = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<BattleLog Log={Log}>";
	}
}

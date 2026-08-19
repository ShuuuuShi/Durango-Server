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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		BattleLog result = default(BattleLog);
		result.Log = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<BattleLog Log={Log}>";
	}
}

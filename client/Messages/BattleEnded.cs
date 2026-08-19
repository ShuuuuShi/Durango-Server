using MsgPack;

namespace Messages;

public struct BattleEnded
{
	public const uint TypeCode = 3587u;

	public string EntityId;

	public double EventAt;

	public static void Pack(Packer packer, BattleEnded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3587u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EventAt);
	}

	public static BattleEnded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BattleEnded result = default(BattleEnded);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<BattleEnded EntityId={EntityId} EventAt={EventAt}>";
	}
}

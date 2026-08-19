using MsgPack;

namespace Messages;

public struct BattleNotification
{
	public string EntityId;

	public double EventAt;

	public static void Pack(Packer packer, BattleNotification val, bool hint = false)
	{
		packer.PackArrayHeader(2);
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

	public static BattleNotification Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BattleNotification result = default(BattleNotification);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<BattleNotification EntityId={EntityId} EventAt={EventAt}>";
	}
}

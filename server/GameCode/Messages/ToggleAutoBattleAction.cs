using MsgPack;

namespace Messages;

public struct ToggleAutoBattleAction
{
	public const uint TypeCode = 3491u;

	public bool Active;

	public static void Pack(Packer packer, ToggleAutoBattleAction val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3491u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Active);
	}

	public static ToggleAutoBattleAction Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ToggleAutoBattleAction result = default(ToggleAutoBattleAction);
		result.Active = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ToggleAutoBattleAction Active={Active}>";
	}
}

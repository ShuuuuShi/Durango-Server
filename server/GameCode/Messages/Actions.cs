using MsgPack;

namespace Messages;

public struct Actions
{
	public const uint TypeCode = 315u;

	public ActionStatus[] BattleActions;

	public static void Pack(Packer packer, Actions val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(315u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.BattleActions == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.BattleActions.Length);
		for (int i = 0; i < val.BattleActions.Length; i++)
		{
			ActionStatus.Pack(packer, val.BattleActions[i]);
		}
	}

	public static Actions Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Actions result = default(Actions);
		result.BattleActions = new ActionStatus[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref ActionStatus reference = ref result.BattleActions[i];
			reference = ActionStatus.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Actions BattleActions={BattleActions}>";
	}
}

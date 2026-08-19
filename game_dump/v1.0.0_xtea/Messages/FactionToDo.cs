using MsgPack;

namespace Messages;

public struct FactionToDo
{
	public string Label;

	public int Progress;

	public int GoalCount;

	public string Tooltip;

	public static void Pack(Packer packer, FactionToDo val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.PackString(val.Label);
		packer.Pack(val.Progress);
		packer.Pack(val.GoalCount);
		if (val.Tooltip == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.Tooltip);
		}
	}

	public static FactionToDo Unpack(Unpacker unpacker)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		FactionToDo result = default(FactionToDo);
		result.Label = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Progress = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.GoalCount = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.Tooltip = null;
		}
		else
		{
			string tooltip = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.Tooltip = tooltip;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<FactionToDo Label={Label} Progress={Progress} GoalCount={GoalCount} Tooltip={Tooltip}>";
	}
}
